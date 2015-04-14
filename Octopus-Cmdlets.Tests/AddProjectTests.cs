using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddProjectTests
    {
        private const string CmdletName = "Add-OctoProject";
        private PowerShell _ps;
        private readonly List<ProjectResource> _projects = new List<ProjectResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddProject));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project group
            var groupResource = new ProjectGroupResource {Name = "Octopus", Id = "projectgroups-1"};
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Octopus")).Returns(groupResource);

            octoRepo.Setup(o => o.ProjectGroups.Get(It.IsIn(new[] { "projectgroups-1" })))
                .Returns(groupResource);

            octoRepo.Setup(o => o.ProjectGroups.Get(It.IsNotIn(new[] { "projectgroups-1" })))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            _projects.Clear();

            var repo = new Mock<IProjectRepository>();
            repo.Setup(e => e.Create(It.IsAny<ProjectResource>()))
                .Returns(delegate(ProjectResource p)
                {
                    _projects.Add(p);
                    return p;
                });

            octoRepo.Setup(o => o.Projects).Returns(repo.Object);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_ProjectGroup()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroup", "Octopus");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_ProjectGroup()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroup", "Gibberish").AddParameter("Name", "Octopus");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_ProjectGroupId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroupId", "projectgroups-1");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_ProjectGroupId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroupId", "Gibberish").AddParameter("Name", "Octopus");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroup", "Octopus").AddParameter("Name", "Octopus");
            _ps.Invoke();

            Assert.AreEqual(1, _projects.Count);
            Assert.AreEqual("Octopus", _projects[0].Name);
        }

        [TestMethod]
        public void ById_With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ProjectGroupId", "projectgroups-1").AddParameter("Name", "Octopus");
            _ps.Invoke();

            Assert.AreEqual(1, _projects.Count);
            Assert.AreEqual("Octopus", _projects[0].Name);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Description", "Octopus Development Project");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Group_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("ProjectGroup", "Octopus")
                .AddParameter("Description", "Octopus Development Project");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("ProjectGroup", "Octopus")
                .AddParameter("Name", "Octopus")
                .AddParameter("Description", "Octopus Development Project");
            _ps.Invoke();

            Assert.AreEqual(1, _projects.Count);
            Assert.AreEqual("Octopus", _projects[0].Name);
            Assert.AreEqual("projectgroups-1", _projects[0].ProjectGroupId);
            Assert.AreEqual("Octopus Development Project", _projects[0].Description);
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("Octopus")
                .AddArgument("Octopus Development Project");
            _ps.Invoke();

            Assert.AreEqual(1, _projects.Count);
            Assert.AreEqual("Octopus", _projects[0].Name);
            Assert.AreEqual("projectgroups-1", _projects[0].ProjectGroupId);
            Assert.AreEqual("Octopus Development Project", _projects[0].Description);
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
