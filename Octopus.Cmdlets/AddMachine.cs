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
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

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

        private IOctopusRepository _octopus;
        private EnvironmentResource _environment;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

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
                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
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
