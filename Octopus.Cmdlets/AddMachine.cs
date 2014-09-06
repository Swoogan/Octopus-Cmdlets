using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Machine", DefaultParameterSetName = "ByName")]
    public class AddMachine : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true)]
        public string[] EnvironmentName { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true)]
        public string[] EnvironmentId { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true)]
        public string Thumbprint { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = true)]
        public string[] Roles { get; set; }

        [Parameter(
            Position = 4,
            Mandatory = true)]
        public string Uri { get; set; }

        [Parameter(
            Position = 5,
            Mandatory = true)]
        [ValidateSet("TentacleActive", "TentaclePassive")]
        public string CommunicationStyle { get; set; }

        //[Parameter]
        //public string HostName { get; set; }
        //[Parameter]
        //public string Port { get; set; }

        private OctopusRepository _octopus;
        private EnvironmentResource _environment;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");

            if (ParameterSetName != "ByName") return;

            _environment = _octopus.Environments.FindByName(EnvironmentName);
            if (_environment == null)
                throw new Exception(string.Format("Environment '{0}' was not found.", EnvironmentName));
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
            //var projects = Name.Select(name => new MachineResource
            //{
            //    Name = name,
            //    EnvironmentIds = new ReferenceCollection(_environment.Id)
            //});

            //foreach (var project in projects)
            //    _octopus.Projects.Create(project);
        }

        private void ProcessById()
        {
            //var projects = Name.Select(name => new MachineResource
            //{
            //    Name = name,
            //    EnvironmentIds = EnvironmentId
            //});

            //foreach (var project in projects)
            //    _octopus.Projects.Create(project);
        }
    }
}
