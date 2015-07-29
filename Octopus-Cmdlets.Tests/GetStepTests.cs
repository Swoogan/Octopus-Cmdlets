using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetStepTests
    {
        private const string CmdletName = "Get-OctoStep";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (GetStep));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            const string deploymentProcessId = "deploymentprocess-projects-1";

            // Create a project
            var projectResources = new List<ProjectResource>
            {
                new ProjectResource {Name = "Octopus", DeploymentProcessId = deploymentProcessId, Id = "projects-1"}
            };

            octoRepo.Setup(o => o.Projects.FindByNames(new[] {"Octopus"})).Returns(projectResources);
            octoRepo.Setup(o => o.Projects.FindByNames(new[] {"Gibberish"})).Returns(new List<ProjectResource>());
            octoRepo.Setup(o => o.Projects.Get("projects-1")).Returns(projectResources[0]);
            octoRepo.Setup(o => o.Projects.Get("Gibberish")).Throws(new OctopusResourceNotFoundException("Not Found"));

            var process = new DeploymentProcessResource();
            //{
            //    Id = "deploymentprocess-projects-1"
            //};
            process.Steps.Add(new DeploymentStepResource { Name = "Test Step" });
            process.Steps.Add(new DeploymentStepResource { Name = "Test Step 2" });

            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] {deploymentProcessId})))
                .Returns(process);

            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsNotIn(new[] {deploymentProcessId})))
                .Throws(new OctopusResourceNotFoundException("Not Found"));
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod]
        public void With_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("Test Step", steps[0].Name);
        }

        [TestMethod]
        public void With_Invalid_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(0, steps.Count);
        }

        [TestMethod]
        public void With_ProjectId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectId", "projects-1");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("Test Step", steps[0].Name);
        }

        [TestMethod]
        public void With_Invalid_ProjectId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectId", "Gibberish");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(0, steps.Count);
        }

        [TestMethod]
        public void With_ProcessId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("DeploymentProcessId", "deploymentprocess-projects-1");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("Test Step", steps[0].Name);
        }

        [TestMethod]
        public void With_Invalid_ProcessId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("DeploymentProcessId", "Gibberish");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(0, steps.Count);
        }

        [TestMethod]
        public void With_ProjectAndName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Test Step 2");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(1, steps.Count);
            Assert.AreEqual("Test Step 2", steps[0].Name);
        }

        [TestMethod]
        public void With_ProjectIdAndName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectId", "projects-1").AddParameter("Name", "Test Step 2");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(1, steps.Count);
            Assert.AreEqual("Test Step 2", steps[0].Name);
        }

        [TestMethod]
        public void With_ProcessIdAndName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("DeploymentProcessId", "deploymentprocess-projects-1")
                .AddParameter("Name", "Test Step 2");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(1, steps.Count);
            Assert.AreEqual("Test Step 2", steps[0].Name);
        }

        
        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Test Step 2");
            var steps = _ps.Invoke<DeploymentStepResource>();

            Assert.AreEqual(1, steps.Count);
            Assert.AreEqual("Test Step 2", steps[0].Name);
        }

        
        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_KitchenSink()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Gibberish")
                .AddParameter("ProjectId", "Gibberish")
                .AddParameter("DeploymentProcessId", "Gibberish");
            _ps.Invoke();
        }
    }
 }
