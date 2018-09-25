using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class GetEnvironmentTests
    {
        private const string CmdletName = "Get-OctoEnvironment";
        private PowerShell _ps;

        public GetEnvironmentTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetEnvironment));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some environments
            var environments = new List<EnvironmentResource>
            {
                new EnvironmentResource {Name = "Octopus", Id = "environments-1"},
                new EnvironmentResource {Name = "Deploy", Id = "environments-2"},
                new EnvironmentResource {Name = "Automation", Id = "environments-3"},
                new EnvironmentResource {Name = "Server", Id = "environments-4"},
            };
            octoRepo.Setup(o => o.Environments.FindAll(null, null)).Returns(environments);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Equal(4, environments.Count);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Single(environments);
            Assert.Equal("Octopus", environments[0].Name);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Empty(environments);
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "environments-1");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Single(environments);
            Assert.Equal("Octopus", environments[0].Name);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Empty(environments);
        }

        [Fact]
        public void With_ScopeValue()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ScopeValue", new ScopeValue("environments-1"));
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Single(environments);
            Assert.Equal("Octopus", environments[0].Name);
        }

        [Fact]
        public void With_Invalid_ScopeValue()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ScopeValue", new ScopeValue("Gibberish"));
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.Empty(environments);
        }

        [Fact]
        public void With_Id_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Name").AddParameter("Id", "Id");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
