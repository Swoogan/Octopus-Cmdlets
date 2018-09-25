using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    public class UseVariableSetTests
    {
        private const string CmdletName = "Use-OctoVariableSet";
        private PowerShell _ps;
        private ProjectResource _projectResource;

        public UseVariableSetTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(UseVariableSet));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a project
            _projectResource = new ProjectResource {Name = "Octopus"};
            var projectRepo = new Mock<IProjectRepository>();
            projectRepo.Setup(p => p.FindByName("Octopus", null, null)).Returns(_projectResource);
            projectRepo.Setup(p => p.Modify(It.IsAny<ProjectResource>())).Returns((ProjectResource p) => p);

            octoRepo.Setup(o => o.Projects).Returns(projectRepo.Object);

            // Create some library variable sets
            var libraryResources = new List<LibraryVariableSetResource>
            {
                new LibraryVariableSetResource {Id = "LibraryVariableSets-1", Name = "ConnectionStrings"},
                new LibraryVariableSetResource {Id = "LibraryVariableSets-2", Name = "Azure"},
            };

            octoRepo.Setup(o => o.LibraryVariableSets.FindAll(null, null)).Returns(libraryResources);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish").AddArgument("ConnectionStrings");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_Parameter()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddParameter("Name", "ConnectionStrings");
            _ps.Invoke();

            Assert.Equal("LibraryVariableSets-1", _projectResource.IncludedLibraryVariableSetIds[0]);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Gibberish");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }
    }
}
