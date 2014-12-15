using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;

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
            HelpMessage = "The name of the project to retrieve.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = false,
            HelpMessage = "The name of the project groups to look in.")]
        public string[] ProjectGroup { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = false,
            HelpMessage = "The name of the projects to exclude from the results.")]
        public string[] Exclude { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the project to retrieve.")]
        public string[] Id { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Tells the command to load and cache all the projects")]
        public SwitchParameter Cache { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            if (Cache && Extensions.Cache.Projects.IsExpired)
                Extensions.Cache.Projects.Set(_octopus.Projects.FindAll());
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
            var projects = Cache
                ? (from id in Id
                    from p in Extensions.Cache.Projects.Values
                    where p.Id == id
                    select p)
                : Id.Select(id => _octopus.Projects.Get(id));

            foreach (var project in projects)
                WriteObject(project);
        }

        private void ProcessByName()
        {
            var projectResources = Name == null ? 
                _octopus.Projects.FindAll() :
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