using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "OctoVariable")]
    public class GetVariable : PSCmdlet
    {
        [Parameter(
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

        private VariableSetResource _variableSet;

        protected override void BeginProcessing()
        {
            var octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
            {
                throw new Exception("Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
            }

            // Find the project that owns the variables we want to edit
            var project = octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was found.";
                throw new Exception(string.Format(msg, Project));
            }

            // Get the variables for editing
            _variableSet = octopus.VariableSets.Get(project.Link("Variables"));
        }

        protected override void ProcessRecord()
        {
            foreach (var name in Name)
                foreach (var variable in _variableSet.Variables)
                    if (string.IsNullOrWhiteSpace(name) || variable.Name == name)
                        WriteObject(variable);                
        }
    }
}
