using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ProjectGroup")]
    public class GetProjectGroup : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the VariableSet to look for.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to look for.")]
        public string[] Id { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");
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
            var groups = from id in Id
                select _octopus.ProjectGroups.Get(id);

            foreach (var group in groups)
                WriteObject(group);
        }

        protected void ProcessByName()
        {
            var groups = Name == null ? 
                _octopus.ProjectGroups.FindAll() : 
                _octopus.ProjectGroups.FindByNames(Name);

            foreach (var group in groups)
                WriteObject(group);
        }
    }
}
