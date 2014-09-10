using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsData.Update, "Variable")]
    public class UpdateVariable : PSCmdlet
    {
         [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = "Parts",
            HelpMessage = "The project to get the variables for."
            )]

        //[Parameter(Position = 0, 
        //    Mandatory = true, 
        //    ParameterSetName = "InputObject")]
        public string Project { get; set; }

         [Parameter(
             Position = 1,
             Mandatory = true,
             ParameterSetName = "Parts"
             )]
         public string Id { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            ParameterSetName = "Parts"
            )]
        public string Name { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = true,
            ParameterSetName = "Parts"
            )]
        public string Value { get; set; }

        [Parameter(
            Position = 4,
            Mandatory = false,
            ParameterSetName = "Parts"
            )]
        public string[] Environments { get; set; }

        //[Parameter(
        //    Mandatory = false,
        //    Position = 4
        //    )]
        //public string[] Roles { get; set; }

        //[Parameter(
        //    Mandatory = false,
        //    Position = 5
        //    )]
        //public string[] Machines { get; set; }

        //[Parameter(
        //    Mandatory = false,
        //    Position = 6
        //    )]
        //public string[] Steps { get; set; }

        [Parameter(
            Position = 5,
            Mandatory = false,
            ParameterSetName = "Parts"
            )]
        public SwitchParameter Sensitive { get; set; }

        //[Parameter(
        //    Position = 1,
        //    Mandatory = true,
        //    ValueFromPipeline = true,
        //    ParameterSetName = "InputObject"
        //    )]
        //public VariableResource[] InputObject { get; set; }

        private OctopusRepository _octopus;
        private VariableSetResource _variableSet;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

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
            else if (Sensitive.IsPresent)
                variable.IsSensitive = Sensitive;
            else if (Value != null)
                variable.Value = Value;
            else if (Environments != null)
            {
                var environments = _octopus.Environments.FindByNames(Environments);
                var ids = environments.Select(environment => environment.Id).ToList();
                variable.Scope[ScopeField.Environment] =  new ScopeValue(ids);
            }

            //_variableSet.Variables.
        }

        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);

            WriteDebug("Modified the variable set");
        }
    }
}
