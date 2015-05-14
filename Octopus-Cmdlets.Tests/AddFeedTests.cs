using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddFeedTests
    {
        private const string CmdletName = "Add-OctoFeed";
        private PowerShell _ps;
        private readonly List<FeedResource> _feeds = new List<FeedResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddFeed));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _feeds.Clear();

            var feedRepo = new Mock<IFeedRepository>();
            feedRepo.Setup(e => e.Create(It.IsAny<FeedResource>()))
                .Returns(delegate(FeedResource e)
                {
                    _feeds.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.Feeds).Returns(feedRepo.Object);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus_Dev");
            _ps.Invoke();

            Assert.AreEqual(1, _feeds.Count);
            Assert.AreEqual("Octopus_Dev", _feeds[0].Name);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Uri()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Uri", "\\test");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus_Dev").AddParameter("Uri", "\\test");
            _ps.Invoke();

            Assert.AreEqual(1, _feeds.Count);
            Assert.AreEqual("Octopus_Dev", _feeds[0].Name);
            Assert.AreEqual("\\test", _feeds[0].FeedUri);
        }

        [TestMethod]
        public void With_Arguements()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus_Dev")
                .AddArgument("\\test");
            _ps.Invoke();

            Assert.AreEqual(1, _feeds.Count);
            Assert.AreEqual("Octopus_Dev", _feeds[0].Name);
            Assert.AreEqual("\\test", _feeds[0].FeedUri);
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
