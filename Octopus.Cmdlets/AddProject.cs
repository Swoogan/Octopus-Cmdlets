using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Project")]
    public class AddProject : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true)]
        public string ProjectGroupId { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true)]
        public string Name { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
        }

        protected override void ProcessRecord()
        {
            var project = new ProjectResource
            {
                Name = Name,
                ProjectGroupId = ProjectGroupId
            };
            
            _octopus.Projects.Create(project);
        }
    }
}
