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
            Mandatory = true,
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

        //[Parameter(
        //    Mandatory = false,
        //    Position = 6
        //    )]
        //public string[] Steps { get; set; }

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

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");

            WriteDebug("Got connection");

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was found.";
                throw new Exception(string.Format(msg, Project));
            }

            WriteDebug("Found project" + project.Id);

            // Get the variables for editing
            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));

            WriteDebug("Found variable set" + _variableSet.Id);
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
            var environments = _octopus.Environments.FindByNames(Environments);
            var ids = environments.Select(environment => environment.Id).ToList();
            
            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Environment, new ScopeValue(ids));

            if (Roles != null && Roles.Length > 0)
                variable.Scope.Add(ScopeField.Role, new ScopeValue(Roles));

            if (Machines != null && Machines.Length > 0)
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(Machines));

            _variableSet.Variables.Add(variable);
        }

        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);

            WriteDebug("Modified the variable set");
        }
    }
}
