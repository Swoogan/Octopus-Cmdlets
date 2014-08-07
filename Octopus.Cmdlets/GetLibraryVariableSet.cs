using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "LibraryVariableSet")]
    public class GetLibraryVariableSet : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the VariableSet to look for."
            )]
        public string[] Name { get; set; }

        private OctopusRepository _octopus;
        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");

            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
        }

        protected override void ProcessRecord()
        {
            var variableSets = _octopus.LibraryVariableSets.FindAll();

            if (Name == null)
            {
                foreach (var variableSet in variableSets)
                    WriteObject(variableSet);
            }
            else
            {
                foreach (var name in Name)
                    foreach (var variableSet in variableSets)
                        if (variableSet.Name == name)
                            WriteObject(variableSet);
            }
        }
    }
}
