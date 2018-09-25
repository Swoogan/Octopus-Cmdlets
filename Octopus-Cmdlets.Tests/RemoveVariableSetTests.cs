using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class RemoveVariableSetTests
    {
        private const string CmdletName = "Remove-OctoVariableSet";
        private PowerShell _ps;
        private readonly List<LibraryVariableSetResource> _sets = new List<LibraryVariableSetResource>();

        private readonly LibraryVariableSetResource _set = new LibraryVariableSetResource
        {
            Id = "LibraryVariableSets-2",
            Name = "Azure"
        };

        public RemoveVariableSetTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (RemoveVariableSet));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _sets.Clear();
            _sets.Add(new LibraryVariableSetResource {Id = "LibraryVariableSets-1", Name = "ConnectionStrings"});
            _sets.Add(_set);
            _sets.Add(new LibraryVariableSetResource {Id = "LibraryVariableSets-3", Name = "Service Endpoints"});

            octoRepo.Setup(o => o.LibraryVariableSets.Delete(It.IsAny<LibraryVariableSetResource>())).Callback(
                (LibraryVariableSetResource set) =>
            {
                if (_sets.Contains(set))
                    _sets.Remove(set);
                else
                    throw new KeyNotFoundException("The given key was not present in the dictionary.");
            });
            

            octoRepo.Setup(o => o.LibraryVariableSets.Get("LibraryVariableSets-2")).Returns(_set);
            octoRepo.Setup(o => o.LibraryVariableSets.Get(It.IsNotIn(new[] {"LibraryVariableSets-2"})))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            // Allow the FindOne predicate to operate on the collection
            octoRepo.Setup(o => o.LibraryVariableSets.FindOne(It.IsAny<Func<LibraryVariableSetResource, bool>>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(
                    (Func<LibraryVariableSetResource, bool> f, string path, object pathParams) =>
                        (from l in _sets where f(l) select l).FirstOrDefault());
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
            _ps.AddCommand(CmdletName).AddParameter("InputObject", _set);
            _ps.Invoke();

            Assert.Equal(2, _sets.Count);
            Assert.DoesNotContain(_set, _sets);
        }

        [Fact]
        public void With_Invalid_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("InputObject", new LibraryVariableSetResource());
            _ps.Invoke();

            Assert.Equal(3, _sets.Count);
            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("The library variable set '' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "LibraryVariableSets-2");
            _ps.Invoke();

            Assert.Equal(2, _sets.Count);
            Assert.DoesNotContain(_set, _sets);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            _ps.Invoke();

            Assert.Equal(3, _sets.Count);
            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("The library variable set with id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Azure");
            _ps.Invoke();

            Assert.Equal(2, _sets.Count);
            Assert.DoesNotContain(_set, _sets);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish");
            _ps.Invoke();

            Assert.Equal(3, _sets.Count);
            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("The library variable set 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Azure");
            _ps.Invoke();

            Assert.Equal(2, _sets.Count);
            Assert.DoesNotContain(_set, _sets);
        }

        [Fact]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish").AddParameter("Id", "Gibberish");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Object_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Id", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_Id_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("Id", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
