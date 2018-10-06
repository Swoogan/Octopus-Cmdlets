using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class GetMachineTests
    {
        private const string CmdletName = "Get-OctoMachine";
        private PowerShell _ps;

        public GetMachineTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetMachine));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            var machine = new MachineResource {Name = "dbserver-01", Id = "Machines-1"};
            var machines = new List<MachineResource>
            {
                machine,
                new MachineResource {Name = "dbserver-02", Id = "Machines-2"}
            };
            
            octoRepo.Setup(o => o.Machines.FindAll(null, null)).Returns(machines);
            octoRepo.Setup(o => o.Machines.FindByNames(new[] { "dbserver-01" }, null, null)).Returns(new List<MachineResource> { machine });
            octoRepo.Setup(o => o.Machines.FindByNames(new[] { "Gibberish" }, null, null)).Returns(new List<MachineResource>());

            octoRepo.Setup(o => o.Machines.Get("Machines-1")).Returns(machine);
            octoRepo.Setup(o => o.Machines.Get(It.Is((string s) => s != "Machines-1"))).Throws(new OctopusResourceNotFoundException("Not Found")); 
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var machines = _ps.Invoke<MachineResource>();

            Assert.Equal(2, machines.Count);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] {"dbserver-01"});
            var machines = _ps.Invoke<MachineResource>();

            Assert.Single(machines);
            Assert.Equal("dbserver-01", machines[0].Name);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Gibberish" });
            var machines = _ps.Invoke<MachineResource>();

            Assert.Empty(machines);
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "Machines-1" });
            var machines = _ps.Invoke<MachineResource>();

            Assert.Single(machines);
            Assert.Equal("dbserver-01", machines[0].Name);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "Gibberish" });
            var machines = _ps.Invoke<MachineResource>();

            Assert.Empty(machines);
        }

        [Fact]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", new[] {"dbserver-01"})
                .AddParameter("Id", new[] {"Machines-1"});
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument(new[] { "dbserver-01" });
            var machines = _ps.Invoke<MachineResource>();

            Assert.Single(machines);
            Assert.Equal("dbserver-01", machines[0].Name);
        }
    }
}
