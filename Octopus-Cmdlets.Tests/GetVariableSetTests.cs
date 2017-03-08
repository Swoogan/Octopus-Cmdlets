using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class GetVariableSetTests
    {
        private const string CmdletName = "Get-OctoVariableSet";
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetVariableSet));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a library variable set
            const string vsId = "/api/variables/variableset-LibraryVariableSets-1";
            var libraryResources = new List<LibraryVariableSetResource>
            {
                new LibraryVariableSetResource {Id = "LibraryVariableSets-1", Name = "Octopus"},
                new LibraryVariableSetResource {Id = "LibraryVariableSets-2", Name = "Deploy"},
                new LibraryVariableSetResource {Id = "LibraryVariableSets-3", Name = "Automation"}
            };

            octoRepo.Setup(o => o.LibraryVariableSets.FindAll(null, null)).Returns(libraryResources);

            // Create a variableset
            var variableRepo = new Mock<IVariableSetRepository>();
            var vsResource = new VariableSetResource
            {
                Variables = new List<VariableResource>
                {
                    new VariableResource {Name = "Octopus"},
                    new VariableResource {Name = "Deploy"},
                    new VariableResource {Name = "Automation"},
                }
            };
            variableRepo.Setup(v => v.Get(vsId)).Returns(vsResource);

            octoRepo.Setup(o => o.VariableSets).Returns(variableRepo.Object);
        }

        [TestMethod]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(3, variables.Count);
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(1, variables.Count);
            Assert.AreEqual("Octopus", variables[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(0, variables.Count);
        }

        [TestMethod]
        public void By_Name_With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus");
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(1, variables.Count);
            Assert.AreEqual("Octopus", variables[0].Name);
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "LibraryVariableSets-1");
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(1, variables.Count);
            Assert.AreEqual("Octopus", variables[0].Name);
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var variables = _ps.Invoke<LibraryVariableSetResource>();

            Assert.AreEqual(0, variables.Count);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Id_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus").AddParameter("Id", "Id");
            _ps.Invoke();
        }
    }
}
