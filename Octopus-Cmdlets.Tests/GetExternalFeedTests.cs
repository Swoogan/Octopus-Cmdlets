using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class GetExternalFeedTests
    {
        private const string CmdletName = "Get-OctoExternalFeed";
        private PowerShell _ps;

        public GetExternalFeedTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (GetExternalFeed));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create feeds
            var feedRepo = new Mock<IFeedRepository>();
            var feedResources = new List<FeedResource>
            {
                new NuGetFeedResource {FeedUri = @"\\someshare\octopus", Name = "Octopus"},
                new NuGetFeedResource {FeedUri = @"\\someshare\deploy", Name = "Deploy"}
            };

            feedRepo.Setup(f => f.FindAll(null, null)).Returns(feedResources);
            feedRepo.Setup(f => f.FindByNames(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<object>())).Returns(
                (string[] names, string path, string pathParams) => (from n in names
                    from f in feedResources
                    where n.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase)
                    select f).ToList());

            octoRepo.Setup(o => o.Feeds).Returns(feedRepo.Object);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var feeds = _ps.Invoke<FeedResource>();

            Assert.Equal(2, feeds.Count);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var feeds = _ps.Invoke<FeedResource>();

            Assert.Single(feeds);
            Assert.Equal("Octopus", feeds[0].Name);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var feeds = _ps.Invoke<FeedResource>();

            Assert.Empty(feeds);
        }
    }
}
