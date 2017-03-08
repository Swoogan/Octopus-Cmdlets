using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetProjectGroupTests
    {
        private const string CmdletName = "Get-OctoProjectGroup";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (GetProjectGroup));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some project groups
            var groupRepo = new Mock<IProjectGroupRepository>();
            var groupResources = new List<ProjectGroupResource>
            {
                new ProjectGroupResource {Name = "Octopus", Id = "projectgroups-1"},
                new ProjectGroupResource {Name = "Deploy", Id = "projectgroups-2"}
            };

            groupRepo.Setup(p => p.FindAll(It.IsAny<string>(), It.IsAny<object>())).Returns(groupResources);
            groupRepo.Setup(p => p.FindByNames(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<object>())).Returns(
                (string[] names, string path, object pathParams) =>
                    (from n in names
                        from g in groupResources
                        where g.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                        select g).ToList());

            groupRepo.Setup(p => p.Get(It.IsAny<string>())).Returns(CreateGet(groupResources));

            octoRepo.Setup(o => o.ProjectGroups).Returns(groupRepo.Object);
        }

        private static Func<string, ProjectGroupResource> CreateGet(IEnumerable<ProjectGroupResource> groupResources)
        {
            return delegate(string id)
            {
                var group = (from g in groupResources
                    where g.Id.Equals(id,
                        StringComparison.InvariantCultureIgnoreCase)
                    select g).FirstOrDefault();

                if (group != null)
                    return group;

                throw new OctopusResourceNotFoundException("Not found");
            };
        }

        [TestMethod]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var groups = _ps.Invoke<ProjectGroupResource>();

            Assert.AreEqual(2, groups.Count);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var groups = _ps.Invoke<ProjectGroupResource>();

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("Octopus", groups[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var groups = _ps.Invoke<ProjectGroupResource>();

            Assert.AreEqual(0, groups.Count);
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "projectgroups-1");
            var groups = _ps.Invoke<ProjectGroupResource>();

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("Octopus", groups[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "gibberish");
            var groups = _ps.Invoke<ProjectGroupResource>();

            Assert.AreEqual(0, groups.Count);
        }
    }
}
