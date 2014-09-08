using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Extensions;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "VariableSet", DefaultParameterSetName = "ByName")]
    public class GetVariableSet : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the action to look for.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the machine to look for.")]
        public string[] Id { get; set; }

        [Parameter(
            Mandatory = false)]
        public SwitchParameter NoCache { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");

            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

            WriteDebug("Connection established");

            if (Cache.LibraryVariableSets.IsExpired || NoCache)
            {
                Cache.LibraryVariableSets.Set(_octopus.LibraryVariableSets.FindAll());
                WriteDebug("Cache miss");
            }
            else
            {
                WriteDebug("Cache hit");
            }

            WriteDebug("Loaded environments");
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    ProcessByName();
                    break;
                case "ById":
                    ProcessById();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessById()
        {
            var variableSets = from id in Id
                               from v in Cache.LibraryVariableSets.Values 
                               where v.Id == id
                               select v;

            foreach (var variableSet in variableSets)
                WriteObject(variableSet);
        }

        private void ProcessByName()
        {
            IEnumerable<LibraryVariableSetResource> variableSets;

            if (Name == null)
            {
                variableSets = Cache.LibraryVariableSets.Values;
            }
            else
            {
                variableSets = from name in Name
                               from v in Cache.LibraryVariableSets.Values 
                               where v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                               select v;
            }

            foreach (var variableSet in variableSets)
                WriteObject(variableSet);
        }
    }
}
