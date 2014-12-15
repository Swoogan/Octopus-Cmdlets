using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Extensions;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsOther.Use, "VariableSet")]
    public class UseVariableSet : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to include the variable set into.")]
        public string Project { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to include the variable set into.")]
        public string[] VariableSet { get; set; }

        private OctopusRepository _octopus;
        private ProjectResource _project;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
            // Find the project that owns the variables we want to edit
            _project = _octopus.Projects.FindByName(Project);

            if (_project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            WriteDebug("Found project" + _project.Id);
        }

        protected override void ProcessRecord()
        {
            if (Cache.LibraryVariableSets.IsExpired)
                Cache.LibraryVariableSets.Set(_octopus.LibraryVariableSets.FindAll());

            var varSets = from name in VariableSet
                from v in Cache.LibraryVariableSets.Values
                where v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select v;

            foreach (var varSet in varSets)
                _project.IncludedLibraryVariableSetIds.Add(varSet.Id);
        }

        protected override void EndProcessing()
        {
            _octopus.Projects.Modify(_project);
            WriteVerbose("Wrote the project changes");
        }
    }
}
