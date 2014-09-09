using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Action", DefaultParameterSetName = "ByName")]
    public class GetAction : PSCmdlet
    {
        [Parameter(
             Position = 0,
             Mandatory = true,
             HelpMessage = "The project to get the action for."
             )]
        public string Project { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the action to retrieve.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the action to retrieve.")]
        public string[] Id { get; set; }

        private OctopusRepository _octopus;
        private DeploymentProcessResource _deploymentProcess;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

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
            if (Name == null)
            {
                foreach (var step in _deploymentProcess.Steps)
                    WriteObject(step);
            }
            else
            {
                var steps = from step in _deploymentProcess.Steps
                    from name in Name
                    where step.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    select step;

                foreach (var step in steps)
                    WriteObject(step);
            }
        }

        private void ProcessById()
        {
            if (Id == null)
            {
                foreach (var step in _deploymentProcess.Steps)
                    WriteObject(step);
            }
            else
            {
                var steps = from step in _deploymentProcess.Steps
                            from id in Id
                            where step.Id == id
                            select step;

                foreach (var step in steps)
                    WriteObject(step);
            }
        }
    }
}
