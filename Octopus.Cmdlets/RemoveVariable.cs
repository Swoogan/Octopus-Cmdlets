using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "Variable", DefaultParameterSetName = "ByName")]
    public class RemoveVariable : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to remove the variables from.")]
        [Alias("ProjectName")]
        public string Project { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The name of the variable to remove.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ByObject",
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The variable to remove.")]
        public VariableResource[] InputObject{ get; set; }

        private OctopusRepository _octopus;
        private VariableSetResource _variableSet;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was found.", Project));

            // Get the variables for editing
            _variableSet = _octopus.VariableSets.Get(project.Link("Variables"));
        }

        protected override void ProcessRecord()
        {
            WriteDebug("ParameterSetName: " + ParameterSetName);

            switch (ParameterSetName)
            {
                case "ByName":
                    ProcessByName();
                    break;
                case "ById":
                    ProcessByObject();
                    break;
                default:
                    throw new ArgumentException("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByName()
        {
            var variables = (from v in _variableSet.Variables
                            from n in Name
                            where v.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                            select v).ToArray();
            
            foreach (var variable in variables)
            {
                const string msg = "Removing variable '{0}' from project '{1}'";
                WriteVerbose(string.Format(msg, variable.Name, Project));
                _variableSet.Variables.Remove(variable);
            }
        }

        private void ProcessByObject()
        {
            foreach (var variable in InputObject)
            {
                const string msg = "Removing variable '{0}' from project '{1}'";
                WriteVerbose(string.Format(msg, variable.Name, Project));
                _variableSet.Variables.Remove(variable);
            }
        }

        protected override void EndProcessing()
        {
            // Save the variables
            _octopus.VariableSets.Modify(_variableSet);
            WriteVerbose("Saved changes");
        }
    }
}
