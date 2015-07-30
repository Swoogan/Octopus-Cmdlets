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
using Octopus.Platform.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Update an existing variable in the Octopus Deploy server.</para>
    /// <para type="description">The Update-OctoVariable cmdlet updates an existing variable in the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>update-octovariable Project variables-1 NewName</code>
    ///   <para>
    ///      Give the variable in the project 'Project', with the id 'variables-1', the 
    ///      name 'NewName'.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>update-octovariable -Project Project -Id variables-1 -Name NewName -Value "New Value" -Environments "DEV", "TEST"</code>
    ///   <para>
    ///      Give the variable in the project 'Project', with the id 'variables-1', the 
    ///      name 'NewName', value "New Value" and restrict the scope to the DEV and TEST environments.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsData.Update, "Variable")]
    public class UpdateVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project containing the variable to update.</para>
        /// </summary>
         [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = "Parts")]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The id of the variable to update.</para>
        /// </summary>
        [Parameter(
             Position = 1,
             Mandatory = true,
             ParameterSetName = "Parts")]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">The new name of the variable.</para>
        /// </summary>
        [Parameter(
            Position = 2,
            Mandatory = false,
            ParameterSetName = "Parts")]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The new value of the variable.</para>
        /// </summary>
        [Parameter(
            Position = 3,
            Mandatory = false,
            ParameterSetName = "Parts")]
        public string Value { get; set; }

        /// <summary>
        /// <para type="description">The environments to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            Position = 4,
            Mandatory = false,
            ParameterSetName = "Parts")]
        public string[] Environments { get; set; }

        /// <summary>
        /// <para type="description">The roles to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            Position = 5)]
        public string[] Roles { get; set; }

        /// <summary>
        /// <para type="description">The machines to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            Position = 6)]
        public string[] Machines { get; set; }

        /// <summary>
        /// <para type="description">The actions to restrict the scope to.</para>
        /// </summary>
        //[Parameter(
        //    Mandatory = false,
        //    Position = 7)]
        //public string[] Actions { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether the variable is sensitive (value should be hidden).</para>
        /// </summary>
        [Parameter(
            Position = 5,
            Mandatory = false,
            ParameterSetName = "Parts")]
        public SwitchParameter Sensitive { get; set; }

        //[Parameter(
        //    Position = 1,
        //    Mandatory = true,
        //    ValueFromPipeline = true,
        //    ParameterSetName = "InputObject")]
        //public VariableResource[] InputObject { get; set; }

        private IOctopusRepository _octopus;
        private VariableSetResource _variableSet;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Got connection");

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was not found.";
                throw new Exception(string.Format(msg, Project));
            }

            WriteDebug("Found project" + project.Id);

            // Get the variables for editing
            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));

            WriteDebug("Found variable set" + _variableSet.Id);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "Parts":
                    ProcessByParts();
                    break;

                //case "InputObject":
                //    foreach (var variable in InputObject)
                //        _variableSet.Variables.Add(variable);
                //    break;

                default:
                    throw new ArgumentException("Bad ParameterSet Name");
            }
        }

        private void ProcessByParts()
        {
            var variable = _variableSet.Variables.FirstOrDefault(v => v.Id == Id);
            if (variable == null)
                throw new Exception(string.Format("Variable with Id '{0}' not found.", Id));

            if (Name != null)
                variable.Name = Name;
            if (Sensitive.IsPresent)
                variable.IsSensitive = Sensitive;
            if (Value != null)
                variable.Value = Value;
            
            if (Environments != null)
            {
                var environments = _octopus.Environments.FindByNames(Environments);
                var ids = environments.Select(environment => environment.Id).ToList();
                variable.Scope[ScopeField.Environment] =  new ScopeValue(ids);
            }

            if (Roles != null)
            {
                variable.Scope[ScopeField.Role].Clear();
                foreach (var role in Roles)
                    variable.Scope[ScopeField.Role].Add(role);
            }

            if (Machines != null)
            {
                var machines = _octopus.Machines.FindByNames(Machines);
                var ids = machines.Select(m => m.Id).ToList();
                variable.Scope[ScopeField.Machine] = new ScopeValue(ids);
            }
        }

        /// <summary>
        /// EndProcessing
        /// </summary>
        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);

            WriteDebug("Modified the variable set");
        }
    }
}
