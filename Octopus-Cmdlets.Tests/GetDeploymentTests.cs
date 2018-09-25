using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Octopus.Client.Model;
using Octopus.Client.Extensibility;

namespace Octopus_Cmdlets.Tests
{
    public class GetDeploymentTests
    {
        private const string CmdletName = "Get-OctoDeployment";
        private PowerShell _ps;

        public GetDeploymentTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetDeployment));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project
            var projectResource = new ProjectResource {Name = "Octopus"};
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", null, null)).Returns(projectResource);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", null, null)).Returns((ProjectResource) null);

            // Create a Release
            var release = new ReleaseResource { Id = "Releases-1", Links = new LinkCollection()};

            release.Links.Add("Deployments", "/api/releases/releases-1/deployments");
            octoRepo.Setup(o => o.Projects.GetReleaseByVersion(projectResource, "1.0.0")).Returns(release);
            octoRepo.Setup(o => o.Projects.GetReleaseByVersion(projectResource, "Gibberish")).Returns((ReleaseResource) null);

            // Create Deployments
            var deployments = new ResourceCollection<DeploymentResource>(new List<DeploymentResource>
            {
                new DeploymentResource {Id = "deployments-1"}
            }, new LinkCollection());
            octoRepo.Setup(o => o.Releases.GetDeployments(release, 0, null)).Returns(deployments);
            octoRepo.Setup(o => o.Releases.Get("Releases-1")).Returns(release);
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
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project_And_Release()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Release", "1.0.0");
            var deployments = _ps.Invoke<DeploymentResource>();

            Assert.Equal(1, deployments.Count);
        }

        [Fact]
        public void With_Invalid_Project_And_Release()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Release", "1.0.0");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project_And_Invalid_Release()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Release", "Gibberish");
            var deployments = _ps.Invoke<DeploymentResource>();

            Assert.Equal(0, deployments.Count);
        }

        [Fact]
        public void With_Project_And_ReleaseId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("ReleaseId", "Releases-1");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_ReleaseId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ReleaseId", "Releases-1");
            var deployments = _ps.Invoke<DeploymentResource>();

            Assert.Equal(1, deployments.Count);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("1.0.0");
            var deployments = _ps.Invoke<DeploymentResource>();

            Assert.Equal(1, deployments.Count);
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
