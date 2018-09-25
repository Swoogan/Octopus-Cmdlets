using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class GetActionTests
    {
        private const string CmdletName = "Get-OctoAction";
        private PowerShell _ps;

        public GetActionTests()
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

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Equal(2, actions.Count);
        }

        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Do Stuff");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Single(actions);
            Assert.Equal("Do Stuff", actions[0].Name);
        }

        [Fact]
        public void With_Name_Parameter()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Name", "Do Stuff");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Single(actions);
            Assert.Equal("Do Stuff", actions[0].Name);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Gibberish");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Empty(actions);
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Id", "Globally unique identifier");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Single(actions);
            Assert.Equal("Do Stuff", actions[0].Name);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Id", "Gibberish");
            var actions = _ps.Invoke<DeploymentActionResource>();

            Assert.Empty(actions);
        }
    }
}
