using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddProjectGroupTests
    {
        private const string CmdletName = "Add-OctoProjectGroup";
        private PowerShell _ps;
        private readonly List<ProjectGroupResource> _groups = new List<ProjectGroupResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddProjectGroup));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _groups.Clear();

            var repo = new Mock<IProjectGroupRepository>();
            repo.Setup(e => e.Create(It.IsAny<ProjectGroupResource>()))
                .Returns(delegate(ProjectGroupResource p)
                {
                    _groups.Add(p);
                    return p;
                });

            octoRepo.Setup(o => o.ProjectGroups).Returns(repo.Object);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            _ps.Invoke();

            Assert.AreEqual(1, _groups.Count);
            Assert.AreEqual("Octopus", _groups[0].Name);
        }

        [TestMethod]
        public void With_Name_Parameter()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus");
            _ps.Invoke();

            Assert.AreEqual(1, _groups.Count);
            Assert.AreEqual("Octopus", _groups[0].Name);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Description", "Octopus Development Group");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("Octopus Development Group");
            _ps.Invoke();

            Assert.AreEqual(1, _groups.Count);
            Assert.AreEqual("Octopus", _groups[0].Name);
            Assert.AreEqual("Octopus Development Group", _groups[0].Description);
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
