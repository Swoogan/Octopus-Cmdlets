using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "OctoProjects")]
    public class GetProjects : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
            {
                throw new Exception("Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
            }

            foreach (var environment in octopus.Projects.GetAll())
            {
                WriteObject(environment);                
            }
        }
    }
}
