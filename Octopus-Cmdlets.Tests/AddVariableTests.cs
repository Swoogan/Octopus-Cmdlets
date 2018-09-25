using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class AddVariableTests
    {
        private const string CmdletName = "Add-OctoVariable";
        private PowerShell _ps;
        private readonly VariableSetResource _variableSet = new VariableSetResource();

        public AddVariableTests()
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

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
        
        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Test");
            _ps.Invoke();

            Assert.Equal(1, _variableSet.Variables.Count);
            Assert.Equal("Test", _variableSet.Variables[0].Name);
        }

        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddParameter("Name", "Test");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
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

            Assert.Equal(1, _variableSet.Variables.Count);
            Assert.Equal("Test", _variableSet.Variables[0].Name);
            Assert.Equal("Test Value", _variableSet.Variables[0].Value);

            var scopeValue = _variableSet.Variables[0].Scope[ScopeField.Action];
            Assert.Equal("Step-1", scopeValue.ToString());
        }

        [Fact]
        public void With_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Octopus")
                .AddParameter("InputObject", new VariableResource {Name = "Test"});
            _ps.Invoke();

            Assert.Equal(1, _variableSet.Variables.Count);
            Assert.Equal("Test", _variableSet.Variables[0].Name);
        }
    }
}
