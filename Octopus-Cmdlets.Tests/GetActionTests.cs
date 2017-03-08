using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetActionTests
    {
        private const string CmdletName = "Get-OctoAction";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetAction));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project
            var projectResource = new ProjectResource {Name = "Octopus"};
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", null, null)).Returns(projectResource);

            // Create a deployment process
            var stepResource = new DeploymentStepResource();
            stepResource.Actions.Add(new DeploymentActionResource { Name = "Do Stuff", Id = "Globally unique identifier"});
            stepResource.Actions.Add(new DeploymentActionResource { Name = "Do Other Stuff", Id = "Universally unique identifier" });

            var dpResource = new DeploymentProcessResource();
            dpResource.Steps.Add(stepResource);

            var dpRepo = new Mock<IDeploymentProcessRepository>();
            dpRepo.Setup(d => d.Get(It.IsAny<string>())).Returns(dpResource);
            
            octoRepo.Setup(o => o.DeploymentProcesses).Returns(dpRepo.Object);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(2, actions.Count);
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Do Stuff");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual("Do Stuff", actions[0].Name);
        }

        [TestMethod]
        public void With_Name_Parameter()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Name", "Do Stuff");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual("Do Stuff", actions[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Gibberish");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(0, actions.Count);
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Id", "Globally unique identifier");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual("Do Stuff", actions[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Id", "Gibberish");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.AreEqual(0, actions.Count);
        }
    }
}
