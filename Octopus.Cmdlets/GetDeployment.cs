using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Deployment", DefaultParameterSetName = "ByProject")]
    public class GetDeployment : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByProject",
            Position = 0,
            Mandatory = true,
            HelpMessage = "The name of the project to get deployments for.")]
        public string Project { get; set; }

        [Parameter(
            ParameterSetName = "ByProject",
            Position = 1,
            Mandatory = true,
            HelpMessage = "The name of the release to get deployments for.")]
        public string Release { get; set; }

        [Parameter(
            ParameterSetName = "ByRelease",
            Position = 1,
            Mandatory = true,
            HelpMessage = "The id of the release to get deployments for.")]
        public string ReleaseId { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByProject":
                    ProcessByProject();
                    break;
                case "ByRelease":
                    ProcessByRelease();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByProject()
        {
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was found.", Project));

            //if (Release != null)
            //{
            var release = _octopus.Projects.GetReleaseByVersion(project, Release);
            var link = release.Links["Deployments"];
            var deployments = _octopus.Client.List<DeploymentResource>(link);
            foreach (var deployment in deployments.Items)
                WriteObject(deployment);

            //}
            //else
            //{
            //    var releases = _octopus.Projects.GetReleases(project);
            //    foreach (var release in releases.Items)
            //        WriteObject(release);
            //}
        }

        private void ProcessByRelease()
        {
            var release = _octopus.Releases.Get(ReleaseId);
            var link = release.Links["Deployments"];
            var deployments = _octopus.Client.List<DeploymentResource>(link);
            foreach (var deployment in deployments.Items)
                WriteObject(deployment);
        }
    }
}