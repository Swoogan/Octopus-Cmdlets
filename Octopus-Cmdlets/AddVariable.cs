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
    /// <para type="synopsis">Add a new variable to the Octopus Deploy server.</para>
    /// <para type="description">The Add-OctoVariable cmdlet adds a new variable set to the Octopus Deploy server.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Variable",
        DefaultParameterSetName = "ByObject")]
    public class AddVariable : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to add the variable to.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to add the variable to.")]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the variable to create.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The value of the variable to create.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Value { get; set; }

        /// <summary>
        /// <para type="description">The environments to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Environments { get; set; }

        /// <summary>
        /// <para type="description">The roles to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Roles { get; set; }

        /// <summary>
        /// <para type="description">The machines to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Machines { get; set; }

        /// <summary>
        /// <para type="description">The deployment steps to restrict the scope to.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Steps { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether the variable is sensitive (value should be hidden).</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Sensitive { get; set; }

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
            ValueFromPipeline = true)]
        public VariableResource[] InputObject { get; set; }

        private IOctopusRepository _octopus;
        private VariableSetResource _variableSet;
        private DeploymentProcessResource _deploymentProcess;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            var project = LoadProject();

            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));
            WriteDebug("Found variable set" + _variableSet.Id);

            if (Steps != null)
                LoadDeploymentProcess(project);
        }

        private ProjectResource LoadProject()
        {
            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            WriteDebug("Found project" + project.Id);

            return project;
        }

        private void LoadDeploymentProcess(ProjectResource project)
        {
            var id = project.DeploymentProcessId;
            _deploymentProcess = _octopus.DeploymentProcesses.Get(id);

            WriteDebug("Loaded the deployment process");
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteDebug("ParameterSetName: " + ParameterSetName);

            switch (ParameterSetName)
            {
                case "ByParts":
                    ProcessByParts();
                    break;

                case "ByObject":
                    foreach (var variable in InputObject)
                        _variableSet.Variables.Add(variable);
                    break;

                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByParts()
        {
            var variable = new VariableResource { Name = Name, Value = Value, IsSensitive = Sensitive };

            if (Environments != null)
                AddEnvironments(variable);

            if (Machines != null)
                AddMachines(variable);

            if (Steps != null) 
                AddSteps(variable);

            if (Roles != null && Roles.Length > 0)
                variable.Scope.Add(ScopeField.Role, new ScopeValue(Roles));

            _variableSet.Variables.Add(variable);
        }

        private void AddEnvironments(VariableResource variable)
        {
            var environments = _octopus.Environments.FindByNames(Environments);
            var ids = environments.Select(environment => environment.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Environment, new ScopeValue(ids));
        }

        private void AddMachines(VariableResource variable)
        {
            var machines = _octopus.Machines.FindByNames(Machines);
            var ids = machines.Select(m => m.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(ids));
        }

        private void AddSteps(VariableResource variable)
        {
            var steps = (from step in _deploymentProcess.Steps
                        from s in Steps
                        where step.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase)
                        select step.Id).ToList();

            if (steps.Any())
                variable.Scope.Add(ScopeField.Action, new ScopeValue(steps));
        }

        /// <summary>
        /// EndProcessing
        /// </summary>
        protected override void EndProcessing()
        {
            _octopus.VariableSets.Modify(_variableSet);
            WriteVerbose("Modified the variable set");
        }
    }
}
