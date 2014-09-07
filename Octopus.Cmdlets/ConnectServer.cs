using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommunications.Connect, "Server")]
    public class ConnectServer : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "The address of the Octopus Deploy server you want to connect to.")]
        public string Server { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            HelpMessage = "The generated ApiKey for the profile you wish to connect as.")]
        public string ApiKey { get; set; }

        protected override void ProcessRecord()
        {
            var octopusServerEndpoint = new OctopusServerEndpoint(Server, ApiKey);
            var octopus = new OctopusRepository(octopusServerEndpoint);

            SessionState.PSVariable.Set("OctopusRepository", octopus);
        }
    }
}
