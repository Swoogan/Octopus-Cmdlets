using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetDeploymentProcessTests
    {
        private const string CmdletName = "Get-OctoDeploymentProcess";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetDeploymentProcess));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            const string deploymentProcessId = "DeploymentProcesses-1";

            // Create a project
            var projectResources = new List<ProjectResource>
            {
                new ProjectResource {Name = "Octopus", DeploymentProcessId = deploymentProcessId}
            };

            octoRepo.Setup(o => o.Projects.FindByNames(new [] {"Octopus"})).Returns(projectResources);
            octoRepo.Setup(o => o.Projects.FindByNames(new[] { "Gibberish" })).Returns(new List<ProjectResource>());


            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { deploymentProcessId }))).Returns(new DeploymentProcessResource());
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
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "DeploymentProcesses-1");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.AreEqual(1, projects.Count);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.AreEqual(0, projects.Count);
        }

        [TestMethod]
        public void With_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.AreEqual(1, projects.Count);
        }

        [TestMethod]
        public void With_Invalid_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.AreEqual(0, projects.Count);
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("DeploymentProcesses-1");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.AreEqual(1, projects.Count);
        }


        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Both()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish").AddParameter("Project", "Gibberish");
            _ps.Invoke();
        }
    }
 }
