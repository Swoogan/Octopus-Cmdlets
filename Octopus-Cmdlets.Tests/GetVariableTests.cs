using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using Octopus.Client.Extensibility;

namespace Octopus_Cmdlets.Tests
{
    public class GetVariableTests
    {
        private const string CmdletName = "Get-OctoVariable";
        private PowerShell _ps;

        public GetVariableTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetVariable));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create a Project
            var projectRepo = new Mock<IProjectRepository>();
            const string pVsId = "/api/variables/variableset-projects-1";
            var projectResource = new ProjectResource
            {
                Name = "Octopus",
                Links = new LinkCollection { { "Variables", new Href(pVsId) } }
            };

            projectRepo.Setup(p => p.FindByName("Octopus", It.IsAny<string>(), It.IsAny<object>())).Returns(projectResource);
            octoRepo.Setup(o => o.Projects).Returns(projectRepo.Object);

            // Create a library variable set
            const string vsId = "/api/variables/variableset-LibraryVariableSets-1";
            var libraryResources = new List<LibraryVariableSetResource>
            {
                new LibraryVariableSetResource
                {
                    Id = "LibraryVariableSets-1",
                    Name = "Octopus",
                    Links = new LinkCollection {{"Variables", new Href("/api/variables/variableset-LibraryVariableSets-1")}}
                },
                new LibraryVariableSetResource
                {
                    Id = "LibraryVariableSets-2",
                    Name = "Deploy",
                }
            };

            // Allow the FindOne predicate to operate on the collection
            octoRepo.Setup(o => o.LibraryVariableSets.FindOne(It.IsAny<Func<LibraryVariableSetResource, bool>>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(
                    (Func<LibraryVariableSetResource, bool> f, string path, string pathParams) =>
                        (from l in libraryResources where f(l) select l).FirstOrDefault());

            // Create a variableset
            var variableRepo = new Mock<IVariableSetRepository>();
            var vsResource = new VariableSetResource
            {
                Variables = new List<VariableResource>
                {
                    new VariableResource {Name = "Octopus"},
                    new VariableResource {Name = "Deploy", Value = "To Production"},
                    new VariableResource {Name = "Automation"},
                }
            };
            variableRepo.Setup(v => v.Get(pVsId)).Returns(vsResource);
            variableRepo.Setup(v => v.Get(vsId)).Returns(vsResource);

            octoRepo.Setup(o => o.VariableSets).Returns(variableRepo.Object);
        }

        [Fact]
        public void By_Project_No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var variables = _ps.Invoke<VariableResource>();

            Assert.Equal(3, variables.Count);
        }

        [Fact]
        public void By_Project_With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus").AddArgument("Deploy");
            var variables = _ps.Invoke<VariableResource>();

            Assert.Single(variables);
            Assert.Equal("To Production", variables[0].Value);
        }

        [Fact]
        public void With_Invalid_Project()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Project", "Gibberish").AddArgument("Deploy");
            Assert.Throws<CmdletInvocationException>(() => _ps.Invoke());
        }

        [Fact]
        public void By_VariableSet_No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSet", "Octopus");
            var variables = _ps.Invoke<VariableResource>();

            Assert.Equal(3, variables.Count);
        }

        [Fact]
        public void With_Invalid_VariableSet()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSet", "Gibberish").AddArgument("Deploy");
            _ps.Invoke();

            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("Library variable set 'Gibberish' was not found.", _ps.Streams.Warning[0].ToString());
        }
        
        [Fact]
        public void By_VariableSet_With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSet", "Octopus").AddArgument("Deploy"); 
            var variables = _ps.Invoke<VariableResource>();

            Assert.Single(variables);
            Assert.Equal("To Production", variables[0].Value);
        }

        [Fact]
        public void By_VariableSetId_No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSetId", "LibraryVariableSets-1");
            var variables = _ps.Invoke<VariableResource>();

            Assert.Equal(3, variables.Count);
        }

        [Fact]
        public void By_VariableSetId_With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSetId", "LibraryVariableSets-1").AddArgument("Deploy");
            var variables = _ps.Invoke<VariableResource>();

            Assert.Single(variables);
            Assert.Equal("To Production", variables[0].Value);
        }

        [Fact]
        public void With_Invalid_VariableSetId()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("VariableSetId", "Gibberish").AddArgument("Deploy");
            _ps.Invoke();

            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("Library variable set with id 'Gibberish' was not found.", _ps.Streams.Warning[0].ToString());
        }
    }
}
