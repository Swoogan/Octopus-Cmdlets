using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class RemoveProjectGroupTests
    {
        private const string CmdletName = "Remove-OctoProjectGroup";
        private PowerShell _ps;
        private readonly List<ProjectGroupResource> _groups = new List<ProjectGroupResource>();

        private readonly ProjectGroupResource _group = new ProjectGroupResource
        {
            Id = "ProjectGroups-2",
            Name = "Deploy"
        };

        public RemoveProjectGroupTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(RemoveProjectGroup));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _groups.Clear();
            _groups.Add(new ProjectGroupResource { Id = "ProjectGroups-1", Name = "Octopus" });
            _groups.Add(_group);
            _groups.Add(new ProjectGroupResource { Id = "ProjectGroups-3", Name = "Automation" });

            octoRepo.Setup(o => o.ProjectGroups.Delete(It.IsAny<ProjectGroupResource>())).Callback(
                delegate (ProjectGroupResource set)
                {
                    if (_groups.Contains(set))
                        _groups.Remove(set);
                    else
                        throw new KeyNotFoundException("The given key was not present in the dictionary.");
                }
                );

            octoRepo.Setup(o => o.ProjectGroups.Get("ProjectGroups-2")).Returns(_group);
            octoRepo.Setup(o => o.ProjectGroups.Get(It.IsNotIn(new[] { "ProjectGroups-2" })))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            octoRepo.Setup(o => o.ProjectGroups.FindByName("Test", It.IsAny<string>(), It.IsAny<object>())).Returns(_group);
            octoRepo.Setup(o => o.ProjectGroups.FindByName("Gibberish", It.IsAny<string>(), It.IsAny<object>())).Returns((ProjectGroupResource)null);
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

        //    Assert.Equal(2, _groups.Count);
        //    Assert.IsFalse(_groups.Contains(_env));
        //}

        //[Fact]
        //public void With_Invalid_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("InputObject", new ProjectGroupResource());
        //    _ps.Invoke();

        //    Assert.Equal(3, _groups.Count);
        //    Assert.Equal(1, _ps.Streams.Warning.Count);
        //    Assert.Equal("The library variable set '' does not exist.", _ps.Streams.Warning[0].ToString());
        //}

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "ProjectGroups-2" });
            _ps.Invoke();

            Assert.Equal(2, _groups.Count);
            Assert.False(_groups.Contains(_group));
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.Equal(3, _groups.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("A project group with the id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Test" });
            _ps.Invoke();

            Assert.Equal(2, _groups.Count);
            Assert.False(_groups.Contains(_group));
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.Equal(3, _groups.Count);
            Assert.Equal(1, _ps.Streams.Warning.Count);
            Assert.Equal("The project group 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Valid_And_Invalid_Names()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Test", "Gibberish" });
            _ps.Invoke();

            Assert.Equal(2, _groups.Count);
            Assert.False(_groups.Contains(_group));
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument(new[] { "Test" });
            _ps.Invoke();

            Assert.Equal(2, _groups.Count);
            Assert.False(_groups.Contains(_group));
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
        //        .AddParameter("InputObject", new ProjectGroupResource());
        //    _ps.Invoke();
        //}

        //[Fact, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Object_And_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new ProjectGroupResource());
        //    _ps.Invoke();
        //}

        //[Fact, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Name_Id_And_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Name", "Gibberish")
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new ProjectGroupResource());
        //    _ps.Invoke();
        //}
    }
}
