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
            if (Project != null)
            {
                // Find the project that owns the variables we want to edit
                var project = _octopus.Projects.FindByName(Project);

                if (project == null)
                {
                    const string msg = "Project '{0}' was found.";
                    throw new Exception(string.Format(msg, Project));
                }

                _releases = _octopus.Releases.FindMany(r => r.ProjectId == project.Id);
            }
            else
            {
                _releases = _octopus.Releases.FindAll();
            }

            foreach (var release in _releases)
            {
                WriteObject(release);
            }
            //if (Name == null)
            //{
            //    foreach (var variable in _variableSet.Variables)
            //        WriteObject(variable);
            //}
            //else
            //{
            //    foreach (var name in Name)
            //        foreach (var variable in _variableSet.Variables)
            //            if (variable.Name == name)
            //                WriteObject(variable);
            //}
        }
    }
}
