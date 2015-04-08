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

using Octopus.Client;
using Octopus.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a variable from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoVariable cmdlet gets a variable from the Octopus Deploy server.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Variable", DefaultParameterSetName = "Project")]
    public class GetVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The project to get the variables for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "Project",
            Position = 0,
            Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the variable to retrieve.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The library variable set to get the variables for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "VariableSet",
            Position = 0,
            Mandatory = true)]
        public string[] VariableSet { get; set; }

        /// <summary>
        /// <para type="description">"The id of the library variable set to get the variables for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "VariableSetId",
            Position = 0,
            Mandatory = true)]
        public string[] VariableSetId { get; set; }

        private readonly List<VariableSetResource> _variableSets = new List<VariableSetResource>();

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            var octopus = Session.RetrieveSession(this);

            switch (ParameterSetName)
            {
                case "Project":
                    LoadProjectVariableSet(octopus);
                    break;
                case "VariableSet":
                    LoadLibraryVariableSetByNames(octopus);
                    break;
                case "VariableSetId":
                    LoadLibraryVariableSetByIds(octopus);
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void LoadLibraryVariableSetByIds(IOctopusRepository octopus)
        {
            // Find the library variable sets that owns the variables we want to get
            foreach (var id in VariableSetId)
            {
                var idForClosure = id;
                var variableSet = octopus.LibraryVariableSets.FindOne(vs =>
                    vs.Id == idForClosure);

                if (variableSet == null)
                    WriteWarning(string.Format("Library variable set with id '{0}' was not found.", id));
                else
                    _variableSets.Add(octopus.VariableSets.Get(variableSet.Link("Variables")));
            }
        }

        private void LoadLibraryVariableSetByNames(IOctopusRepository octopus)
        {
            // Find the library variable sets that owns the variables we want to get
            foreach (var name in VariableSet)
            {
                var nameForClosure = name;
                var variableSet = octopus.LibraryVariableSets.FindOne(vs =>
                    vs.Name.Equals(nameForClosure, StringComparison.InvariantCultureIgnoreCase));

                if (variableSet == null)
                    WriteWarning(string.Format("Library variable set '{0}' was not found.", name));
                else
                    _variableSets.Add(octopus.VariableSets.Get(variableSet.Link("Variables")));
            }
        }

        private void LoadProjectVariableSet(IOctopusRepository octopus)
        {
            // Find the project that owns the variables we want to get
            var project = octopus.Projects.FindByName(Project);

            if (project == null)
            {
                throw new Exception(string.Format("Project '{0}' was not found.", Project));
            }

            // Get the variable set
            _variableSets.Add(octopus.VariableSets.Get(project.Link("Variables")));
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            var variables = Name == null
                ? _variableSets.SelectMany(v => v.Variables)
                : (from name in Name
                    from variableSet in _variableSets
                    from variable in variableSet.Variables
                    where variable.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    select variable);

            foreach (var variable in variables)
                WriteObject(variable);
        }
    }
}
