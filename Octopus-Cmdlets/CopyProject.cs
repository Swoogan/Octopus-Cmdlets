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
using Octopus.Client.Model;
using Octopus.Platform.Model;
using Octopus.Platform.Util;
using Octopus_Cmdlets.Utilities;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Copy a project in the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoProject cmdlet copies a project in the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>copy-octoproject -Name Example -Destination "Example - Copy" -ProjectGroup ExampleGroup</code>
    ///   <para>
    ///      Make a copy of the project 'Example', name it 'Example - Copy' and place
    ///      it in the project group named 'ExampleGroup'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Copy, "Project")]
    public class CopyProject : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to copy.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        public string Name { get; set; }

        //[Parameter(
        //    ParameterSetName = "ById",
        //    Mandatory = true,
        //    ValueFromPipelineByPropertyName = true,
        //    ValueFromPipeline = true,
        //    HelpMessage = "The id of the project to remove.")]
        //[Alias("ProjectId")]
        //public string[] Id { get; set; }
      
        //[Parameter(
        //    Position = 2,
        //    Mandatory = true,
        //    HelpMessage = "The id of the ProjectGroup to create the project in.")]
        //[Alias("GroupId")]
        //public string ProjectGroupId { get; set; }

        /// <summary>
        /// <para type="description">The destination name of the new project.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        /// <summary>
        /// <para type="description">The name of the ProjectGroup to create the project in.</para>
        /// </summary>
        [Parameter(
          Position = 2,
          Mandatory = true)]
        public string ProjectGroup { get; set; }

        private IOctopusRepository _octopus;
        private ProjectResource _oldProject;
        private ProjectResource _newProject;
        private DeploymentProcessResource _oldProcess;
        private DeploymentProcessResource _newProcess;

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
            _oldProject = _octopus.Projects.FindByName(Name);

            if (_oldProject == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Name));

            var group = _octopus.ProjectGroups.FindByName(ProjectGroup);

            if (group == null)
                throw new Exception(string.Format("Project Group '{0}' was not found.", ProjectGroup));

            CreateNewProject(group.Id);

            _oldProcess = _octopus.DeploymentProcesses.Get(_oldProject.DeploymentProcessId);
            _newProcess = _octopus.DeploymentProcesses.Get(_newProject.DeploymentProcessId);

            CopyProcess();
            CopyVariables();
        }

        private void CreateNewProject(string groupId)
        {
            WriteVerbose(string.Format("Creating the project '{0}'...", Destination));

            _newProject = _octopus.Projects.Create(new ProjectResource
            {
                Name = Destination,
                Description = _oldProject.Description,
                ProjectGroupId = groupId,
                DefaultToSkipIfAlreadyInstalled = _oldProject.DefaultToSkipIfAlreadyInstalled,
                IncludedLibraryVariableSetIds = _oldProject.IncludedLibraryVariableSetIds,
                VersioningStrategy = _oldProject.VersioningStrategy,
                AutoCreateRelease = _oldProject.AutoCreateRelease,
                ReleaseCreationStrategy = _oldProject.ReleaseCreationStrategy,
                IsDisabled = _oldProject.IsDisabled,
                LifecycleId = _oldProject.LifecycleId
            });

            WriteVerbose(string.Format("Project '{0}' created.", Destination));
        }

        private void CopyVariables()
        {
            var oldSet = _octopus.VariableSets.Get(_oldProject.VariableSetId);
            var newSet = _octopus.VariableSets.Get(_newProject.VariableSetId);

            var copier = new Variables(newSet.Variables, WriteWarning);
            copier.CopyVariables(oldSet.Variables, CopyActionScope);

            WriteVerbose("Saving the variable set.");
            _octopus.VariableSets.Modify(newSet);
        }

        private ScopeValue CopyActionScope(KeyValuePair<ScopeField, ScopeValue> scope)
        {
            var results = new List<string>();

            foreach (var value in scope.Value)
            {
                var innerValue = value;

                // find old action name
                var actionNames = 
                    from step in _oldProcess.Steps
                    from action in step.Actions
                    where action.Id == innerValue
                    select action.Name;

                // should only ever be zero or one
                var ids = from name in actionNames
                    from step in _newProcess.Steps
                    from action in step.Actions
                    where action.Name == name
                    select action.Id;

                results.AddRange(ids);
            }

            return new ScopeValue(results);
        }

        private void CopyProcess()
        {
            foreach (var step in _oldProcess.Steps)
            {
                var newStep = new DeploymentStepResource
                {
                    Name = step.Name,
                    Condition = step.Condition,
                    RequiresPackagesToBeAcquired = step.RequiresPackagesToBeAcquired,
                };

                newStep.Properties.AddRange(step.Properties);
                newStep.SensitiveProperties.AddRange(step.SensitiveProperties);

                CopyActions(step, newStep);

                _newProcess.Steps.Add(newStep);
            }
        }

        private static void CopyActions(DeploymentStepResource step, DeploymentStepResource newStep)
        {
            foreach (var action in step.Actions)
            {
                var newAction = new DeploymentActionResource
                {
                    Name = action.Name,
                    ActionType = action.ActionType,
                };

                newAction.Environments.AddRange(action.Environments);
                newAction.Properties.AddRange(action.Properties);
                newAction.SensitiveProperties.AddRange(action.SensitiveProperties);

                newStep.Actions.Add(newAction);
            }
        }

        /// <summary>
        /// EndProcessing
        /// </summary>
        protected override void EndProcessing()
        {
            WriteVerbose("Saving the deployment process...");
            _newProcess = _octopus.DeploymentProcesses.Modify(_newProcess);
            WriteVerbose("Deployment process saved.");
        }
    }
}
