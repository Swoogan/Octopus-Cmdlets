using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class GetMachineRoleTests
    {
        private const string CmdletName = "Get-OctoMachineRole";
        private PowerShell _ps;

        public GetMachineRoleTests()
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

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var results = _ps.Invoke<string>();

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("WebServer");
            var results = _ps.Invoke<string>();

            Assert.Single(results);
            Assert.Equal("WebServer", results[0]);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var results = _ps.Invoke<string>();

            Assert.Empty(results);
        }
    }
}
