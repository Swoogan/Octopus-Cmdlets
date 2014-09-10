using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "Project", DefaultParameterSetName = "ByName")]
    public class RemoveProject : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The name of the project to remove.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "The id of the project to remove.")]
        [Alias("ProjectId")]
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
             var projects = from id in Id
                         select _octopus.Projects.Get(id);

            foreach (var project in projects)
            {
                WriteVerbose("Deleting project: " + project.Name);
                _octopus.Projects.Delete(project);
            }
        }

        private void ProcessByName()
        {
            var projects = _octopus.Projects.FindByNames(Name);

            foreach (var project in projects)
            {
                WriteVerbose("Deleting project: " + project.Name);
                _octopus.Projects.Delete(project);
            }
        }
    }
}
