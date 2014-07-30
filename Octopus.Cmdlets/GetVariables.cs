using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "OctoVariables")]
    public class GetVariables : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "The project to get the variables for."
            )]
        public string Project { get; set; }

        protected override void ProcessRecord()
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
            var variableSet = octopus.VariableSets.Get(project.Link("Variables"));

            foreach (var environment in variableSet.Variables)
            {
                WriteObject(environment);                
            }
        }
    }
}
