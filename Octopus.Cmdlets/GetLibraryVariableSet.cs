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
            Mandatory = true,
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
            foreach (var name in Name)
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    foreach (var variableSet in _octopus.LibraryVariableSets.FindAll())
                        WriteObject(variableSet);
                }
                else
                {
                    var variableSet = _octopus.LibraryVariableSets.FindOne(x => x.Name == name);
                    WriteObject(variableSet);
                }
            }
        }
    }
}
