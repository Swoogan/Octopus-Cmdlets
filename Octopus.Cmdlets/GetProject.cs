using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Project", DefaultParameterSetName = "ByName")]
    public class GetProject : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to look for.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = false,
            HelpMessage = "The name of the project groups to look in.")]
        public string[] GroupName { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the project to look for.")]
        public string[] Id { get; set; }

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
                WriteObject(project);
        }

        private void ProcessByName()
        {
            var projectResources = Name == null ? 
                _octopus.Projects.FindAll() :
                _octopus.Projects.FindByNames(Name);

            var groups = _octopus.ProjectGroups.FindByNames(GroupName);

            var projects = groups.Count > 0
                ? (from p in projectResources
                    from g in groups
                    where p.ProjectGroupId == g.Id
                    select p)
                : projectResources;

            foreach (var project in projects)
                WriteObject(project);
        }
    }
}