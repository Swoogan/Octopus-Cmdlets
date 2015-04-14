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
using Octopus.Client.Exceptions;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a machine from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoMachine cmdlet gets a machine from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octomachine</code>
    ///   <para>
    ///      Get all the machines.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>get-octomachine web-01</code>
    ///   <para>
    ///      Get the machine named 'web-01'
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Machine", DefaultParameterSetName = "ByName")]
    public class GetMachine : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the machine to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the machine to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Id { get; set; }

        private IOctopusRepository _octopus;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
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
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByName()
        {
            var machines = Name != null
                ? _octopus.Machines.FindByNames(Name)
                : _octopus.Machines.FindAll();

            foreach (var machine in machines)
                WriteObject(machine);
        }

        private void ProcessById()
        {
            foreach (var id in Id)
                OutputMachine(id);
        }

        private void OutputMachine(string id)
        {
            try
            {
                WriteObject(_octopus.Machines.Get(id));
            }
            catch (OctopusResourceNotFoundException)
            {
            }
        }
    }
}
