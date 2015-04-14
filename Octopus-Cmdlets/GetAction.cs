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

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get an action role from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoAction cmdlet gets an action from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octoaction Project</code>
    ///   <para>
    ///      Get all the actions in the project named 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Action", DefaultParameterSetName = "ByName")]
    public class GetAction : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The project to get the action for.</para>
        /// </summary>
        [Parameter(
             Position = 0,
             Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the action to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the action to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Id { get; set; }

        private IOctopusRepository _octopus;
        private DeploymentProcessResource _deploymentProcess;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // Find the project that owns the variables we want to get
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was not found.";
                throw new Exception(string.Format(msg, Project));
            }

            var id = project.DeploymentProcessId;
            _deploymentProcess = _octopus.DeploymentProcesses.Get(id);
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

        private void ProcessByName()
        {
            var actions = Name == null
                ? _deploymentProcess.Steps.SelectMany(step => step.Actions)
                : (from step in _deploymentProcess.Steps
                    from action in step.Actions
                    from name in Name
                    where action.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    select action);

            foreach (var action in actions)
                WriteObject(action);
        }

        private void ProcessById()
        {
            var actions = from step in _deploymentProcess.Steps
                    from action in step.Actions
                    from id in Id
                    where action.Id == id
                    select action;

            foreach (var action in actions)
                WriteObject(action);
        }
    }
}
