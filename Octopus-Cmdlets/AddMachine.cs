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
using Octopus.Platform.Model;
using System.Collections.Generic;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Add a new machine.</para>
    /// <para type="description">The Add-OctoMachine cmdlet adds a new machine (tentacle) to Octopus.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Machine", DefaultParameterSetName = "ByName")]
    public class AddMachine : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the environment to add the machine to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true)]
        public string[] Environment { get; set; }

        /// <summary>
        /// <para type="description">The id of the environment to add the machine to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true)]
        public string[] EnvironmentId { get; set; }

        /// <summary>
        /// <para type="description">The name of the machine.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The thumbprint of the machine.</para>
        /// </summary>
        [Parameter(
            Position = 2,
            Mandatory = true)]
        public string Thumbprint { get; set; }

        /// <summary>
        /// <para type="description">The roles that the machine will perform.</para>
        /// </summary>
        [Parameter(
            Position = 3,
            Mandatory = true)]
        public string[] Roles { get; set; }

        /// <summary>
        /// <para type="description">The URI of the machine.</para>
        /// </summary>
        [Parameter(
            Position = 4,
            Mandatory = true)]
        public string Uri { get; set; }

        /// <summary>
        /// <para type="description">
        /// The communication style of the server. Either TentacleActive or TentaclePassive.
        /// </para>
        /// </summary>
        [Parameter(
            Position = 5,
            Mandatory = true)]
        [ValidateSet("TentacleActive", "TentaclePassive")]
        public string CommunicationStyle { get; set; }

        private IOctopusRepository _octopus;
        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            if (ParameterSetName != "ByName") return;

            if (Environment.Length != 1)
                throw new Exception(string.Format("Only 1 Environment is currently supported, you specified {0}", Environment.Length));

            var environmentIds = new List<string>();
            foreach (var environment in Environment)
            {
                var e = _octopus.Environments.FindByName(environment);
                if (e == null)
                    throw new Exception(string.Format("Environment '{0}' was not found.", environment));
                environmentIds.Add(e.Id);
            }
            EnvironmentId = environmentIds.ToArray();
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                case "ById":
                    CreateMachine();
                    break;
                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void CreateMachine()
        {
            _octopus.Machines.Create(new MachineResource
            {
                EnvironmentIds = new ReferenceCollection(EnvironmentId),
                Name = Name,
                Thumbprint = Thumbprint,
                Roles = new ReferenceCollection(Roles),
                Uri = Uri,
                CommunicationStyle = (CommunicationStyle)Enum.Parse(typeof(CommunicationStyle), CommunicationStyle, true)
            });
        }
    }
}
