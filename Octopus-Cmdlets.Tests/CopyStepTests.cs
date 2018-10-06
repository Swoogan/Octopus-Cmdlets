using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class CopyStepTests
    {
        private const string CmdletName = "Copy-OctoStep";
        private PowerShell _ps;
        private DeploymentProcessResource _process;

        public CopyStepTests()
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
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", It.IsAny<string>(), It.IsAny<object>())).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", It.IsAny<string>(), It.IsAny<object>())).Returns((ProjectResource)null);

            // Create deployment process
            var action = new DeploymentActionResource { Name = "NuGet", ActionType = "NuGet" };
            action.Environments.Add("environments-1");
            action.Properties.Add("Something", "Value");
            action.Properties.Add("SomethingElse", new PropertyValueResource("Secret", true));

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

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Just_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void Just_Name()
        {
            // Execute cmdlet
           _ps.AddCommand(CmdletName).AddParameter("Name", "Website");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Website");
            _ps.Invoke();

            Assert.Equal(2, _process.Steps.Count);
            Assert.Equal("Website - Copy", _process.Steps[1].Name);
        }
        
        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Name", "Website");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project_And_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Gibberish");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());

        }

        [Fact]
        public void With_Destination()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("Name", "Website")
                .AddParameter("Destination", "Webservice");
            _ps.Invoke();

            Assert.Equal(2, _process.Steps.Count);
            
            var step = _process.Steps[1];
            Assert.Equal("Webservice", step.Name);
            Assert.NotEqual("deploymentsteps-1", step.Id);

            var action = new DeploymentActionResource { Name = "Webservice" };
            action.Environments.Add("environments-1");

            var actionResource = step.Actions[0];
            Assert.Equal(action.Name, actionResource.Name);
            Assert.Equal("NuGet", actionResource.ActionType);

            Assert.Equal(action.Environments.ToString(), actionResource.Environments.ToString());
            Assert.True(actionResource.Properties.ContainsKey("Something"));
            Assert.Equal("Value", actionResource.Properties["Something"].Value);
            Assert.True(actionResource.Properties["SomethingElse"].IsSensitive);
            Assert.Equal("Secret", actionResource.Properties["SomethingElse"].SensitiveValue.NewValue);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("Website")
                .AddArgument("Webservice");
            _ps.Invoke();

            Assert.Equal(2, _process.Steps.Count);

            var step = _process.Steps[1];
            Assert.Equal("Webservice", step.Name);
            Assert.NotEqual("deploymentsteps-1", step.Id);

            var action = new DeploymentActionResource { Name = "Webservice" };
            action.Environments.Add("environments-1");

            var actionResource = step.Actions[0];
            Assert.Equal(action.Name, actionResource.Name);
            Assert.Equal("NuGet", actionResource.ActionType);

            Assert.Equal(action.Environments.ToString(), actionResource.Environments.ToString());
            Assert.True(actionResource.Properties.ContainsKey("Something"));
            Assert.Equal("Value", actionResource.Properties["Something"].Value);
            Assert.True(actionResource.Properties["SomethingElse"].IsSensitive);
            Assert.Equal("Secret", actionResource.Properties["SomethingElse"].SensitiveValue.NewValue);
        }
    }
}
