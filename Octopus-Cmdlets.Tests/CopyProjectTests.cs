using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class CopyProjectTests
    {
        private const string CmdletName = "Copy-OctoProject";
        private PowerShell _ps;
        private readonly List<ProjectResource> _projects = new List<ProjectResource>();
        private VariableSetResource _copyVariables;
        private DeploymentProcessResource _copyProcess;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (CopyProject));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project group
            var groupResource = new ProjectGroupResource {Name = "Octopus", Id = "projectgroups-1"};
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Octopus")).Returns(groupResource);
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Gibberish")).Returns((ProjectGroupResource) null);

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

            octoRepo.Setup(o => o.Projects.FindByName("Source")).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish")).Returns((ProjectResource) null);
            octoRepo.Setup(o => o.Projects.Create(It.IsAny<ProjectResource>())).Returns(
                delegate(ProjectResource p)
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

        [TestMethod]
        public void With_All()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Source")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Octopus");
            _ps.Invoke();

            Assert.AreEqual(2, _projects.Count);

            var source = _projects[0];
            var copy = _projects[1];
            Assert.AreEqual("Copy", copy.Name);
            Assert.AreEqual(source.Description, copy.Description);
            Assert.AreEqual("projectgroups-1", copy.ProjectGroupId);
            Assert.AreEqual(source.DefaultToSkipIfAlreadyInstalled, copy.DefaultToSkipIfAlreadyInstalled);
            CollectionAssert.AreEqual(source.IncludedLibraryVariableSetIds, copy.IncludedLibraryVariableSetIds);
            Assert.AreEqual(source.VersioningStrategy, copy.VersioningStrategy);
            Assert.AreEqual(source.AutoCreateRelease, copy.AutoCreateRelease);
            Assert.AreEqual(source.ReleaseCreationStrategy, copy.ReleaseCreationStrategy);
            Assert.AreEqual(source.IsDisabled, copy.IsDisabled);
            Assert.AreEqual(source.LifecycleId, copy.LifecycleId);

            var variable = _copyVariables.Variables.FirstOrDefault(x => x.Name == "Name" && x.Value == "Value");
            Assert.IsNotNull(variable);
            Assert.IsTrue(variable.Scope.ContainsKey(ScopeField.Action));
            Assert.AreEqual(0, variable.Scope[ScopeField.Action].Count);
            Assert.IsTrue(variable.Scope.ContainsKey(ScopeField.Environment));
            Assert.AreEqual("environments-1", variable.Scope[ScopeField.Environment].First());

            var steps = _copyProcess.Steps.FirstOrDefault(x => x.Name == "Database");
            Assert.IsNotNull(steps);
            Assert.AreEqual(1, steps.Actions.Count);
            Assert.AreEqual("Action", steps.Actions[0].Name);
            Assert.AreEqual("environments-1", steps.Actions[0].Environments.First());
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Group()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Source")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("Destination", "Copy")
                .AddParameter("ProjectGroup", "Octopus");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Source")
                .AddArgument("Copy")
                .AddArgument("Octopus");
            _ps.Invoke();

            Assert.AreEqual(2, _projects.Count);

            var source = _projects[0];
            var copy = _projects[1];
            Assert.AreEqual("Copy", copy.Name);
            Assert.AreEqual(source.Description, copy.Description);
            Assert.AreEqual("projectgroups-1", copy.ProjectGroupId);
            Assert.AreEqual(source.DefaultToSkipIfAlreadyInstalled, copy.DefaultToSkipIfAlreadyInstalled);
            CollectionAssert.AreEqual(source.IncludedLibraryVariableSetIds, copy.IncludedLibraryVariableSetIds);
            Assert.AreEqual(source.VersioningStrategy, copy.VersioningStrategy);
            Assert.AreEqual(source.AutoCreateRelease, copy.AutoCreateRelease);
            Assert.AreEqual(source.ReleaseCreationStrategy, copy.ReleaseCreationStrategy);
            Assert.AreEqual(source.IsDisabled, copy.IsDisabled);
            Assert.AreEqual(source.LifecycleId, copy.LifecycleId);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }
    }
}
