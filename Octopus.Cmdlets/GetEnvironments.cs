using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "OctoEnvironments")]
    public class GetEnvironments : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
            {
                throw new Exception("Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
            }

            foreach (var environment in octopus.Environments.GetAll())
            {
                WriteObject(environment);                
            }
        }
    }
}
