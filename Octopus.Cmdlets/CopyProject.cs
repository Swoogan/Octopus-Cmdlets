using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
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

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            var oldProject = _octopus.Projects.FindByName(Name);

            if (oldProject == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Name));

            var group = _octopus.ProjectGroups.FindByName(ProjectGroup);

            if (group == null)
                throw new Exception(string.Format("Project Group '{0}' was not found.", ProjectGroup));

            var newProject = new ProjectResource
            {
                Name = Destination,
                Description = oldProject.Description,
                ProjectGroupId = group.Id,
                DefaultToSkipIfAlreadyInstalled = oldProject.DefaultToSkipIfAlreadyInstalled,
                IncludedLibraryVariableSetIds = oldProject.IncludedLibraryVariableSetIds,
                VersioningStrategy = oldProject.VersioningStrategy,
                IsDisabled = oldProject.IsDisabled
            };

            WriteVerbose("Creating the project.");
            var result = _octopus.Projects.Create(newProject);
            
            CopyVariables(oldProject, result);
            CopyProcess(oldProject, result);
        }

        private void CopyVariables(ProjectResource oldProject, ProjectResource result)
        {
            var oldSet = _octopus.VariableSets.Get(oldProject.VariableSetId);
            var newSet = _octopus.VariableSets.Get(result.VariableSetId);

            foreach (var variable in oldSet.Variables)
            {
                newSet.Variables.Add(new VariableResource
                {
                    Name = variable.Name,
                    IsEditable = variable.IsEditable,
                    IsSensitive = variable.IsSensitive,
                    Prompt = variable.Prompt,
                    Value = variable.Value,
                    Scope = variable.Scope
                });
            }

            WriteVerbose("Saving the variable set.");
            _octopus.VariableSets.Modify(newSet);
        }

        private void CopyProcess(ProjectResource oldProject, ProjectResource result)
        {
            var process = _octopus.DeploymentProcesses.Get(oldProject.DeploymentProcessId);
            var newProcess = _octopus.DeploymentProcesses.Get(result.DeploymentProcessId);

            CopySteps(process, newProcess);

            WriteVerbose("Saving the deployment process.");
            _octopus.DeploymentProcesses.Modify(newProcess);
        }

        private static void CopySteps(DeploymentProcessResource process, DeploymentProcessResource newProcess)
        {
            foreach (var step in process.Steps)
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

                newProcess.Steps.Add(newStep);
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
    }
}
