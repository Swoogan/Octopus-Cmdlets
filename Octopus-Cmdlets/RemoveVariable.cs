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
    /// <para type="synopsis">Remove a variable from the Octopus Deploy server.</para>
    /// <para type="description">The Remove-OctoVariable cmdlet removes a variable from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>remove-octovariable Project Variable</code>
    ///   <para>
    ///      Remove the variable 'Variable' from the project 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "Variable", DefaultParameterSetName = "ByName")]
    public class RemoveVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to remove the variable from.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the variable to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">
        /// Specifies one or more variable objects. Enter a variable that contains the objects, 
        /// or type a command or expressionthat gets the objects.
        /// </para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByObject",
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The variable to remove.")]
        public VariableResource[] InputObject{ get; set; }

        private IOctopusRepository _octopus;
        private VariableSetResource _variableSet;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            // Get the variables for editing
            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));
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
                case "ByObject":
                    ProcessByObject();
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
                    WriteVerbose(string.Format("Removing variable '{0}' from project '{1}'.", variable.Name, Project));
                    _variableSet.Variables.Remove(variable);
                    found = true;
                }

                if (!found)
                    WriteWarning(string.Format("Variable '{0}' in project '{1}' does not exist.", name, Project));
            }
        }

        private void ProcessByObject()
        {
            foreach (var variable in InputObject)
            {
                const string msg = "Removing variable '{0}' from project '{1}'";
                WriteVerbose(string.Format(msg, variable.Name, Project));
                if (!_variableSet.Variables.Remove(variable))
                    WriteWarning(string.Format("Variable '{0}' in project '{1}' does not exist.", variable.Name, Project));
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
