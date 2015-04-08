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
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a library variable set from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoVariableSet cmdlet gets a library variable set from the Octopus Deploy server.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "VariableSet", DefaultParameterSetName = "ByName")]
    public class GetVariableSet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the variable set to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the variable set to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Id { get; set; }

        /// <summary>
        /// <para type="description">Tells the command to load and cache all the variable sets.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private List<LibraryVariableSetResource> _variableSets;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Connection established");

            if (!Cache || Utilities.Cache.LibraryVariableSets.IsExpired)
                _variableSets = _octopus.LibraryVariableSets.FindAll();

            if (Cache)
            {
                if (Utilities.Cache.LibraryVariableSets.IsExpired)
                    Utilities.Cache.LibraryVariableSets.Set(_variableSets);
                else
                    _variableSets = Utilities.Cache.LibraryVariableSets.Values;
            }

            WriteDebug("Loaded environments");
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
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessById()
        {
            var variableSets = from id in Id
                               from v in _variableSets
                               where v.Id == id
                               select v;

            foreach (var variableSet in variableSets)
                WriteObject(variableSet);
        }

        private void ProcessByName()
        {
            IEnumerable<LibraryVariableSetResource> variableSets;

            if (Name == null)
            {
                variableSets = _variableSets;
            }
            else
            {
                variableSets = from name in Name
                               from v in _variableSets 
                               where v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                               select v;
            }

            foreach (var variableSet in variableSets)
                WriteObject(variableSet);
        }
    }
}
