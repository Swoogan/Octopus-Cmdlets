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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets the projects in the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoProject cmdlet gets the projects in the Octopus Deploy server.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Project", DefaultParameterSetName = "ByName")]
    public class GetProject : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The name of the project groups to look in.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = false)]
        public string[] ProjectGroup { get; set; }

        /// <summary>
        /// <para type="description">The name of the projects to exclude from the results.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = false)]
        public string[] Exclude { get; set; }

        /// <summary>
        /// <para type="description">The id of the project to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Id { get; set; }

        /// <summary>
        /// <para type="description">Tells the command to load and cache all the projects.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private List<ProjectResource> _projects;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // FIXME: Loading all the projects when you might only
            // be looking for one, isn't exactly efficient

            if (!Cache || Octopus_Cmdlets.Extensions.Cache.Projects.IsExpired)
                _projects = _octopus.Projects.FindAll();

            if (Cache)
            {
                if (Octopus_Cmdlets.Extensions.Cache.Projects.IsExpired)
                    Octopus_Cmdlets.Extensions.Cache.Projects.Set(_projects);
                else
                    _projects = Octopus_Cmdlets.Extensions.Cache.Projects.Values;
            }

            WriteDebug("Loaded projects");
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
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
            //var projects = Cache
            //    ? (from id in Id
            //       from p in _projects
            //        where p.Id == id
            //        select p)
            //    : Id.Select(id => _octopus.Projects.Get(id));

            var projects = from id in Id
                from p in _projects
                where p.Id == id
                select p;

            foreach (var project in projects)
                WriteObject(project);
        }

        private void ProcessByName()
        {
            var projectResources = Name == null ?
                _projects :
                _octopus.Projects.FindByNames(Name);

            // Filter by project group
            var groups = _octopus.ProjectGroups.FindByNames(ProjectGroup);

            var projects = groups.Count > 0
                ? (from p in projectResources
                    from g in groups
                    where p.ProjectGroupId == g.Id
                    select p)
                : projectResources;

            // Filter excludes
            var final = Exclude == null
                ? projects
                : projects.Where(p =>
                    !Exclude.Any(e => p.Name.Equals(e, StringComparison.InvariantCultureIgnoreCase))); 

            foreach (var project in final)
                WriteObject(project);
        }
    }
}