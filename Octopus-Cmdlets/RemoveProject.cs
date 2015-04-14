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
    /// <para type="synopsis">Remove a project from the Octopus Deploy server.</para>
    /// <para type="description">The Remove-OctoProject cmdlet removes a project from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>remove-octoproject Project</code>
    ///   <para>
    ///      Remove the project named 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "Project", DefaultParameterSetName = "ByName")]
    public class RemoveProject : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the project to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        [Alias("ProjectId")]
        public string[] Id { get; set; }

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
            foreach (var id in Id)
            {
                try
                {
                    var project = _octopus.Projects.Get(id);
                    WriteVerbose("Deleting project: " + project.Name);
                    _octopus.Projects.Delete(project);
                }
                catch (OctopusResourceNotFoundException)
                {
                    WriteWarning(string.Format("A project with the id '{0}' does not exist.", id));
                }
            }
        }

        private void ProcessByName()
        {
            foreach (var name in Name)
            {
                var project = _octopus.Projects.FindByName(name);
                if (project != null)
                {
                    WriteVerbose("Deleting project: " + project.Name);
                    _octopus.Projects.Delete(project);
                }
                else
                {
                    WriteWarning(string.Format("The project '{0}' does not exist.", name));
                }
            }
        }
    }
}
