﻿#region License
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
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a release from the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoRelease cmdlet gets a release from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octorelease Project</code>
    ///   <para>
    ///      Get all the releases for the project 'Project'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Release", DefaultParameterSetName = "ByProject")]
    public class GetRelease : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to get the release for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProject",
            Position = 0,
            Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The id of the project to get the release for.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProjectId",
            Position = 0,
            Mandatory = true)]
        public string ProjectId { get; set; }

        /// <summary>
        /// <para type="description">The version number of the release to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByProject",
            Position = 1,
            Mandatory = false)]
        [Parameter(
            ParameterSetName = "ByProjectId",
            Position = 1,
            Mandatory = false)]
        public string[] Version { get; set; }

        /// <summary>
        /// <para type="description">The id of the release to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string[] ReleaseId { get; set; }

        //[Parameter(Mandatory = false,
        //    HelpMessage = "Tells the command to load and cache all the releases")]
        //public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private ProjectResource _project;
        //private List<ReleaseResource> _releases;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            switch (ParameterSetName)
            {
                case "ByProject":
                    _project = _octopus.Projects.FindByName(Project);
                    if (_project == null)
                        throw new Exception(string.Format("The project '{0}' was not found.", Project));
                    break;
                case "ByProjectId":
                    _project = _octopus.Projects.Get(ProjectId);
                    if (_project == null)
                        throw new Exception(string.Format("A project with the id '{0}' was not found.", ProjectId));
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

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByProject":
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
                foreach (var version in Version)
                    OutputReleaseByVersion(version);
            }
            else
            {
                var releases = _octopus.Projects.GetReleases(_project);
                foreach (var release in releases.Items)
                    WriteObject(release);
            }
        }

        private void OutputReleaseByVersion(string v)
        {
            try
            {
                WriteObject(_octopus.Projects.GetReleaseByVersion(_project, v));
            }
            catch (OctopusResourceNotFoundException)
            {
            }
        }

        private void ProcessById()
        {
            foreach (var id in ReleaseId)
                OutputRelease(id);
        }

        private void OutputRelease(string id)
        {
            try
            {
                WriteObject(_octopus.Releases.Get(id));
            }
            catch (OctopusResourceNotFoundException)
            {
            }
        }
    }
}
