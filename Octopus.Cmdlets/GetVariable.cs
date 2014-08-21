using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Variable", DefaultParameterSetName = "Project")]
    public class GetVariable : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "Project",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to get the variables for."
            )]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the variable to look for."
            )]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "VariableSet",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The library variable set to get the variables for."
            )]
        public string VariableSet { get; set; }

        private VariableSetResource _variableSet;

        protected override void BeginProcessing()
        {
            var octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
            {
                throw new Exception("Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
            }

            switch (ParameterSetName)
            {
                case "Project":
                    LoadProjectVariableSet(octopus);
                    break;
                case "VariableSet":
                    LoadLibraryVariableSet(octopus);
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void LoadLibraryVariableSet(OctopusRepository octopus)
        {
            // Find the library variable set that owns the variables we want to get
            var variableSet = octopus.LibraryVariableSets.FindOne(vs => vs.Name.Equals(VariableSet, StringComparison.InvariantCultureIgnoreCase));

            if (variableSet == null)
            {
                const string msg = "Library variable set '{0}' was not found.";
                throw new Exception(string.Format(msg, Project));
            }

            // Get the variable set
            _variableSet = octopus.VariableSets.Get(variableSet.Link("Variables"));
        }

        private void LoadProjectVariableSet(OctopusRepository octopus)
        {
            // Find the project that owns the variables we want to get
            var project = octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was not found.";
                throw new Exception(string.Format(msg, Project));
            }

            // Get the variable set
            _variableSet = octopus.VariableSets.Get(project.Link("Variables"));
        }

        protected override void ProcessRecord()
        {
            if (Name == null)
            {
                foreach (var variable in _variableSet.Variables)
                    WriteObject(variable);
            }
            else
            {
                foreach (var name in Name)
                    foreach (var variable in _variableSet.Variables)
                        if (variable.Name == name)
                            WriteObject(variable);                    
            }
        }
    }
}
