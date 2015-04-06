using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetExternalFeedTests
    {
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell("Get-OctoExternalFeed", typeof(GetExternalFeed));
        }
        
        [TestMethod]
        public void No_Arguments()
        {
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            var feedRepo = new Mock<IFeedRepository>();
            var feedResources = new List<FeedResource>
            {
                new FeedResource {FeedUri = @"\\someshare\octopus", Name = "Octopus"},
                new FeedResource {FeedUri = @"\\someshare\deploy", Name = "Deploy"}
            };

            feedRepo.Setup(f => f.FindAll()).Returns(feedResources);
            octoRepo.Setup(f => f.Feeds).Returns(feedRepo.Object);

            _ps.AddCommand("Get-OctoExternalFeed");
            var feeds = _ps.Invoke<FeedResource>();
            Assert.AreEqual(feeds.Count, 2);
        }

        [TestMethod]
        public void With_Name()
        {
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);
            var feedRepo = new Mock<IFeedRepository>();

            feedRepo.Setup(f => f.FindByNames(new List<string> {"Octopus"})).Returns(
                new List<FeedResource>
                {
                    new FeedResource {FeedUri = @"\\someshare\octopus", Name = "Octopus"},
                });

            octoRepo.Setup(f => f.Feeds).Returns(feedRepo.Object);

            _ps.AddCommand("Get-OctoExternalFeed").AddArgument("Octopus");
            var feeds = _ps.Invoke<FeedResource>();
            Assert.AreEqual(feeds.Count, 1);
            Assert.AreEqual(feeds[0].Name, "Octopus");
        }
    }
}
