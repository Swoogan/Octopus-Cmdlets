using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "Project", DefaultParameterSetName = "ByName")]
    public class RemoveProject : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The name of the project to remove.")]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The id of the project to remove.")]
        public string Id { get; set; }

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

        private void ProcessById()
        {
            var project = _octopus.Projects.Get(Id);
            _octopus.Projects.Delete(project);
        }

        private void ProcessByName()
        {
            var project = _octopus.Projects.FindByName(Name);
            _octopus.Projects.Delete(project);
        }
    }
}
