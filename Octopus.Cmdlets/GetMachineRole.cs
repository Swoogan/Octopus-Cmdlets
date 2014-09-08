using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "MachineRole", DefaultParameterSetName = "ByName")]
    public class GetMachineRole : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the machine to look for.")]
        public string[] Name { get; set; }

        private OctopusRepository _octopus;
        private List<string> _roles;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

            _roles = _octopus.MachineRoles.GetAllRoleNames();
        }

        protected override void ProcessRecord()
        {
            if (Name == null)
            {
                foreach (var role in _roles)
                    WriteObject(role);
            }
            else
            {
                var roles = from name in Name
                    from role in _roles
                    where role.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    select role;
                
                foreach (var role in roles)
                    WriteObject(role);
            }
        }
    }
}
