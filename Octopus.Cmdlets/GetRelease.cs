using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get,
        "Release")]
    public class GetRelease : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to get the release for.")]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The version number of the release to retrieve.")]
        public string[] Version { get; set; }

        private OctopusRepository _octopus;
        private ProjectResource _project;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was found.";
                throw new Exception(string.Format(msg, Project));
            }

            _project = project;
        }

        protected override void ProcessRecord()
        {
            if (Version != null)
            {
                var releases = Version.Select(v => _octopus.Projects.GetReleaseByVersion(_project, v));
                foreach (var release in releases)
                    WriteObject(release);
            }
            else
            {
                var releases = _octopus.Releases.FindMany(r => r.ProjectId == _project.Id);

                foreach (var release in releases)
                    WriteObject(release);                
            }
        }
    }
}
