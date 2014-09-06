using System;
using System.Linq;
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
            Mandatory = true)]
        public string GroupName { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true)]
        public string GroupId { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        private OctopusRepository _octopus;
        private string _projectGroupId;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");

            if (ParameterSetName != "ByName") return;

            var projectGroup = _octopus.ProjectGroups.FindByName(GroupName);
            if (projectGroup == null)
                throw new Exception(string.Format("Project '{0}' was not found.", GroupName));

            _projectGroupId = projectGroup.Id;
        }

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
            }
        }

        private void ProcessByName()
        {
            var projects = Name.Select(name => new ProjectResource
            {
                Name = name,
                ProjectGroupId = _projectGroupId
            });

            foreach (var project in projects)
                _octopus.Projects.Create(project);
        }

        private void ProcessById()
        {
            var projects = Name.Select(name => new ProjectResource
            {
                Name = name,
                ProjectGroupId = GroupId
            });

            foreach (var project in projects)
                _octopus.Projects.Create(project);
        }
    }
}
