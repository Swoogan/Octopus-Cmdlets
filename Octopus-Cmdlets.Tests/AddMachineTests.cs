using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using Octopus.Client.Model.Endpoints;

namespace Octopus_Cmdlets.Tests
{
    public class AddMachineTests
    {
        private PowerShell _ps;
        private readonly List<MachineResource> _machines = new List<MachineResource>();

        public AddMachineTests()
        {
            _ps = Utilities.CreatePowerShell("Add-OctoMachine", typeof (AddMachine));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create an environment
            var environments = new List<EnvironmentResource>
            {
                new EnvironmentResource {Name = "Octopus_Dev", Id = "environments-1"}
            };
            octoRepo.Setup(o => o.Environments.FindAll(It.IsAny<string>(), It.IsAny<object>())).Returns(environments);
            octoRepo.Setup(o => o.Environments.FindByName("Octopus_Dev", It.IsAny<string>(), It.IsAny<object>())).Returns(environments[0]);
            _machines.Clear();

            var machineRepo = new Mock<IMachineRepository>();
            machineRepo.Setup(m => m.Create(It.IsAny<MachineResource>(), It.IsAny<object>()))
                .Returns((MachineResource m, object o) => 
                {
                    _machines.Add(m);
                    return m;
                });
            
            octoRepo.Setup(o => o.Machines).Returns(machineRepo.Object);
        }

        [Fact]
        public void With_EnvName()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("Environment", new[] { "Octopus_Dev" })
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Roles", new[] { "Role1", "Role2" } )
                .AddParameter("Endpoint", new ListeningTentacleEndpointResource { Uri = "https://server.domain:port/", Thumbprint = "ThisIsMyThumbprint" });
            _ps.Invoke();

            Assert.Single(_machines);
            Assert.Equal(new ReferenceCollection("environments-1").ToString(), _machines[0].EnvironmentIds.ToString());
            Assert.Equal("Tentacle_Name", _machines[0].Name);
            Assert.Equal("ThisIsMyThumbprint", ((ListeningTentacleEndpointResource)_machines[0].Endpoint).Thumbprint);
            Assert.Equal(new ReferenceCollection() { "Role1", "Role2" }.ToString(), _machines[0].Roles.ToString());
            Assert.Equal("https://server.domain:port/", ((ListeningTentacleEndpointResource)_machines[0].Endpoint).Uri);
            Assert.Equal(CommunicationStyle.TentaclePassive, _machines[0].Endpoint.CommunicationStyle);
        }

        [Fact]
        public void With_EnvId()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", new[] { "environments-1" } )
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Roles", new[] { "Role1", "Role2" } )
                .AddParameter("Endpoint", new ListeningTentacleEndpointResource { Uri = "https://server.domain:port/", Thumbprint = "ThisIsMyThumbprint" });
            _ps.Invoke();

            Assert.Single(_machines);
            Assert.Equal(new ReferenceCollection("environments-1").ToString(), _machines[0].EnvironmentIds.ToString());
            Assert.Equal("Tentacle_Name", _machines[0].Name);
            Assert.Equal("ThisIsMyThumbprint", ((ListeningTentacleEndpointResource)_machines[0].Endpoint).Thumbprint);
            Assert.Equal(new ReferenceCollection() { "Role1", "Role2" }.ToString(), _machines[0].Roles.ToString());
            Assert.Equal("https://server.domain:port/", ((ListeningTentacleEndpointResource)_machines[0].Endpoint).Uri);
            Assert.Equal(CommunicationStyle.TentaclePassive, _machines[0].Endpoint.CommunicationStyle);
        }

        [Fact]
        public void Missing_Arguments1()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                //.AddParameter("EnvironmentId", "environments-1")
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", "Role1,Role2")
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Missing_Arguments2()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", "environments-1")
                //.AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", "Role1,Role2")
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Missing_Arguments3()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", "environments-1")
                .AddParameter("Name", "Tentacle_Name")
                //.AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", "Role1,Role2")
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Missing_Arguments4()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", "environments-1")
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                //.AddParameter("Roles", "Role1,Role2")
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Missing_Arguments5()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", "environments-1")
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", "Role1,Role2")
                //.AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Missing_Arguments6()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", "environments-1")
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", "Role1,Role2")
                .AddParameter("Uri", "https://server.domain:port/")
                //.AddParameter("CommunicationStyle", "TentaclePassive")
                ;
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
