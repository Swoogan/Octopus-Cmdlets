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
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "VariableSet", DefaultParameterSetName = "ByName")]
    public class RemoveVariableSet : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public string Id { get; set; }

        [Parameter(
            ParameterSetName = "ByObject",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public LibraryVariableSetResource InputObject { get; set; }

        private IOctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    ProcessByName();
                    break;
                case "ById":
                    ProcessById();
                    break;
                case "ByObject":
                    ProcessByObject();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByObject()
        {
            _octopus.LibraryVariableSets.Delete(InputObject);
        }

        private void ProcessById()
        {
            var set = _octopus.LibraryVariableSets.Get(Id);
            _octopus.LibraryVariableSets.Delete(set);
        }

        private void ProcessByName()
        {
            var set = _octopus.LibraryVariableSets.FindOne(vs => vs.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            WriteVerbose("Deleting variableset: " + set.Name);
            _octopus.LibraryVariableSets.Delete(set);
        }
    }
}
