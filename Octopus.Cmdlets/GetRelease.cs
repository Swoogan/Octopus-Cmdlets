using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Release", DefaultParameterSetName = "ByProjectName")]
    public class GetRelease : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByProjectName",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to get the release for.")]
        public string Project { get; set; }

        [Parameter(
            ParameterSetName = "ByProjectId",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The project to get the release for.")]
        public string ProjectId { get; set; }

        [Parameter(
            ParameterSetName = "ByProjectName",
            Position = 1,
            Mandatory = false,
            HelpMessage = "The version number of the release to retrieve.")]
        [Parameter(
            ParameterSetName = "ByProjectId",
            Position = 1,
            Mandatory = false,
            HelpMessage = "The version number of the release to retrieve.")]
        public string[] Version { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the project to retrieve.")]
        [Alias("Id")]
        public string[] ReleaseId { get; set; }

        //[Parameter(Mandatory = false,
        //    HelpMessage = "Tells the command to load and cache all the releases")]
        //public SwitchParameter Cache { get; set; }

        private OctopusRepository _octopus;
        private ProjectResource _project;
        //private List<ReleaseResource> _releases;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            switch (ParameterSetName)
            {
                case "ByProjectName":
                    _project = _octopus.Projects.FindByName(Project);
                    if (_project == null)
                        throw new Exception(string.Format("Project '{0}' was found.", Project));
                    break;
                case "ByProjectId":
                    _project = _octopus.Projects.Get(ProjectId);
                    if (_project == null)
                        throw new Exception(string.Format("Project '{0}' was found.", Project));
                    break;
            }

            //if (!Cache || Extensions.Cache.Releases.IsExpired)
            //    _releases = _octopus.Projects.GetReleases(_project).Items.ToList();

            //if (Cache)
            //{
            //    if (Extensions.Cache.Releases.IsExpired)
            //        Extensions.Cache.Releases.Set(_releases);
            //    else
            //        _releases = Extensions.Cache.Releases.Values;
            //}

            WriteDebug("Loaded releases");
        }
       
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByProjectName":
                case "ByProjectId":
                    ProcessByProject();
                    break;
                case "ById":
                    ProcessById();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByProject()
        {
            if (Version != null)
            {
                var releases = Version.Select(v => _octopus.Projects.GetReleaseByVersion(_project, v));
                foreach (var release in releases)
                    WriteObject(release);
            }
            else
            {
                var releases = _octopus.Projects.GetReleases(_project);
                foreach (var release in releases.Items)
                    WriteObject(release);
            }
        }

        private void ProcessById()
        {
            foreach (var id in ReleaseId)
                WriteObject(_octopus.Releases.Get(id));
        }
    }
}
