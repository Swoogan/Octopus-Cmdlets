using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetReleaseTests
    {
        private const string CmdletName = "Get-OctoRelease";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetRelease));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project
            var project = new ProjectResource { Name = "Octopus" };
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", null, null)).Returns(project);
            octoRepo.Setup(o => o.Projects.Get("projects-1")).Returns(project);
            octoRepo.Setup(o => o.Projects.Get("Gibberish")).Throws(new OctopusResourceNotFoundException("Not Found"));

            var releases = new List<ReleaseResource>
            {
                new ReleaseResource {Version = "1.0.0"},
                new ReleaseResource {Version = "1.0.1"},
                new ReleaseResource {Version = "1.1.0"}
            };

            octoRepo.Setup(o => o.Projects.GetReleases(project, 0, null, null))
                .Returns(new ResourceCollection<ReleaseResource>(releases, new LinkCollection()));

            octoRepo.Setup(o => o.Projects.GetReleaseByVersion(project, "1.0.0")).Returns(releases[0]);
            octoRepo.Setup(o => o.Projects.GetReleaseByVersion(project, "Gibberish")).Throws(new OctopusResourceNotFoundException("Not found"));

            octoRepo.Setup(o => o.Releases.Get("releases-1")).Returns(releases[0]);
            octoRepo.Setup(o => o.Releases.Get("Gibberish")).Throws(new OctopusResourceNotFoundException("Not found"));
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
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(3, releases.Count);
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_ProjectId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectId", "projects-1");
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(3, releases.Count);
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_ProjectId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectId", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Project_And_Version()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Version", new[] {"1.0.0"});
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(1, releases.Count);
            Assert.AreEqual("1.0.0", releases[0].Version);
        }

        [TestMethod]
        public void With_Project_And_Invalid_Version()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Version", new[] { "Gibberish" });
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(0, releases.Count);
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "releases-1");
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(1, releases.Count);
            Assert.AreEqual("1.0.0", releases[0].Version);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var releases = _ps.Invoke<ReleaseResource>();

            Assert.AreEqual(0, releases.Count);
        }
    }
}
