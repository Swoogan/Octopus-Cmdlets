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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a deployment step for an Octopus Deploy project.</para>
    /// <para type="description">The Get-OctoStep cmdlet gets a deployment step for an Octopus Deploy project.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octostep Project</code>
    ///   <para>
    ///      Get all deployment steps for the project 'Project'.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>get-octostep -ProjectId projects-1</code>
    ///   <para>
    ///      Get all deployment steps for the project with the id 'projects-1'.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>get-octostep -DeploymentProcessId deploymentprocess-1</code>
    ///   <para>
    ///      Get all deployment steps for the deploymentprocess with the id 'projects-1'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Step", DefaultParameterSetName = "ByProjectName")]
    public class GetStep : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to retrieve the step for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProjectName",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The name of the project to retrieve the process for.")]
        public string[] Project { get; set; }

        /// <summary>
        /// <para type="description">The id of the project to retrieve the step for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProjectId",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The id of the project to retrieve the process for.")]
        public string[] ProjectId { get; set; }

        /// <summary>
        /// <para type="description">The id of the deployment process to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByDeploymentProcessId",
            Position = 0,
            Mandatory = true)]
        public string[] DeploymentProcessId { get; set; }

        /// <summary>
        /// <para type="description">The name of the step to retrieve.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }
         
        private IOctopusRepository _octopus;
        private List<ProjectResource> _projects;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Connection established");

            switch (ParameterSetName)
            {
                case "ByProjectName":
                    _projects = _octopus.Projects.FindByNames(Project);
                    break;
                case "ByProjectId":
                    LoadProjectsByIds();
                    break;
                case "ByDeploymentProcessId":
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void LoadProjectsByIds()
        {
            _projects = new List<ProjectResource>();
            foreach (var id in ProjectId)
            {
                try
                {
                    _projects.Add(_octopus.Projects.Get(id));
                }
                catch (OctopusResourceNotFoundException)
                {
                    WriteDebug(string.Format("Project id '{0}' was not found", id));
                }
            }
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByProjectName":
                case "ByProjectId":
                    ProcessByProject();
                    break;
                case "ByDeploymentProcessId":
                    ProcessByDeploymentProcessId();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByProject()
        {
            var steps = from p in _projects
                            let proc = _octopus.DeploymentProcesses.Get(p.DeploymentProcessId)
                            from s in GetSteps(proc.Steps)
                            select s;

            foreach (var step in steps)
                WriteObject(step);
        }

        private void ProcessByDeploymentProcessId()
        {
            foreach (var id in DeploymentProcessId)
            {
                try
                {
                    var process = _octopus.DeploymentProcesses.Get(id);

                    foreach (var step in GetSteps(process.Steps))
                        WriteObject(step);
                }
                catch (OctopusResourceNotFoundException)
                {
                    WriteDebug(string.Format("Deployment process id '{0}' was not found", id));
                }
            }
        }

        private IEnumerable<DeploymentStepResource> GetSteps(IList<DeploymentStepResource> steps)
        {
            return Name == null
                ? steps
                : from n in Name
                    from s in steps
                    where n.Equals(s.Name, StringComparison.InvariantCultureIgnoreCase)
                    select s;
        }
    }
}
