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
