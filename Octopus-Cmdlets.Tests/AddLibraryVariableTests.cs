using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddLibraryVariableTests
    {
        private const string CmdletName = "Add-OctoLibraryVariable";
        private PowerShell _ps;
        private readonly VariableSetResource _variableSet = new VariableSetResource();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddLibraryVariable));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _variableSet.Variables.Clear();
            
            var lib = new LibraryVariableSetResource { Name = "Octopus" };
            var libs = new List<LibraryVariableSetResource> {lib};
            lib.Links.Add("Variables", "variablesets-1");
            octoRepo.Setup(o => o.LibraryVariableSets.FindOne(It.IsAny<Func<LibraryVariableSetResource, bool>>()))
                .Returns(
                    (Func<LibraryVariableSetResource, bool> f) =>
                        (from l in libs where f(l) select l).FirstOrDefault());

            octoRepo.Setup(o => o.Projects.FindByName("Gibberish")).Returns((ProjectResource)null);

            octoRepo.Setup(o => o.VariableSets.Get("variablesets-1")).Returns(_variableSet);

            var process = new DeploymentProcessResource();
            process.Steps.Add(new DeploymentStepResource { Name = "Website", Id = "Step-1" });
            octoRepo.Setup(o => o.DeploymentProcesses.Get("deploymentprocesses-1")).Returns(process);

            var envs = new List<EnvironmentResource>
            {
                new EnvironmentResource {Id = "Environments-1", Name = "DEV"}
            };

            octoRepo.Setup(o => o.Environments.FindByNames(new[] { "DEV" })).Returns(envs);
            var machines = new List<MachineResource>
            {
                new MachineResource {Id = "Machines-1", Name = "web-01"}
            };
            octoRepo.Setup(o => o.Machines.FindByNames(new[] { "web-01" })).Returns(machines);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSet", "Octopus").AddParameter("Name", "Test");
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_VariableSet()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSet", "Gibberish").AddParameter("Name", "Test");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_All()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("VariableSet", "Octopus")
                .AddParameter("Name", "Test")
                .AddParameter("Value", "Test Value")
                .AddParameter("Environments", new[] { "DEV" })
                .AddParameter("Roles", new[] { "Web" })
                .AddParameter("Machines", new[] { "web-01" })
                .AddParameter("Sensitive", false);
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
            Assert.AreEqual("Test Value", _variableSet.Variables[0].Value);
        }

        [TestMethod]
        public void With_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("VariableSet", "Octopus")
                .AddParameter("InputObject", new VariableResource { Name = "Test" });
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
        }
    }
}
