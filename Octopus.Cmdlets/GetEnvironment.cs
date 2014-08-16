using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Environment", DefaultParameterSetName = "ByName")]
    public class GetEnvironment : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the envrionment to look for.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The id of the environment to look for.")]
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
            if (Name != null)
            {
                foreach (var environment in _octopus.Environments.FindByNames(Name))
                    WriteObject(environment);
            }
            else if (Id != null)
            {
                foreach (var environment in _octopus.Environments.FindMany(e => e.Id == Id))
                    WriteObject(environment);
            }
            else
            {
                foreach (var environment in _octopus.Environments.FindAll())
                    WriteObject(environment);
            }
        }
    }
}
