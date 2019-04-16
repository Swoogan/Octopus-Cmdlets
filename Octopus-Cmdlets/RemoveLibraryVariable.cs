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
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Remove a variable from the Octopus Deploy library variable set.</para>
    /// <para type="description">The Remove-OctoLibraryVariable cmdlet removes a variable from the Octopus Deploy library variable set.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>remove-octolibraryvariable -VariableSet Database -Name ConnectionString</code>
    ///   <para>
    ///      Remove the variable 'ConnectionString' from the variable set 'Database'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "LibraryVariable", DefaultParameterSetName = "ByName")]
    public class RemoveLibraryVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the library variable set to remove the variable from.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string VariableSet { get; set; }

        /// <summary>
        /// <para type="description">The name of the variable to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        private IOctopusRepository _octopus;
        private VariableSetResource _variableSet;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            var libraryVariableSet =
                _octopus.LibraryVariableSets.FindOne(
                    v => v.Name.Equals(VariableSet, StringComparison.InvariantCultureIgnoreCase));

            if (libraryVariableSet == null)
                throw new Exception(string.Format("Library variable set '{0}' was not found.", VariableSet));

            _variableSet = _octopus.VariableSets.Get(libraryVariableSet.Link("Variables"));
            WriteDebug("Found variable set" + _variableSet.Id);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteDebug("ParameterSetName: " + ParameterSetName);

            switch (ParameterSetName)
            {
                case "ByName":
                    ProcessByName();
                    break;
                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByName()
        {
            foreach (var name in Name)
            {
                var found = false;
                var nameForClosure = name;

                var variables =
                    _variableSet.Variables.Where(
                        variable => variable.Name.Equals(nameForClosure, StringComparison.InvariantCultureIgnoreCase)).ToArray();

                foreach (var variable in variables)
                {
                    WriteVerbose(string.Format("Removing variable '{0}' from variable set '{1}'.", variable.Name, VariableSet));
                    _variableSet.Variables.Remove(variable);
                    found = true;
                }

                if (!found)
                    WriteWarning(string.Format("Variable '{0}' in variable set '{1}' does not exist.", name, VariableSet));
            }
        }

        /// <summary>
        /// EndProcessing
        /// </summary>
        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);
            WriteVerbose("Saved changes");
        }
    }
}
