using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, 
        "Variable", 
        DefaultParameterSetName = "Parts")]
    public class AddVariable : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to get the variables for."
            )]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Parts"
            )]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Parts"
            )]
        public string Value { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Parts"
            )]
        public string[] Environments { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Parts"
            )]
        public string[] Roles { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Parts"
            )]
        public string[] Machines { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 6
            )]
        public string[] Steps { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = "Parts"
            )]
        public SwitchParameter Sensitive { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = "InputObject"
            )]
        public VariableResource[] InputObject { get; set; }

        private OctopusRepository _octopus;
        private VariableSetResource _variableSet;
        private DeploymentProcessResource _deploymentProcess;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Got connection");

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            WriteDebug("Found project" + project.Id);

            // Get the variables for editing
            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));

            WriteDebug("Found variable set" + _variableSet.Id);

            var id = project.DeploymentProcessId;
            _deploymentProcess = _octopus.DeploymentProcesses.Get(id);

            WriteDebug("Loaded the deployment process");
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "Parts":
                    ProcessByParts();
                    break;

                case "InputObject":
                    foreach (var variable in InputObject)
                        _variableSet.Variables.Add(variable);
                    break;

                default:
                    throw new ArgumentException("Bad ParameterSet Name");
            }
        }

        private void ProcessByParts()
        {
            var variable = new VariableResource { Name = Name, Value = Value, IsSensitive = Sensitive };
            
            AddEnvironments(variable);
            AddMachines(variable);
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
            if (Steps == null) return;

            var steps = (from step in _deploymentProcess.Steps
                        from s in Steps
                        where step.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase)
                        select step.Id).ToList();

            if (steps.Any())
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(steps));
        }

        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);

            WriteDebug("Modified the variable set");
        }
    }
}
