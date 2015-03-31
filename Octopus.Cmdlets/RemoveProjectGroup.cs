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

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "ProjectGroup", DefaultParameterSetName = "ByName")]
    public class RemovedProjectGroup : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project group to remove.")]
        [Alias("GroupName")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the project to retrieve.")]
        [Alias("ProjectGroupId")]
        public string[] Id { get; set; }

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
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessById()
        {
            var groups = from id in Id
                         select _octopus.ProjectGroups.Get(id);

            foreach (var group in groups)
            {
                WriteVerbose("Deleting project group: " + group.Name);
                _octopus.ProjectGroups.Delete(group);
            }
        }

        protected void ProcessByName()
        {
            var groups = _octopus.ProjectGroups.FindByNames(Name);

            foreach (var group in groups)
            {
                WriteVerbose("Deleting project group: " + group.Name);
                _octopus.ProjectGroups.Delete(group);
            }
        }
    }
}
