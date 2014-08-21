using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "VariableSet")]
    public class AddVariableSet : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "The name of the VariableSet to create."
            )]
        public string Name { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            HelpMessage = "The name of the VariableSet to look for."
            )]
        public string Description { get; set; }

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
            var variableSet = new LibraryVariableSetResource
            {
                Name = Name,
                Description = Description
            };

            _octopus.LibraryVariableSets.Create(variableSet);
        }
    }
}
