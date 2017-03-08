using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class RemoveVariableTests
    {
        private const string CmdletName = "Remove-OctoVariableSet";
        private PowerShell _ps;

        private readonly VariableSetResource _variableSet = new VariableSetResource
        {
            Id = "VariableSets-1",
        };

        private VariableResource _variable;

        [TestInitialize]
        public void Init()
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

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("InputObject", _variable);
            _ps.Invoke();

            Assert.AreEqual(2, _variableSet.Variables.Count);
            Assert.IsFalse(_variableSet.Variables.Contains(_variable));
        }

        [TestMethod]
        public void With_Invalid_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("InputObject", new VariableResource());
            _ps.Invoke();

            Assert.AreEqual(3, _variableSet.Variables.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("Variable '' in project 'Octopus' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        //[TestMethod]
        //public void With_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("Id", "LibraryVariableSets-2");
        //    _ps.Invoke();

        //    Assert.AreEqual(2, _variableSet.Variables.Count);
        //    Assert.IsFalse(_variableSet.Variables.Contains(_set));
        //}

        //[TestMethod]
        //public void With_Invalid_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
        //    _ps.Invoke();

        //    Assert.AreEqual(3, _variableSet.Variables.Count);
        //    Assert.AreEqual(1, _ps.Streams.Warning.Count);
        //    Assert.AreEqual("The library variable set with id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        //}

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Azure");
            _ps.Invoke();

            Assert.AreEqual(2, _variableSet.Variables.Count);
            Assert.IsFalse(_variableSet.Variables.Contains(_variable));
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Octopus").AddParameter("Name", "Gibberish");
            _ps.Invoke();

            Assert.AreEqual(3, _variableSet.Variables.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("Variable 'Gibberish' in project 'Octopus' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Azure");
            _ps.Invoke();

            Assert.AreEqual(2, _variableSet.Variables.Count);
            Assert.IsFalse(_variableSet.Variables.Contains(_variable));
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Name_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Project", "Gibberish")
                .AddParameter("Name", "Gibberish")
                .AddParameter("InputObject", new VariableResource());
            _ps.Invoke();
        }
    }
}
