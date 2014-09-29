using System;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, 
        "Variable", 
        DefaultParameterSetName = "ProjectByParts")]
    public class AddVariable : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ProjectByParts",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to add the variable to.")]
        [Parameter(
            ParameterSetName = "ProjectByInputObject",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The project to add the variable to.")]
        public string Project { get; set; }

        [Parameter(
            ParameterSetName = "VsByParts",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Library VariableSet to add the variable to.")]
        [Parameter(
            ParameterSetName = "VsByInputObject",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Library VariableSet to add the variable to.")]
        public string VariableSet { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Value { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Environments { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Roles { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Machines { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Steps { get; set; }

        [Parameter(
            ParameterSetName = "ProjectByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            ParameterSetName = "VsByParts",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Sensitive { get; set; }

        [Parameter(
            ParameterSetName = "VsByInputObject",
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true)]
        [Parameter(
            ParameterSetName = "ProjectByInputObject",
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true)]
        public VariableResource[] InputObject { get; set; }

        private OctopusRepository _octopus;
        private VariableSetResource _variableSet;
        private DeploymentProcessResource _deploymentProcess;

        #region BeginProcessing

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Got connection");

            // Get the variables for editing
            switch (ParameterSetName)
            {
                case "VsByParts":
                    InitVsByParts();
                    break;
                case "VsByInputObject":
                    LoadLibraryVariableSet();
                    break;
                case "ProjectByParts":
                    InitProjectByParts();
                    break;
                case "ProjectByInputObject":
                    var project = LoadProject();
                    LoadVariableSet(project.Link("Variables"));
                    break;

                default:
                    throw new ArgumentException("Bad ParameterSet Name");
            }
        }

        private void InitVsByParts()
        {
            LoadLibraryVariableSet();
            var project = LoadProject();

            if (Steps != null)
                LoadDeploymentProcess(project);
        }

        private void InitProjectByParts()
        {
            var project = LoadProject();
            LoadVariableSet(project.Link("Variables"));

            if (Steps != null)
                LoadDeploymentProcess(project);
        }

        private ProjectResource LoadProject()
        {
            // Find the project that owns the variables we want to edit
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            WriteDebug("Found project" + project.Id);

            return project;
        }

        private void LoadVariableSet(string link)
        {
            _variableSet = _octopus.VariableSets.Get(link);
            WriteDebug("Found variable set" + _variableSet.Id);
        }

        private void LoadDeploymentProcess(ProjectResource project)
        {
            var id = project.DeploymentProcessId;
            _deploymentProcess = _octopus.DeploymentProcesses.Get(id);

            WriteDebug("Loaded the deployment process");
        }

        private void LoadLibraryVariableSet()
        {
            var libraryVariableSet =
                _octopus.LibraryVariableSets.FindOne(
                    v => v.Name.Equals(VariableSet, StringComparison.InvariantCultureIgnoreCase));

            if (libraryVariableSet == null)
                throw new Exception(string.Format("Library variable set '{0}' was not found.", VariableSet));

            LoadVariableSet(libraryVariableSet.Link("Variables"));
        }

        #endregion

        #region ProcessRecord

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "VsByParts":
                case "ProjectByParts":
                    ProcessByParts();
                    break;

                case "VsByInputObject":
                case "ProjectByInputObject":
                    foreach (var variable in InputObject)
                        _variableSet.Variables.Add(variable);
                    break;

                default:
                    throw new ArgumentException("Bad ParameterSet Name");
            }
        }

        private void ProcessByParts()
        {
            var variable = new VariableResource { Name = Name, Value = Value, IsSensitive = Sensitive };
            
            AddEnvironments(variable);
            AddMachines(variable);
            AddSteps(variable);

            if (Roles != null && Roles.Length > 0)
                variable.Scope.Add(ScopeField.Role, new ScopeValue(Roles));

            _variableSet.Variables.Add(variable);
        }

        private void AddEnvironments(VariableResource variable)
        {
            var environments = _octopus.Environments.FindByNames(Environments);
            var ids = environments.Select(environment => environment.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Environment, new ScopeValue(ids));
        }

        private void AddMachines(VariableResource variable)
        {
            var machines = _octopus.Machines.FindByNames(Machines);
            var ids = machines.Select(m => m.Id).ToList();

            if (ids.Count > 0)
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(ids));
        }

        private void AddSteps(VariableResource variable)
        {
            if (Steps == null) return;

            var steps = (from step in _deploymentProcess.Steps
                        from s in Steps
                        where step.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase)
                        select step.Id).ToList();

            if (steps.Any())
                variable.Scope.Add(ScopeField.Machine, new ScopeValue(steps));
        }

        #endregion

        protected override void EndProcessing()
        {
            _octopus.VariableSets.Modify(_variableSet);
            WriteDebug("Modified the variable set");
        }
    }
}
