using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class RemoveProjectTests
    {
        private const string CmdletName = "Remove-OctoProject";
        private PowerShell _ps;
        private readonly List<ProjectResource> _projects = new List<ProjectResource>();

        private readonly ProjectResource _project = new ProjectResource
        {
            Id = "Projects-2",
            Name = "Deploy"
        };

        public RemoveProjectTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(RemoveProject));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _projects.Clear();
            _projects.Add(new ProjectResource { Id = "Projects-1", Name = "Octopus" });
            _projects.Add(_project);
            _projects.Add(new ProjectResource { Id = "Projects-3", Name = "Automation" });

            octoRepo.Setup(o => o.Projects.Delete(It.IsAny<ProjectResource>())).Callback(
                delegate (ProjectResource set)
                {
                    if (_projects.Contains(set))
                        _projects.Remove(set);
                    else
                        throw new KeyNotFoundException("The given key was not present in the dictionary.");
                }
                );

            octoRepo.Setup(o => o.Projects.Get("Projects-2")).Returns(_project);
            octoRepo.Setup(o => o.Projects.Get(It.IsNotIn(new[] { "Projects-2" })))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            octoRepo.Setup(o => o.Projects.FindByName("Test", It.IsAny<string>(), It.IsAny<object>())).Returns(_project);
            octoRepo.Setup(o => o.Projects.FindByName("Gibberish", It.IsAny<string>(), It.IsAny<object>())).Returns((ProjectResource)null);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        //[Fact]
        //public void With_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("InputObject", _env);
        //    _ps.Invoke();

        //    Assert.Equal(2, _projects.Count);
        //    Assert.IsFalse(_projects.Contains(_env));
        //}

        //[Fact]
        //public void With_Invalid_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("InputObject", new ProjectResource());
        //    _ps.Invoke();

        //    Assert.Equal(3, _projects.Count);
        //    Assert.Equal(1, _ps.Streams.Warning.Count);
        //    Assert.Equal("The library variable set '' does not exist.", _ps.Streams.Warning[0].ToString());
        //}

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "Projects-2" });
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);
            Assert.False(_projects.Contains(_project));
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.Equal(3, _projects.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("A project with the id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Test" });
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);
            Assert.False(_projects.Contains(_project));
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.Equal(3, _projects.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("The project 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Valid_And_Invalid_Names()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Test", "Gibberish" });
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);
            Assert.False(_projects.Contains(_project));
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument(new[] { "Test" });
            _ps.Invoke();

            Assert.Equal(2, _projects.Count);
            Assert.False(_projects.Contains(_project));
        }

        [Fact]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish").AddParameter("Id", "Gibberish");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        //[Fact, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Name_And_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Name", "Gibberish")
        //        .AddParameter("InputObject", new ProjectResource());
        //    _ps.Invoke();
        //}

        //[Fact, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Object_And_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new ProjectResource());
        //    _ps.Invoke();
        //}

        //[Fact, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Name_Id_And_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Name", "Gibberish")
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new ProjectResource());
        //    _ps.Invoke();
        //}
    }
}
