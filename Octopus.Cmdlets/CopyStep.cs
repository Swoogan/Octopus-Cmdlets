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
using Octopus.Platform.Util;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Copy, "Step")]
    public class CopyStep : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to get the step for.")]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the step to copy.")]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The new name of the step.")]
        public string Destination { get; set; }

        private IOctopusRepository _octopus;
        private DeploymentProcessResource _deploymentProcess;
        private DeploymentStepResource _step;

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

            var steps = from s in _deploymentProcess.Steps
                        where s.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                        select s;

            _step = steps.FirstOrDefault();

            if (_step == null)
                throw new Exception(string.Format("Step with name '{0}' was not found.", Name));
        }

        protected override void ProcessRecord()
        {
            var clone = new DeploymentStepResource
            {
                Name = GetName(_step.Name),
                Condition = _step.Condition,
                RequiresPackagesToBeAcquired = _step.RequiresPackagesToBeAcquired,
            };

            clone.Properties.AddRange(_step.Properties);
            clone.SensitiveProperties.AddRange(_step.SensitiveProperties);

            CopyActions(_step, clone);

            _deploymentProcess.Steps.Add(clone);
        }

        private string GetName(string name)
        {
            return String.IsNullOrWhiteSpace(Destination) ? name + " - Copy" : Destination;
        }

        private void CopyActions(DeploymentStepResource step, DeploymentStepResource newStep)
        {
            foreach (var action in step.Actions)
            {
                var newAction = new DeploymentActionResource
                {
                    Name = GetName(action.Name),
                    ActionType = action.ActionType,
                };

                newAction.Environments.AddRange(action.Environments);
                newAction.Properties.AddRange(action.Properties);
                newAction.SensitiveProperties.AddRange(action.SensitiveProperties);

                newStep.Actions.Add(newAction);
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose("Saving the deployment process...");
            _octopus.DeploymentProcesses.Modify(_deploymentProcess);
            WriteVerbose("Deployment process saved.");
        }
    }
}
