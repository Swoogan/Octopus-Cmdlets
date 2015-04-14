#region License
// Copyright 2014 Colin Svingen

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a deployment from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoDeployment cmdlet gets a deployment from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octodeployment Project v1.0.1</code>
    ///   <para>
    ///      Get all the deployments of the release 'v1.01.' for the project named 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Deployment", DefaultParameterSetName = "ByProject")]
    public class GetDeployment : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to get deployments for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProject",
            Position = 0,
            Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the release to get deployments for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProject",
            Position = 1,
            Mandatory = true)]
        public string Release { get; set; }

        /// <summary>
        /// <para type="description">The id of the release to get deployments for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByRelease",
            Mandatory = true)]
        public string ReleaseId { get; set; }

        private IOctopusRepository _octopus;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
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
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            var release = _octopus.Projects.GetReleaseByVersion(project, Release);
            if (release != null)
            {
                var deployments = _octopus.Releases.GetDeployments(release);
                foreach (var deployment in deployments.Items)
                    WriteObject(deployment);
            }
            else
            {
                WriteVerbose(string.Format("Project '{0}' does not have a release '{1}'", project.Name, Release));
            }
        }

        private void ProcessByRelease()
        {
            var release = _octopus.Releases.Get(ReleaseId);
            var deployments = _octopus.Releases.GetDeployments(release);
            foreach (var deployment in deployments.Items)
                WriteObject(deployment);
        }
    }
}