using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "ProjectGroup")]
    public class RemovedProjectGroup : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the VariableSet to retrieve.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to retrieve.")]
        [Alias("Id")]
        public string[] ProjectGroupId { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");
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
            var groups = from id in ProjectGroupId
                         select _octopus.ProjectGroups.Get(id);

            foreach (var group in groups)
            {
                WriteVerbose("Deleting project group: " + group.Name);
                _octopus.ProjectGroups.Delete(group);
            }
        }

        protected void ProcessByName()
        {
            var groups = _octopus.ProjectGroups.FindByNames(Name);

            foreach (var group in groups)
            {
                WriteVerbose("Deleting project group: " + group.Name);
                _octopus.ProjectGroups.Delete(group);
            }
        }
    }
}
