using System;
using System.Collections.Generic;
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
            Mandatory = false,
            HelpMessage = "The project to get the release for.")]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            HelpMessage = "The version number of the release to lookup.")]
        public string Version { get; set; }

        //[Parameter(
        //    Position = 1,
        //    Mandatory = false,
        //    ValueFromPipeline = true,
        //    ValueFromPipelineByPropertyName = true,
        //    HelpMessage = "The name of the variable to look for."
        //    )]
        //public string[] Name { get; set; }

        private List<ReleaseResource> _releases;
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
            ProjectResource project = null;

            if (Project != null)
            {
                // Find the project that owns the variables we want to edit
                project = _octopus.Projects.FindByName(Project);

                if (project == null)
                {
                    const string msg = "Project '{0}' was found.";
                    throw new Exception(string.Format(msg, Project));
                }
            }

            _releases = _octopus.Releases.FindMany(r => 
                (project == null || r.ProjectId == project.Id) &&
                (Version == null || r.Version == Version));

            foreach (var release in _releases)
                WriteObject(release);
        }
    }
}
