using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "OctoLibraryVariableSetVariable")]
    public class GetLibraryVariableSetVariable : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the variable to look for."
            )]
        public string[] Name { get; set; }

        [Parameter(
           Position = 1,
           Mandatory = false,
           HelpMessage = "The name of the VariableSet to look in."
           )]
        public string VariableSetName { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception("Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
        }

        protected override void ProcessRecord()
        {
            if (String.IsNullOrWhiteSpace(VariableSetName))
            {
                foreach (var libraryVariableSet in _octopus.LibraryVariableSets.FindAll())
                {
                    var link = libraryVariableSet.Link("Variables");
                    WriteDebug(link);

                    foreach (var variable in _octopus.VariableSets.Get(link).Variables)
                    {
                        WriteDebug(variable.Name);
                        foreach (var name in Name)
                        {
                            WriteDebug(name);

                            if (string.IsNullOrWhiteSpace(name) || variable.Name == name)
                                WriteObject(variable);
                        }
                    }
                }
            }
            else
            {
                var libraryVariableSet = _octopus.LibraryVariableSets.FindOne(x => x.Name == VariableSetName);

                if (libraryVariableSet == null)
                    throw new Exception(string.Format("LibraryVariableSet '{0}' was not found", VariableSetName));

                var link = libraryVariableSet.Link("Variables");

                foreach (var variable in _octopus.VariableSets.Get(link).Variables)
                {
                    WriteDebug(variable.Name);

                    foreach (var name in Name)
                    {
                        WriteDebug(name);

                        if (string.IsNullOrWhiteSpace(name) || variable.Name == name)
                            WriteObject(variable);
                    }
                }
            }
        }
    }
}
