using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetExternalFeedTests
    {
        private const string CmdletName = "Get-OctoExternalFeed";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (GetExternalFeed));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create feeds
            var feedRepo = new Mock<IFeedRepository>();
            var feedResources = new List<FeedResource>
            {
                new FeedResource {FeedUri = @"\\someshare\octopus", Name = "Octopus"},
                new FeedResource {FeedUri = @"\\someshare\deploy", Name = "Deploy"}
            };

            feedRepo.Setup(f => f.FindAll()).Returns(feedResources);
            feedRepo.Setup(f => f.FindByNames(It.IsAny<string[]>())).Returns(
                (string[] names) => (from n in names
                    from f in feedResources
                    where n.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase)
                    select f).ToList());

            octoRepo.Setup(o => o.Feeds).Returns(feedRepo.Object);
        }

        [TestMethod]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var feeds = _ps.Invoke<FeedResource>();

            Assert.AreEqual(2, feeds.Count);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var feeds = _ps.Invoke<FeedResource>();

            Assert.AreEqual(1, feeds.Count);
            Assert.AreEqual("Octopus", feeds[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var feeds = _ps.Invoke<FeedResource>();

            Assert.AreEqual(0, feeds.Count);
        }
    }
}
