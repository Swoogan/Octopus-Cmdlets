using Octopus.Client;
using Octopus.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

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
            HelpMessage = "The name of the variable to retrieve."
            )]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "VariableSet",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The library variable set to get the variables for."
            )]
        public string[] VariableSet { get; set; }

        [Parameter(
            ParameterSetName = "VariableSetId",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The library variable set to get the variables for."
            )]
        public string[] VariableSetId { get; set; }

        private readonly List<VariableSetResource> _variableSets = new List<VariableSetResource>();

        protected override void BeginProcessing()
        {
            var octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (octopus == null)
            {
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");
            }

            switch (ParameterSetName)
            {
                case "Project":
                    LoadProjectVariableSet(octopus);
                    break;
                case "VariableSet":
                    LoadLibraryVariableSetByNames(octopus);
                    break;
                case "VariableSetId":
                    LoadLibraryVariableSetByIds(octopus);
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void LoadLibraryVariableSetByIds(IOctopusRepository octopus)
        {
            // Find the library variable sets that owns the variables we want to get
            foreach (var id in VariableSetId)
            {
                var idForClosure = id;
                var variableSet = octopus.LibraryVariableSets.FindOne(vs =>
                    vs.Id == idForClosure);

                if (variableSet == null)
                    WriteWarning(string.Format("Library variable set '{0}' was not found.", id));
                else
                    _variableSets.Add(octopus.VariableSets.Get(variableSet.Link("Variables")));
            }
        }

        private void LoadLibraryVariableSetByNames(IOctopusRepository octopus)
        {
            // Find the library variable sets that owns the variables we want to get
            foreach (var name in VariableSet)
            {
                var nameForClosure = name;
                var variableSet = octopus.LibraryVariableSets.FindOne(vs =>
                    vs.Name.Equals(nameForClosure, StringComparison.InvariantCultureIgnoreCase));

                if (variableSet == null)
                    WriteWarning(string.Format("Library variable set '{0}' was not found.", name));
                else
                    _variableSets.Add(octopus.VariableSets.Get(variableSet.Link("Variables")));
            }
        }

        private void LoadProjectVariableSet(IOctopusRepository octopus)
        {
            // Find the project that owns the variables we want to get
            var project = octopus.Projects.FindByName(Project);

            if (project == null)
            {
                throw new Exception(string.Format("Project '{0}' was not found.", Project));
            }

            // Get the variable set
            _variableSets.Add(octopus.VariableSets.Get(project.Link("Variables")));
        }

        protected override void ProcessRecord()
        {
            var variables = Name == null
                ? _variableSets.SelectMany(variableSet => variableSet.Variables)
                : (from name in Name
                    from variableSet in _variableSets
                    from variable in variableSet.Variables
                    where variable.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    select variable);

            foreach (var variable in variables)
                WriteObject(variable);
        }
    }
}
