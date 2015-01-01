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
                var actions = _deploymentProcess.Steps.SelectMany(step => step.Actions);
                foreach (var action in actions)
                    WriteObject(action);
            }
            else
            {
                var actions = from step in _deploymentProcess.Steps
                            from action in step.Actions
                            from name in Name
                            where action.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                            select action;

                foreach (var action in actions)
                    WriteObject(action);
            }
        }

        private void ProcessById()
        {
            if (Id == null)
            {
                var actions = _deploymentProcess.Steps.SelectMany(step => step.Actions);
                foreach (var action in actions)
                    WriteObject(action);
            }
            else
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
}
