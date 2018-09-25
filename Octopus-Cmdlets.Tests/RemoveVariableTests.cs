using System.Management.Automation;
using Xunit;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class RemoveVariableTests
    {
        private const string CmdletName = "Remove-OctoVariableSet";
        private PowerShell _ps;

        private readonly VariableSetResource _variableSet = new VariableSetResource
        {
            Id = "VariableSets-1",
        };

        private VariableResource _variable;

        public RemoveVariableTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (RemoveVariable));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _variableSet.Variables.Clear();

            var project = new ProjectResource();
            project.Links.Add("Variables", "variablesets-1");
            octoRepo.Setup(o => o.Projects.FindByName("Octopus", null, null)).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", null, null)).Returns((ProjectResource) null);

            _variable = new VariableResource {Name = "Azure"};
            _variableSet.Variables.Add(_variable);
            _variableSet.Variables.Add(new VariableResource {Name = "ConnectionString"});
            _variableSet.Variables.Add(new VariableResource {Name = "ServerName"});
            octoRepo.Setup(o => o.VariableSets.Get("variablesets-1")).Returns(_variableSet);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("InputObject", _variable);
            _ps.Invoke();

            Assert.Equal(2, _variableSet.Variables.Count);
            Assert.False(_variableSet.Variables.Contains(_variable));
        }

        [Fact]
        public void With_Invalid_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("InputObject", new VariableResource());
            _ps.Invoke();

            Assert.Equal(3, _variableSet.Variables.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("Variable '' in project 'Octopus' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        //[Fact]
        //public void With_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("Id", "LibraryVariableSets-2");
        //    _ps.Invoke();

        //    Assert.Equal(2, _variableSet.Variables.Count);
        //    Assert.IsFalse(_variableSet.Variables.Contains(_set));
        //}

        //[Fact]
        //public void With_Invalid_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
        //    _ps.Invoke();

        //    Assert.Equal(3, _variableSet.Variables.Count);
        //    Assert.Equal(1, _ps.Streams.Warning.Count);
        //    Assert.Equal("The library variable set with id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        //}

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Azure");
            _ps.Invoke();

            Assert.Equal(2, _variableSet.Variables.Count);
            Assert.False(_variableSet.Variables.Contains(_variable));
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Gibberish");
            _ps.Invoke();

            Assert.Equal(3, _variableSet.Variables.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("Variable 'Gibberish' in project 'Octopus' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Azure");
            _ps.Invoke();

            Assert.Equal(2, _variableSet.Variables.Count);
            Assert.False(_variableSet.Variables.Contains(_variable));
        }

        [Fact]
        public void With_Name_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Gibberish")
                .AddParameter("Name", "Gibberish")
                .AddParameter("InputObject", new VariableResource());
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
