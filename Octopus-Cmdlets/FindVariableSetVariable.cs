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
using Octopus_Cmdlets.Extensions;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Find a varaiable in an Octopus Deploy library variable set.</para>
    /// <para type="description">The Find-OctoVariableSetVariable cmdlet finds a varaiable in an Octopus Deploy library variable set</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "VariableSetVariable")]
    public class FindVariableSetVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the variable to retrieve.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The name of the VariableSet to look in.</para>
        /// </summary>
        [Parameter(
           Position = 1,
           Mandatory = false)]
        public string VariableSetName { get; set; }

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
            if (String.IsNullOrWhiteSpace(VariableSetName))
            {
                if (Cache.LibraryVariableSets.IsExpired)
                    Cache.LibraryVariableSets.Set(_octopus.LibraryVariableSets.FindAll());

                foreach (var libraryVariableSet in Cache.LibraryVariableSets.Values)
                {
                    var link = libraryVariableSet.Link("Variables");
                    WriteDebug(link);
                    WriteObjects(link);
                }
            }
            else
            {
                var libraryVariableSet = _octopus.LibraryVariableSets.FindOne(x => x.Name.Equals(VariableSetName, StringComparison.InvariantCultureIgnoreCase));

                if (libraryVariableSet == null)
                    throw new Exception(string.Format("LibraryVariableSet '{0}' was not found", VariableSetName));

                var link = libraryVariableSet.Link("Variables");
                WriteDebug(link);
                WriteObjects(link);
            }
        }

        private void WriteObjects(string link)
        {
            foreach (var variable in _octopus.VariableSets.Get(link).Variables)
            {
                WriteDebug(variable.Name);
                foreach (var name in Name)
                {
                    WriteDebug(name);

                    if (string.IsNullOrWhiteSpace(name) || variable.Name == name)
                        WriteObject(variable);
                }
            }
        }
    }
}
