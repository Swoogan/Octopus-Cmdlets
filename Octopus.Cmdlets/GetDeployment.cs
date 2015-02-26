using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Deployment")]
    public class GetDeployment : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "The name of the project to get deployments for.")]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = "The name of the release to get deployments for.")]
        public string Release { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was found.", Project));

            var release = _octopus.Projects.GetReleases(project);
            WriteObject(release);
        }
    }
}
