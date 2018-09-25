using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class AddEnvironmentTests
    {
        private const string CmdletName = "Add-OctoEnvironment";
        private PowerShell _ps;
        private readonly List<EnvironmentResource> _envs = new List<EnvironmentResource>();

        public AddEnvironmentTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (AddEnvironment));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _envs.Clear();

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.Create(It.IsAny<EnvironmentResource>(), It.IsAny<object>()))
                .Returns((EnvironmentResource e, object o) =>
                {
                    _envs.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.Environments).Returns(envRepo.Object);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus_Dev");
            _ps.Invoke();

            Assert.Single(_envs);
            Assert.Equal("Octopus_Dev", _envs[0].Name);
        }

        [Fact]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Description", "Octopus Development environment");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).
                AddParameter("Name", "Octopus_Dev").
                AddParameter("Description", "Octopus Development environment");
            _ps.Invoke();

            Assert.Single(_envs);
            Assert.Equal("Octopus_Dev", _envs[0].Name);
            Assert.Equal("Octopus Development environment", _envs[0].Description);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus_Dev")
                .AddArgument("Octopus Development environment");
            _ps.Invoke();

            Assert.Single(_envs);
            Assert.Equal("Octopus_Dev", _envs[0].Name);
            Assert.Equal("Octopus Development environment", _envs[0].Description);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
