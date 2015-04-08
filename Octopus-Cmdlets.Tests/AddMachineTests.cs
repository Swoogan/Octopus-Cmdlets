using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddMachineTests
    {
        private PowerShell _ps;
        private readonly List<EnvironmentResource> _envs = new List<EnvironmentResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell("Add-OctoMachine", typeof (AddMachine));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _envs.Clear();

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.Create(It.IsAny<EnvironmentResource>()))
                .Returns(delegate(EnvironmentResource e)
                {
                    _envs.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.Environments).Returns(envRepo.Object);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoEnvironment").AddArgument("Octopus_Dev");
            _ps.Invoke();

            Assert.AreEqual(1, _envs.Count);
            Assert.AreEqual("Octopus_Dev", _envs[0].Name);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoEnvironment").AddParameter("Description", "Octopus Development environment");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoEnvironment")
                .AddArgument("Octopus_Dev")
                .AddArgument("Octopus Development environment");
            _ps.Invoke();

            Assert.AreEqual(1, _envs.Count);
            Assert.AreEqual("Octopus_Dev", _envs[0].Name);
            Assert.AreEqual("Octopus Development environment", _envs[0].Description);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoEnvironment");
            _ps.Invoke();
        }
    }
}
