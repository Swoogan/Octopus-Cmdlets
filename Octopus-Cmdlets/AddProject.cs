using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Add a new deployment project.</para>
    /// <para type="description">The Add-OctoProject cmdlet adds a new deployment project to Octopus.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>add-octoproject ExampleGroup ExampleProject</code>
    ///   <para>
    ///      Add a new project named 'ExampleProject' to the group 'ExampleGroup'
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "Project", DefaultParameterSetName = "ByName")]
    public class AddProject : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the ProjectGroup to create the project in.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true)]
        [Alias("GroupName", "ProjectGroup")]
        public string ProjectGroupName { get; set; }

        /// <summary>
        /// <para type="description">The id of the ProjectGroup to create the project in.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true)]
        [Alias("GroupId")]
        public string ProjectGroupId { get; set; }

        /// <summary>
        /// <para type="description">The name of the project to create.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The description of the project to create.</para>
        /// </summary>
        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Description { get; set; }

        private IOctopusRepository _octopus;
        private string _projectGroupId;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            if (ParameterSetName != "ByName") return;

            var projectGroup = _octopus.ProjectGroups.FindByName(ProjectGroupName);
            if (projectGroup == null)
                throw new Exception(string.Format("Project '{0}' was not found.", ProjectGroupName));

            _projectGroupId = projectGroup.Id;
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    CreateProject(_projectGroupId);
                    break;
                case "ById":
                    _octopus.ProjectGroups.Get(ProjectGroupId);
                    CreateProject(ProjectGroupId);
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void CreateProject(string projectGroupId)
        {
            _octopus.Projects.Create(new ProjectResource
            {
                Name = Name,
                Description = Description,
                ProjectGroupId = projectGroupId
            });
        }
    }
}
