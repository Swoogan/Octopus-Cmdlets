using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetEnvironmentTests
    {
        private const string CmdletName = "Get-OctoEnvironment";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
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
            octoRepo.Setup(o => o.Environments.FindAll()).Returns(environments);
        }

        [TestMethod]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(4, environments.Count);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(1, environments.Count);
            Assert.AreEqual("Octopus", environments[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(0, environments.Count);
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "environments-1");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(1, environments.Count);
            Assert.AreEqual("Octopus", environments[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(0, environments.Count);
        }

        [TestMethod]
        public void With_ScopeValue()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ScopeValue", new ScopeValue("environments-1"));
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(1, environments.Count);
            Assert.AreEqual("Octopus", environments[0].Name);
        }

        [TestMethod]
        public void With_Invalid_ScopeValue()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("ScopeValue", new ScopeValue("Gibberish"));
            var environments = _ps.Invoke<EnvironmentResource>();

            Assert.AreEqual(0, environments.Count);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Id_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Name").AddParameter("Id", "Id");
            _ps.Invoke();
        }
    }
}
