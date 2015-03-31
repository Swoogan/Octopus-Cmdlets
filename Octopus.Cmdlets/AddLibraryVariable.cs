#region License
// Copyright 2014 Colin Svingen

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Extensions;
using Octopus.Platform.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "LibraryVariable", 
        DefaultParameterSetName = "ByObject")]
    public class AddLibraryVariable : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Library VariableSet to add the variable to.")]
        public string VariableSet { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Value { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Environments { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Roles { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Machines { get; set; }

        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Sensitive { get; set; }

        [Parameter(
            ParameterSetName = "ByObject",
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true)]
        public VariableResource[] InputObject { get; set; }

        private IOctopusRepository _octopus;
        private VariableSetResource _variableSet;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            var libraryVariableSet =
                _octopus.LibraryVariableSets.FindOne(
                    v => v.Name.Equals(VariableSet, StringComparison.InvariantCultureIgnoreCase));

            if (libraryVariableSet == null)
                throw new Exception(string.Format("Library variable set '{0}' was not found.", VariableSet));

            _variableSet = _octopus.VariableSets.Get(libraryVariableSet.Link("Variables"));
            WriteDebug("Found variable set" + _variableSet.Id);
        }

        protected override void ProcessRecord()
        {
            WriteDebug("ParameterSetName: " + ParameterSetName);

            switch (ParameterSetName)
            {
                case "ByParts":
                    ProcessByParts();
                    break;

                case "ByObject":
                    ProcessByObject();
                    break;

                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByObject()
        {
            var copier = new Variables(_variableSet.Variables, WriteWarning);
            copier.CopyVariables(InputObject);
        }

        private void ProcessByParts()
        {
            var variable = new VariableResource { Name = Name, Value = Value, IsSensitive = Sensitive };
            
            AddEnvironments(variable);
            AddMachines(variable);

            if (Roles != null && Roles.Length > 0)
                variable.Scope.Add(ScopeField.Role, new ScopeValue(Roles));

            _variableSet.Variables.Add(variable);
        }

        private void AddEnvironments(VariableResource variable)
        {
            var environments = _octopus.Environments.FindByNames(Environments);
            var ids = environments.Select(environment => environment.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Environment, new ScopeValue(ids));
        }

        private void AddMachines(VariableResource variable)
        {
            var machines = _octopus.Machines.FindByNames(Machines);
            var ids = machines.Select(m => m.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(ids));
        }

        protected override void EndProcessing()
        {
            _octopus.VariableSets.Modify(_variableSet);
            WriteVerbose("Modified the variable set");
        }
    }
}
