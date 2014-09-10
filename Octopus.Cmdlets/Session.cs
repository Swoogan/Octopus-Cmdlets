using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    static class Session
    {
        internal static OctopusRepository RetrieveSession(PSCmdlet cmdlet)
        {
            var octopus = (OctopusRepository) cmdlet.SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

            return octopus;
        }
    }
}
