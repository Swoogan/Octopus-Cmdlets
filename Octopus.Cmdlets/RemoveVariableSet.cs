using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "VariableSet", DefaultParameterSetName = "ByName")]
    public class RemoveVariableSet : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public string Id { get; set; }

        [Parameter(
            ParameterSetName = "ByObject",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
            )]
        public LibraryVariableSetResource InputObject { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository)SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
            {
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");
            }
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
                case "ByObject":
                    ProcessByObject();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByObject()
        {
            _octopus.LibraryVariableSets.Delete(InputObject);
        }

        private void ProcessById()
        {
            var set = _octopus.LibraryVariableSets.Get(Id);
            _octopus.LibraryVariableSets.Delete(set);
        }

        private void ProcessByName()
        {
            var set = _octopus.LibraryVariableSets.FindOne(vs => vs.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            _octopus.LibraryVariableSets.Delete(set);
        }
    }
}
