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
    public class UpdateVariableTests
    {
        private const string CmdletName = "Update-OctoVariable";
        private PowerShell _ps;
        private readonly VariableSetResource _variableSet = new VariableSetResource();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(UpdateVariable));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            var variable = new VariableResource
            {
                Id = "variables-1",
                Name = "Test",
                Value = "Test Value",
                IsSensitive = false
            };
            variable.Scope.Add(ScopeField.Action, "actions-1");
            variable.Scope.Add(ScopeField.Environment, "environments-1");
            variable.Scope.Add(ScopeField.Role, "DB");

            _variableSet.Variables.Add(variable);

            var project = new ProjectResource { DeploymentProcessId = "deploymentprocesses-1" };
            project.Links.Add("Variables", "variablesets-1");
            octoRepo.Setup(o => o.Projects.FindByName("Octopus")).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish")).Returns((ProjectResource)null);

            octoRepo.Setup(o => o.VariableSets.Get("variablesets-1")).Returns(_variableSet);

            var process = new DeploymentProcessResource();
            process.Steps.Add(new DeploymentStepResource { Name = "Website", Id = "Step-1" });
            octoRepo.Setup(o => o.DeploymentProcesses.Get("deploymentprocesses-1")).Returns(process);

            var envs = new List<EnvironmentResource>
            {
                new EnvironmentResource {Id = "environments-1", Name = "DEV"},
                new EnvironmentResource {Id = "environments-2", Name = "TEST"}
            };

            octoRepo.Setup(o => o.Environments.FindByNames(It.IsAny<string[]>()))
                .Returns((string[] names) => (from n in names
                    from e in envs
                    where e.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                    select e).ToList());

            var machines = new List<MachineResource>
            {
                new MachineResource {Id = "machines-1", Name = "db-01"},
                new MachineResource {Id = "machines-2", Name = "web-01"}
            };
            octoRepo.Setup(o => o.Machines.FindByNames(It.IsAny<string[]>())).Returns(
                (string[] names) => (from n in names
                                     from m in machines
                                     where m.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                                     select m).ToList());
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Test");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "variables-1").AddParameter("Name", "Test");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Id", "variables-1").AddParameter("Name", "NewName");
            _ps.Invoke();

            Assert.AreEqual("NewName", _variableSet.Variables[0].Name);
        }

        [TestMethod]
        public void With_Sensitive()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Id", "variables-1").AddParameter("Sensitive", true);
            _ps.Invoke();

            Assert.AreEqual(true, _variableSet.Variables[0].IsSensitive);
        }

        [TestMethod]
        public void With_Environments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Id", "variables-1").AddParameter("Environment", "TEST");
            _ps.Invoke();

            Assert.AreEqual("environments-2", _variableSet.Variables[0].Scope[ScopeField.Environment].First());
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Id", "variables-1");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Id", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_All()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Id", "variables-1")
                .AddParameter("Name", "NewName")
                .AddParameter("Value", "New Test Value")
                .AddParameter("Environments", new[] { "TEST" })
                .AddParameter("Roles", new[] { "Web" })
                .AddParameter("Machines", new[] { "web-01" })
                .AddParameter("Sensitive", true);
            _ps.Invoke();

            Assert.AreEqual(1, _variableSet.Variables.Count);
            Assert.AreEqual("NewName", _variableSet.Variables[0].Name);
            Assert.AreEqual("New Test Value", _variableSet.Variables[0].Value);
            Assert.AreEqual(true, _variableSet.Variables[0].IsSensitive);
            Assert.AreEqual("environments-2", _variableSet.Variables[0].Scope[ScopeField.Environment].First());
            Assert.AreEqual("Web", _variableSet.Variables[0].Scope[ScopeField.Role].First());
            Assert.AreEqual("machines-2", _variableSet.Variables[0].Scope[ScopeField.Machine].First());
        }

        //[TestMethod]
        //public void With_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Project", "Octopus")
        //        .AddParameter("InputObject", new VariableResource { Name = "Test" });
        //    _ps.Invoke();

        //    Assert.AreEqual(1, _variableSet.Variables.Count);
        //    Assert.AreEqual("Test", _variableSet.Variables[0].Name);
        //}
    }
}
