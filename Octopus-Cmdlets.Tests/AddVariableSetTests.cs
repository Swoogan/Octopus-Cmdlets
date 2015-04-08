using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class AddVariableSetTests
    {
        private const string CmdletName = "Add-OctoVariableSet";
        private PowerShell _ps;
        private readonly List<LibraryVariableSetResource> _sets = new List<LibraryVariableSetResource>();

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddVariableSet));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _sets.Clear();

            var repo = new Mock<ILibraryVariableSetRepository>();
            repo.Setup(e => e.Create(It.IsAny<LibraryVariableSetResource>()))
                .Returns(delegate(LibraryVariableSetResource e)
                {
                    _sets.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.LibraryVariableSets).Returns(repo.Object);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus");
            _ps.Invoke();

            Assert.AreEqual(1, _sets.Count);
            Assert.AreEqual("Octopus", _sets[0].Name);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Description", "VaribleSet");
            _ps.Invoke();
        }

        [TestMethod]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Octopus")
                .AddParameter("Description", "VariableSet");
            _ps.Invoke();

            Assert.AreEqual(1, _sets.Count);
            Assert.AreEqual("Octopus", _sets[0].Name);
            Assert.AreEqual("VariableSet", _sets[0].Description);
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("VariableSet");
            _ps.Invoke();

            Assert.AreEqual(1, _sets.Count);
            Assert.AreEqual("Octopus", _sets[0].Name);
            Assert.AreEqual("VariableSet", _sets[0].Description);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }
    }
}
