using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;
using Octopus.Platform.Util;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Copy, "Project")]
    public class CopyProject : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The name of the project to copy.")]
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

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to create.")]
        public string Destination { get; set; }

        [Parameter(
          Position = 2,
          Mandatory = true,
          HelpMessage = "The name of the ProjectGroup to create the project in.")]
        public string ProjectGroup { get; set; }

        private OctopusRepository _octopus;
        private ProjectResource _oldProject;
        private ProjectResource _newProject;
        private DeploymentProcessResource _oldProcess;
        private DeploymentProcessResource _newProcess;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

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
                IsDisabled = _oldProject.IsDisabled,
                LifecycleId = _oldProject.LifecycleId
            });

            WriteVerbose(string.Format("Project '{0}' created.", Destination));
        }

        private void CopyVariables()
        {
            var oldSet = _octopus.VariableSets.Get(_oldProject.VariableSetId);
            var newSet = _octopus.VariableSets.Get(_newProject.VariableSetId);

            foreach (var variable in oldSet.Variables)
            {
                if (variable.IsSensitive)
                {
                    const string warning = "Variable '{0}' was sensitive. Sensitive flag has been removed and the value has been set to an empty string.";
                    WriteWarning(string.Format(warning, variable.Name));
                }

                newSet.Variables.Add(new VariableResource
                {
                    Name = variable.Name,
                    IsEditable = variable.IsEditable,
                    IsSensitive = false,
                    Prompt = variable.Prompt,
                    Value = variable.IsSensitive ? "" : variable.Value,
                    Scope = CopyScopeSpec(variable.Scope)
                });
            }

            WriteVerbose("Saving the variable set.");
            _octopus.VariableSets.Modify(newSet);
        }

        private ScopeSpecification CopyScopeSpec(ScopeSpecification scopeSpec)
        {
            var newScopeSpec = new ScopeSpecification();

            foreach (var scope in scopeSpec)
                newScopeSpec.Add(scope.Key, CopyScope(scope));

            return newScopeSpec;
        }

        private ScopeValue CopyScope(KeyValuePair<ScopeField, ScopeValue> scope)
        {
            if (scope.Key != ScopeField.Action) 
                return new ScopeValue(scope.Value);

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

        protected override void EndProcessing()
        {
            WriteVerbose("Saving the deployment process...");
            _newProcess = _octopus.DeploymentProcesses.Modify(_newProcess);
            WriteVerbose("Deployment process saved.");
        }
    }
}
