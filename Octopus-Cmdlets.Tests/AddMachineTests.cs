using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using Octopus.Platform.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddMachineTests
    {
        private PowerShell _ps;
        private readonly List<MachineResource> _machines = new List<MachineResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell("Add-OctoMachine", typeof (AddMachine));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create an environment
            var environments = new List<EnvironmentResource>
            {
                new EnvironmentResource {Name = "Octopus_Dev", Id = "environments-1"}
            };
            octoRepo.Setup(o => o.Environments.FindAll()).Returns(environments);
            octoRepo.Setup(o => o.Environments.FindByName("Octopus_Dev")).Returns(environments[0]);
            _machines.Clear();

            var machineRepo = new Mock<IMachineRepository>();
            machineRepo.Setup(m => m.Create(It.IsAny<MachineResource>()))
                .Returns(delegate(MachineResource m)
                {
                    _machines.Add(m);
                    return m;
                });
            
            octoRepo.Setup(o => o.Machines).Returns(machineRepo.Object);
        }

        [TestMethod]
        public void With_EnvName()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("Environment", new[] { "Octopus_Dev" })
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", new[] { "Role1", "Role2" } )
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            _ps.Invoke();

            Assert.AreEqual(1, _machines.Count);
            Assert.AreEqual(new ReferenceCollection("environments-1").ToString(), _machines[0].EnvironmentIds.ToString());
            Assert.AreEqual("Tentacle_Name", _machines[0].Name);
            Assert.AreEqual("ThisIsMyThumbprint", _machines[0].Thumbprint);
            Assert.AreEqual(new ReferenceCollection() { "Role1", "Role2" }.ToString(), _machines[0].Roles.ToString());
            Assert.AreEqual("https://server.domain:port/", _machines[0].Uri);
            Assert.AreEqual(CommunicationStyle.TentaclePassive, _machines[0].CommunicationStyle);
        }

        [TestMethod]
        public void With_EnvId()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine")
                .AddParameter("EnvironmentId", new[] { "environments-1" } )
                .AddParameter("Name", "Tentacle_Name")
                .AddParameter("Thumbprint", "ThisIsMyThumbprint")
                .AddParameter("Roles", new[] { "Role1", "Role2" } )
                .AddParameter("Uri", "https://server.domain:port/")
                .AddParameter("CommunicationStyle", "TentaclePassive");
            _ps.Invoke();

            Assert.AreEqual(1, _machines.Count);
            Assert.AreEqual(new ReferenceCollection("environments-1").ToString(), _machines[0].EnvironmentIds.ToString());
            Assert.AreEqual("Tentacle_Name", _machines[0].Name);
            Assert.AreEqual("ThisIsMyThumbprint", _machines[0].Thumbprint);
            Assert.AreEqual(new ReferenceCollection() { "Role1", "Role2" }.ToString(), _machines[0].Roles.ToString());
            Assert.AreEqual("https://server.domain:port/", _machines[0].Uri);
            Assert.AreEqual(CommunicationStyle.TentaclePassive, _machines[0].CommunicationStyle);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
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
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand("Add-OctoMachine");
            _ps.Invoke();
        }
    }
}
