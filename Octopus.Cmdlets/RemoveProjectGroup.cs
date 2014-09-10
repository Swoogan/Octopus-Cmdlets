using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "ProjectGroup", DefaultParameterSetName = "ByName")]
    public class RemovedProjectGroup : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project group to remove.")]
        [Alias("GroupName")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the project to retrieve.")]
        [Alias("ProjectGroupId")]
        public string[] Id { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
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
