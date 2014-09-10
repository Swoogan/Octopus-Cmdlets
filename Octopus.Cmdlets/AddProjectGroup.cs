using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "ProjectGroup")]
    public class AddProjectGroup : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project group to create.")]
        public string Name { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The description of the project group to create.")]
        public string Description { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            _octopus.ProjectGroups.Create(new ProjectGroupResource
            {
                Name = Name,
                Description = Description
            });
        }
    }
}
