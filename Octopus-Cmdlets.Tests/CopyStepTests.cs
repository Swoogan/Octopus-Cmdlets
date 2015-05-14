using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class CopyStepTests
    {
        private const string CmdletName = "Copy-OctoStep";
        private PowerShell _ps;
        private DeploymentProcessResource _process;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(CopyStep));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create project
            var project = new ProjectResource
            {
                Name = "Octopus",
                Description = "Test Source",
                DeploymentProcessId = "deploymentprocesses-1",
                VariableSetId = "variablesets-1",
            };

            // Create projects
            octoRepo.Setup(o => o.Projects.FindByName("Octopus")).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish")).Returns((ProjectResource)null);

            // Create deployment process
            var action = new DeploymentActionResource { Name = "NuGet", ActionType = "NuGet" };
            action.Environments.Add("environments-1");
            action.Properties.Add("Something", "Value");
            action.SensitiveProperties.Add("SomethingElse", "Secret");

            var step = new DeploymentStepResource { Id = "deploymentsteps-1", Name = "Website" };
            step.Actions.Add(action);

            _process = new DeploymentProcessResource();
            _process.Steps.Add(step);

            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { "deploymentprocesses-1" }))).Returns(_process);
            octoRepo.Setup(o => o.DeploymentProcesses.Get(It.IsIn(new[] { "deploymentprocesses-2" }))).Returns(new DeploymentProcessResource());

            // Create variable set
            var variable = new VariableResource { Name = "Name", Value = "Value" };
            variable.Scope.Add(ScopeField.Action, "DeploymentsActions-1");
            variable.Scope.Add(ScopeField.Environment, "Environments-1");

            var variableSet = new VariableSetResource();
            variableSet.Variables.Add(variable);

            octoRepo.Setup(o => o.VariableSets.Get(It.IsIn(new[] { "variablesets-1" }))).Returns(variableSet);
            octoRepo.Setup(o => o.VariableSets.Get(It.IsIn(new[] { "variablesets-2" }))).Returns(new VariableSetResource());
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void Just_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void Just_Name()
        {
            // Execute cmdlet
           _ps.AddCommand(CmdletName).AddParameter("Name", "Website");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Project_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Website");
            _ps.Invoke();

            Assert.AreEqual(2, _process.Steps.Count);
            Assert.AreEqual("Website - Copy", _process.Steps[1].Name);
        }
        
        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Name", "Website");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(CmdletInvocationException))]
        public void With_Project_And_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Destination()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Website")
                .AddParameter("Destination", "Webservice");
            _ps.Invoke();

            Assert.AreEqual(2, _process.Steps.Count);
            
            var step = _process.Steps[1];
            Assert.AreEqual("Webservice", step.Name);
            Assert.AreNotEqual("deploymentsteps-1", step.Id);

            var action = new DeploymentActionResource { Name = "Webservice" };
            action.Environments.Add("environments-1");

            var actionResource = step.Actions[0];
            Assert.AreEqual(action.Name, actionResource.Name);
            Assert.AreEqual("NuGet", actionResource.ActionType);

            Assert.AreEqual(action.Environments.ToString(), actionResource.Environments.ToString());
            Assert.IsTrue(actionResource.Properties.ContainsKey("Something"));
            Assert.AreEqual("Value", actionResource.Properties["Something"]);
            Assert.AreEqual("Secret", actionResource.SensitiveProperties["SomethingElse"]);
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("Website")
                .AddArgument("Webservice");
            _ps.Invoke();

            Assert.AreEqual(2, _process.Steps.Count);

            var step = _process.Steps[1];
            Assert.AreEqual("Webservice", step.Name);
            Assert.AreNotEqual("deploymentsteps-1", step.Id);

            var action = new DeploymentActionResource { Name = "Webservice" };
            action.Environments.Add("environments-1");

            var actionResource = step.Actions[0];
            Assert.AreEqual(action.Name, actionResource.Name);
            Assert.AreEqual("NuGet", actionResource.ActionType);

            Assert.AreEqual(action.Environments.ToString(), actionResource.Environments.ToString());
            Assert.IsTrue(actionResource.Properties.ContainsKey("Something"));
            Assert.AreEqual("Value", actionResource.Properties["Something"]);
            Assert.AreEqual("Secret", actionResource.SensitiveProperties["SomethingElse"]);
        }
    }
}
