using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class AddVariableSetTests
    {
        private const string CmdletName = "Add-OctoVariableSet";
        private PowerShell _ps;
        private readonly List<LibraryVariableSetResource> _sets = new List<LibraryVariableSetResource>();

        public AddVariableSetTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(AddVariableSet));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _sets.Clear();

            var repo = new Mock<ILibraryVariableSetRepository>();
            repo.Setup(e => e.Create(It.IsAny<LibraryVariableSetResource>(), null))
                .Returns(delegate(LibraryVariableSetResource e)
                {
                    _sets.Add(e);
                    return e;
                });

            octoRepo.Setup(o => o.LibraryVariableSets).Returns(repo.Object);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Octopus");
            _ps.Invoke();

            Assert.Equal(1, _sets.Count);
            Assert.Equal("Octopus", _sets[0].Name);
        }

        [Fact]
        public void With_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Description", "VaribleSet");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_And_Description()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Octopus")
                .AddParameter("Description", "VariableSet");
            _ps.Invoke();

            Assert.Equal(1, _sets.Count);
            Assert.Equal("Octopus", _sets[0].Name);
            Assert.Equal("VariableSet", _sets[0].Description);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus")
                .AddArgument("VariableSet");
            _ps.Invoke();

            Assert.Equal(1, _sets.Count);
            Assert.Equal("Octopus", _sets[0].Name);
            Assert.Equal("VariableSet", _sets[0].Description);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
