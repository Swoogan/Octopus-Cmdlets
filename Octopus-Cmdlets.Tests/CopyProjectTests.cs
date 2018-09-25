using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class CopyProjectTests
    {
        private const string CmdletName = "Copy-OctoProject";
        private PowerShell _ps;
        private readonly List<ProjectResource> _projects = new List<ProjectResource>();
        private VariableSetResource _copyVariables;
        private DeploymentProcessResource _copyProcess;

        public CopyProjectTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (CopyProject));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project group
            var groupResource = new ProjectGroupResource {Name = "Octopus", Id = "projectgroups-1"};
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Octopus", null, null)).Returns(groupResource);
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Gibberish", null, null)).Returns((ProjectGroupResource) null);

            // Create project
            var project = new ProjectResource
            {
                Name = "Source", 
                Description = "Test Source",
                DeploymentProcessId = "deploymentprocesses-1",
                VariableSetId = "variablesets-1",
                DefaultToSkipIfAlreadyInstalled = true,
                IncludedLibraryVariableSetIds = new List<string> { "libraryvariablesets-1" },
                VersioningStrategy = new VersioningStrategyResource(),
                AutoCreateRelease = false,
                ReleaseCreationStrategy = new ReleaseCreationStrategyResource(),
                IsDisabled = false,
                LifecycleId = "lifecycle-1"
            };

            // Create projects
            _projects.Clear();
            _projects.Add(project);

            octoRepo.Setup(o => o.Projects.FindByName("Source", null, null)).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", null, null)).Returns((ProjectResource) null);
            octoRepo.Setup(o => o.Projects.Create(It.IsAny<ProjectResource>(), It.IsAny<object>())).Returns(
                (ProjectResource p, object o) =>
                {
                    p.VariableSetId = "variablesets-2";
                    p.DeploymentProcessId = "deploymentprocesses-2";
                    _projects.Add(p);
                    return p;
                }
            );

            // Create deployment process
            var action = new DeploymentActionResource {Name = "Action"};
            action.Environments.Add("environments-1");

            var step = new DeploymentStepResource { Id = "deploymentsteps-1", Name = "Database"};
            step.Actions.Add(action);

            var process = new DeploymentProcessResource();
            process.Steps.Add(step);

            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { "deploymentprocesses-1" }))).Returns(process);
            _copyProcess = new DeploymentProcessResource();
            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { "deploymentprocesses-2" }))).Returns(_copyProcess);

            // Create variable set
            var variable = new VariableResource { Name = "Name", Value = "Value" };
            variable.Scope.Add(ScopeField.Action, "deploymentsactions-1");
            variable.Scope.Add(ScopeField.Environment, "environments-1");
            
            var sourceVariables = new VariableSetResource();
            sourceVariables.Variables.Add(variable);

            octoRepo.Setup(o => o.VariableSets.Get(It.IsIn(new[] { "variablesets-1" }))).Returns(sourceVariables);
            _copyVariables = new VariableSetResource();
            octoRepo.Setup(o => o.VariableSets.Get(It.IsIn(new[] { "variablesets-2" }))).Returns(_copyVariables);
        }

        [Fact]
        public void With_All()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Source")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Octopus");
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);

            var source = _projects[0];
            var copy = _projects[1];
            Assert.Equal("Copy", copy.Name);
            Assert.Equal(source.Description, copy.Description);
            Assert.Equal("projectgroups-1", copy.ProjectGroupId);
            Assert.Equal(source.DefaultToSkipIfAlreadyInstalled, copy.DefaultToSkipIfAlreadyInstalled);
            Assert.Equal(source.IncludedLibraryVariableSetIds, copy.IncludedLibraryVariableSetIds);
            Assert.Equal(source.VersioningStrategy, copy.VersioningStrategy);
            Assert.Equal(source.AutoCreateRelease, copy.AutoCreateRelease);
            Assert.Equal(source.ReleaseCreationStrategy, copy.ReleaseCreationStrategy);
            Assert.Equal(source.IsDisabled, copy.IsDisabled);
            Assert.Equal(source.LifecycleId, copy.LifecycleId);

            var variable = _copyVariables.Variables.FirstOrDefault(x => x.Name == "Name" && x.Value == "Value");
            Assert.NotNull(variable);
            Assert.True(variable.Scope.ContainsKey(ScopeField.Action));
            Assert.Empty(variable.Scope[ScopeField.Action]);
            Assert.True(variable.Scope.ContainsKey(ScopeField.Environment));
            Assert.Equal("environments-1", variable.Scope[ScopeField.Environment].First());

            var steps = _copyProcess.Steps.FirstOrDefault(x => x.Name == "Database");
            Assert.NotNull(steps);
            Assert.Single(steps.Actions);
            Assert.Equal("Action", steps.Actions[0].Name);
            Assert.Equal("environments-1", steps.Actions[0].Environments.First());
        }

        [Fact]
        public void With_Invalid_Group()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Source")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Gibberish");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Octopus");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Source")
                .AddArgument("Copy")
                .AddArgument("Octopus");
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);

            var source = _projects[0];
            var copy = _projects[1];
            Assert.Equal("Copy", copy.Name);
            Assert.Equal(source.Description, copy.Description);
            Assert.Equal("projectgroups-1", copy.ProjectGroupId);
            Assert.Equal(source.DefaultToSkipIfAlreadyInstalled, copy.DefaultToSkipIfAlreadyInstalled);
            Assert.Equal(source.IncludedLibraryVariableSetIds, copy.IncludedLibraryVariableSetIds);
            Assert.Equal(source.VersioningStrategy, copy.VersioningStrategy);
            Assert.Equal(source.AutoCreateRelease, copy.AutoCreateRelease);
            Assert.Equal(source.ReleaseCreationStrategy, copy.ReleaseCreationStrategy);
            Assert.Equal(source.IsDisabled, copy.IsDisabled);
            Assert.Equal(source.LifecycleId, copy.LifecycleId);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
