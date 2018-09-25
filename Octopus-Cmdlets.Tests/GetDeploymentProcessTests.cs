using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class GetDeploymentProcessTests
    {
        private const string CmdletName = "Get-OctoDeploymentProcess";
        private PowerShell _ps;

        public GetDeploymentProcessTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetDeploymentProcess));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            const string deploymentProcessId = "DeploymentProcesses-1";

            // Create a project
            var projectResources = new List<ProjectResource>
            {
                new ProjectResource {Name = "Octopus", DeploymentProcessId = deploymentProcessId}
            };

            octoRepo.Setup(o => o.Projects.FindByNames(new [] {"Octopus"}, null, null)).Returns(projectResources);
            octoRepo.Setup(o => o.Projects.FindByNames(new[] { "Gibberish" }, null, null)).Returns(new List<ProjectResource>());


            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { deploymentProcessId }))).Returns(new DeploymentProcessResource());
            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsNotIn(new[] {deploymentProcessId})))
                .Throws(new OctopusResourceNotFoundException("Not Found"));
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "DeploymentProcesses-1");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.Equal(1, projects.Count);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.Equal(0, projects.Count);
        }

        [Fact]
        public void With_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.Equal(1, projects.Count);
        }

        [Fact]
        public void With_Invalid_ProjectName()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.Equal(0, projects.Count);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("DeploymentProcesses-1");
            var projects = _ps.Invoke<DeploymentProcessResource>();

            Assert.Equal(1, projects.Count);
        }


        [Fact]
        public void With_Both()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish").AddParameter("Project", "Gibberish");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
 }
