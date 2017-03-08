using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddVariableTests
    {
        private const string CmdletName = "Add-OctoVariable";
        private PowerShell _ps;
        private readonly VariableSetResource _variableSet = new VariableSetResource();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddVariable));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _variableSet.Variables.Clear();

            var project = new ProjectResource {DeploymentProcessId = "deploymentprocesses-1"};
            project.Links.Add("Variables", "variablesets-1");
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", null, null)).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", null, null)).Returns((ProjectResource) null);

            octoRepo.Setup(o => o.VariableSets.Get("variablesets-1")).Returns(_variableSet);

            var process = new DeploymentProcessResource();
            process.Steps.Add(new DeploymentStepResource {Name = "Website", Id = "Step-1"});
            octoRepo.Setup(o => o.DeploymentProcesses.Get("deploymentprocesses-1")).Returns(process);

            var envs = new List<EnvironmentResource>
            {
                new EnvironmentResource {Id = "Environments-1", Name = "DEV"}
            };

            octoRepo.Setup(o => o.Environments.FindByNames(new[] {"DEV"}, null, null)).Returns(envs);
            var machines = new List<MachineResource>
            {
                new MachineResource {Id = "Machines-1", Name = "web-01"}
            };
            octoRepo.Setup(o => o.Machines.FindByNames(new[] { "web-01" }, null, null)).Returns(machines);
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
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Test");
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Name", "Test");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_All()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Test")
                .AddParameter("Value", "Test Value")
                .AddParameter("Environments", new[] {"DEV"})
                .AddParameter("Roles", new[] {"Web"})
                .AddParameter("Machines", new[] {"web-01"})
                .AddParameter("Steps", new[] {"Website"})
                .AddParameter("Sensitive", false);
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
            Assert.AreEqual("Test Value", _variableSet.Variables[0].Value);

            var scopeValue = _variableSet.Variables[0].Scope[ScopeField.Action];
            Assert.AreEqual("Step-1", scopeValue.ToString());
        }

        [TestMethod]
        public void With_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("InputObject", new VariableResource {Name = "Test"});
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("Test", _variableSet.Variables[0].Name);
        }
    }
}
