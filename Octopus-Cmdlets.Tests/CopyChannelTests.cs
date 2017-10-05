using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using System.Collections.Generic;
using Octopus.Client;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class CopyChannelTests
    {
        private const string CmdletName = "Copy-OctoChannel";
        private PowerShell _ps;
        private Mock<IOctopusRepository> _octoRepo;
        private ChannelResource _channel;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(CopyChannel));
            _octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create project
            var project = new ProjectResource
            {
                Id = "project-1",
                Name = "Octopus",
                Description = "Test Source",
                DeploymentProcessId = "deploymentprocesses-1",
                VariableSetId = "variablesets-1",
            };

            // Create projects
            _octoRepo.Setup(o => o.Projects.FindByName("Octopus", It.IsAny<string>(), It.IsAny<object>())).Returns(project);
            _octoRepo.Setup(o => o.Projects.FindByName("Gibberish", It.IsAny<string>(), It.IsAny<object>())).Returns((ProjectResource)null);

            // Create channel
            var action = new DeploymentActionResource { Name = "NuGet", ActionType = "NuGet" };
            action.Environments.Add("environments-1");
            action.Properties.Add("Something", "Value");
            action.Properties.Add("SomethingElse", new PropertyValueResource("Secret", true));

            _channel = new ChannelResource
            {
                ProjectId = "project-1",
                Id = "channel-1",
                LifecycleId = "lifecycle-1",
                Name = "Priority",
                Description = "Description",
                IsDefault = true,
                Links = new LinkCollection { ["LinkA1"] = "HrefA1", ["LinkA2"] = "HrefA2" },
                Rules = new List<ChannelVersionRuleResource>
                {
                    new ChannelVersionRuleResource
                    {
                        Id = "version-rule-1",
                        VersionRange = "version-range",
                        Actions = new ReferenceCollection { "action-1", "action-2" },
                        Tag = "tag",
                        Links = new LinkCollection { ["LinkB1"] = "HrefB1", ["LinkB2"] = "HrefB2" },
                    }
                },
                TenantTags = new ReferenceCollection { "tag1", "tag2" }
            };

            _octoRepo.Setup(o => o.Channels.FindByName(project, "Priority")).Returns(_channel);
            _octoRepo.Setup(o => o.Channels.FindByName(It.IsAny<ProjectResource>(), "Default")).Returns((ChannelResource)null);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void Just_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void Just_Name()
        {
            // Execute cmdlet
           _ps.AddCommand(CmdletName).AddParameter("Name", "Priority");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Project_And_Name()
        {
            ChannelResource createdChannel = null;

            _octoRepo
                .Setup(o => o.Channels.Create(It.IsAny<ChannelResource>()))
                .Callback<ChannelResource>(ch => createdChannel = ch)
                .Returns<ChannelResource>(ch => ch);

            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Priority");
            _ps.Invoke();

            Assert.IsNotNull(createdChannel);
            Assert.IsNull(createdChannel.Id);
            Assert.AreEqual(_channel.LifecycleId, createdChannel.LifecycleId);
            Assert.AreEqual(_channel.ProjectId, createdChannel.ProjectId);
            Assert.AreEqual($"{_channel.Name} - Copy", createdChannel.Name);
            Assert.AreEqual(_channel.Description, createdChannel.Description);
            Assert.IsFalse(createdChannel.IsDefault);
            Assert.IsNull(createdChannel.LastModifiedBy);
            Assert.IsNull(createdChannel.LastModifiedOn);
            Assert.IsNotNull(createdChannel.Links);
            Assert.AreNotSame(_channel.Links, createdChannel.Links);
            Assert.IsNotNull(createdChannel.Rules);
            Assert.AreNotSame(_channel.Rules, createdChannel.Rules);
            Assert.IsNotNull(createdChannel.TenantTags);
            Assert.AreNotSame(_channel.TenantTags, createdChannel.TenantTags);
        }
        
        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Gibberish")
                .AddParameter("Name", "Priority");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Project_And_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Default");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Destination()
        {
            ChannelResource createdChannel = null;

            _octoRepo
                .Setup(o => o.Channels.Create(It.IsAny<ChannelResource>()))
                .Callback<ChannelResource>(ch => createdChannel = ch)
                .Returns<ChannelResource>(ch => ch);

            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Priority")
                .AddParameter("Destination", "Passthrough");
            _ps.Invoke();

            Assert.IsNotNull(createdChannel);
            Assert.IsNull(createdChannel.Id);
            Assert.AreEqual(_channel.LifecycleId, createdChannel.LifecycleId);
            Assert.AreEqual(_channel.ProjectId, createdChannel.ProjectId);
            Assert.AreEqual("Passthrough", createdChannel.Name);
            Assert.AreEqual(_channel.Description, createdChannel.Description);
            Assert.IsFalse(createdChannel.IsDefault);
            Assert.IsNull(createdChannel.LastModifiedBy);
            Assert.IsNull(createdChannel.LastModifiedOn);
            Assert.IsNotNull(createdChannel.Links);
            Assert.AreNotSame(_channel.Links, createdChannel.Links);
            Assert.IsNotNull(createdChannel.Rules);
            Assert.AreNotSame(_channel.Rules, createdChannel.Rules);
            Assert.IsNotNull(createdChannel.TenantTags);
            Assert.AreNotSame(_channel.TenantTags, createdChannel.TenantTags);
        }

        [TestMethod]
        public void With_Arguments()
        {
            ChannelResource createdChannel = null;

            _octoRepo
                .Setup(o => o.Channels.Create(It.IsAny<ChannelResource>()))
                .Callback<ChannelResource>(ch => createdChannel = ch)
                .Returns<ChannelResource>(ch => ch);

            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("Priority")
                .AddArgument("Passthrough");
            _ps.Invoke();

            Assert.IsNotNull(createdChannel);
            Assert.IsNull(createdChannel.Id);
            Assert.AreEqual(_channel.LifecycleId, createdChannel.LifecycleId);
            Assert.AreEqual(_channel.ProjectId, createdChannel.ProjectId);
            Assert.AreEqual("Passthrough", createdChannel.Name);
            Assert.AreEqual(_channel.Description, createdChannel.Description);
            Assert.IsFalse(createdChannel.IsDefault);
            Assert.IsNull(createdChannel.LastModifiedBy);
            Assert.IsNull(createdChannel.LastModifiedOn);
            Assert.IsNotNull(createdChannel.Links);
            Assert.AreNotSame(_channel.Links, createdChannel.Links);
            Assert.IsNotNull(createdChannel.Rules);
            Assert.AreNotSame(_channel.Rules, createdChannel.Rules);
            Assert.IsNotNull(createdChannel.TenantTags);
            Assert.AreNotSame(_channel.TenantTags, createdChannel.TenantTags);
        }
    }
}
