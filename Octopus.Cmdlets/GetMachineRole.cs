#region License
// Copyright 2014 Colin Svingen

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
            HelpMessage = "The name of the machine role to retrieve.")]
        public string[] Name { get; set; }

        private IOctopusRepository _octopus;
        private List<string> _roles;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

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
