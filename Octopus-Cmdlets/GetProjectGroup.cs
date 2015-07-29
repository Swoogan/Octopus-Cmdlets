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
using Octopus.Client.Exceptions;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a project group from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoProjectGroup cmdlet gets a project group from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octoprojectgroup</code>
    ///   <para>
    ///      Get all the project groups.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "ProjectGroup", DefaultParameterSetName = "ByName")]
    public class GetProjectGroup : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project group to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project group to retrieve.")]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the project group to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the project group to retrieve.")]
        [Alias("Id")]
        public string[] ProjectGroupId { get; set; }

        private IOctopusRepository _octopus;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
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
            foreach (var id in ProjectGroupId)
                OutputGroup(id);
        }

        private void OutputGroup(string id)
        {
            try
            {
                WriteObject(_octopus.ProjectGroups.Get(id));
            }
            catch (OctopusResourceNotFoundException)
            {
            }
        }

        private void ProcessByName()
        {
            var groups = Name == null ?
                _octopus.ProjectGroups.FindAll() :
                _octopus.ProjectGroups.FindByNames(Name);

            foreach (var group in groups)
                WriteObject(group);
        }
    }
}
