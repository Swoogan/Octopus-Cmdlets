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
using Octopus.Client.Exceptions;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a deployment process from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoDeploymentProcess cmdlet gets a deployment process from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octodeploymentprocess Project</code>
    ///   <para>
    ///      Get the deployment process for the project 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "DeploymentProcess", DefaultParameterSetName = "ById")]
    public class GetDeploymentProcess : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to retrieve the process for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProjectName",
            Mandatory = true)]
        public string[] Project { get; set; }

        //[Parameter(
        //    ParameterSetName = "ByProjectId",
        //    Mandatory = true,
        //    ValueFromPipeline = true,
        //    ValueFromPipelineByPropertyName = true,
        //    HelpMessage = "The id of the project to retrieve the process for.")]
        //public string[] ProjectId { get; set; }

        /// <summary>
        /// <para type="description">The id of the deployment process to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string[] DeploymentProcessId { get; set; }

        private IOctopusRepository _octopus;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Connection established");
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByProjectName":
                    ProcessByProjectName();
                    break;
                //case "ByProjectId":
                //    ProcessByProjectId();
                //    break;
                case "ById":
                    ProcessById();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByProjectName()
        {
            var projects = _octopus.Projects.FindByNames(Project);
            var processes = projects.Select(p => _octopus.DeploymentProcesses.Get(p.DeploymentProcessId));

            foreach (var process in processes)
                WriteObject(process);
        }


        private void ProcessById()
        {
            foreach (var id in DeploymentProcessId)
                GetProcess(id);
        }

        private void GetProcess(string id)
        {
            try
            {
                WriteObject(_octopus.DeploymentProcesses.Get(id));
            }
            catch (OctopusResourceNotFoundException)
            {
                WriteDebug(string.Format("Deployment process id '{0}' not found", id));
            }
        }
    }
}
