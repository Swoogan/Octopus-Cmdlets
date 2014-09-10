using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "Environment")]
    public class RemoveEnvironment : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The name of the environment to remove.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The id of the environment to remove.")]
        [Alias("EnvironmentId")]
        public string[] Id { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
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

        private void ProcessById()
        {
            var environments = from id in Id
                         select _octopus.Environments.Get(id);

            foreach (var environment in environments)
            {
                WriteVerbose("Deleting environment: " + environment.Name);
                _octopus.Environments.Delete(environment);
            }
        }

        protected void ProcessByName()
        {
            var environments = _octopus.Environments.FindByNames(Name);

            foreach (var environment in environments)
            {
                WriteVerbose("Deleting environment: " + environment.Name);
                _octopus.Environments.Delete(environment);
            }
        }
    }
}
