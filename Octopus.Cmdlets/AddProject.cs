using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Project", DefaultParameterSetName = "ByName")]
    public class AddProject : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The name of the ProjectGroup to create the project in.")]
        [Alias("GroupName", "ProjectGroup")]
        public string ProjectGroupName { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The id of the ProjectGroup to create the project in.")]
        [Alias("GroupId")]
        public string ProjectGroupId { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to create.")]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The description of the project to create.")]
        public string Description { get; set; }

        private OctopusRepository _octopus;
        private string _projectGroupId;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

            if (ParameterSetName != "ByName") return;

            var projectGroup = _octopus.ProjectGroups.FindByName(ProjectGroupName);
            if (projectGroup == null)
                throw new Exception(string.Format("Project '{0}' was not found.", ProjectGroupName));

            _projectGroupId = projectGroup.Id;
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    CreateProject(_projectGroupId);
                    break;
                case "ById":
                    CreateProject(ProjectGroupId);
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void CreateProject(string projectGroupId)
        {
            _octopus.Projects.Create(new ProjectResource
            {
                Name = Name,
                Description = Description,
                ProjectGroupId = projectGroupId
            });
        }
    }
}
