using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetMachineRoleTests
    {
        private const string CmdletName = "Get-OctoMachineRole";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetMachineRole));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some roles
            var machineRepo = new Mock<IMachineRoleRepository>();
            var roles = new List<string>
            {
                "WebServer",
                "DatbaseServer"
            };

            machineRepo.Setup(m => m.GetAllRoleNames()).Returns(roles);
            octoRepo.Setup(o => o.MachineRoles).Returns(machineRepo.Object);
        }

        [TestMethod]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var results = _ps.Invoke<string>();

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("WebServer");
            var results = _ps.Invoke<string>();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("WebServer", results[0]);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var results = _ps.Invoke<string>();

            Assert.AreEqual(0, results.Count);
        }
    }
}
