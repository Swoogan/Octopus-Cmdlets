using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class AddNugetFeedTests
    {
        private const string CmdletName = "Add-OctoNugetFeed";
        private PowerShell _ps;
        private readonly List<FeedResource> _feeds = new List<FeedResource>();

        public AddNugetFeedTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddNugetFeed));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _feeds.Clear();

            var feedRepo = new Mock<IFeedRepository>();
            feedRepo.Setup(e => e.Create(It.IsAny<FeedResource>(), null))
                .Returns(delegate(FeedResource e)
                {
                    _feeds.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.Feeds).Returns(feedRepo.Object);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus_Dev");
            _ps.Invoke();

            Assert.Equal(1, _feeds.Count);
            Assert.Equal("Octopus_Dev", _feeds[0].Name);
        }

        [Fact]
        public void With_Uri()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Uri", "\\test");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        //[Fact]
        //public void With_Name_And_Description()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus_Dev").AddParameter("Uri", "\\test");
        //    _ps.Invoke();

        //    Assert.Equal(1, _feeds.Count);
        //    Assert.Equal("Octopus_Dev", _feeds[0].Name);
        //    Assert.Equal("\\test", _feeds[0].FeedUri);
        //}

        //[Fact]
        //public void With_Arguements()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddArgument("Octopus_Dev")
        //        .AddArgument("\\test");
        //    _ps.Invoke();

        //    Assert.Equal(1, _feeds.Count);
        //    Assert.Equal("Octopus_Dev", _feeds[0].Name);
        //    Assert.Equal("\\test", _feeds[0].FeedUri);
        //}

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
