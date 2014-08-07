using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommunications.Connect, "Server")]
    public class ConnectServer : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0
            )]
        public string Server { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1
            )]
        public string ApiKey { get; set; }

        protected override void ProcessRecord()
        {
            //var octopusServerEndpoint = new OctopusServerEndpoint("http://aprdappvm030:81/", "API-IQJURANHERTYDKLAZG9CKUBQHY");
            var octopusServerEndpoint = new OctopusServerEndpoint(Server, ApiKey);
            var octopus = new OctopusRepository(octopusServerEndpoint);

            SessionState.PSVariable.Set("OctopusRepository", octopus);
        }
    }
}
