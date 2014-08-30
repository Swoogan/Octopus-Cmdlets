using System;
using System.Collections.Generic;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Extensions;

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
            HelpMessage = "The version number of the release to lookup.")]
        public string Version { get; set; }

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
            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                const string msg = "Project '{0}' was found.";
                throw new Exception(string.Format(msg, Project));
            }

            if (Version != null)
            {
                var release = Release.FindByVersion(_octopus, project.Id, Version);
                WriteObject(release);
            }
            else
            {
                var releases = _octopus.Releases.FindMany(r => r.ProjectId == project.Id);

                foreach (var release in releases)
                    WriteObject(release);                
            }
        }
    }
}
