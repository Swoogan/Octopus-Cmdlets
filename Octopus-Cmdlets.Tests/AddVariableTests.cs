using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

            var project = new ProjectResource();
            project.Links.Add("Variables", "variablesets-1");
            octoRepo.Setup(o => o.Projects.FindByName("Octopus")).Returns(project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish")).Returns((ProjectResource) null);

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
    }
}
