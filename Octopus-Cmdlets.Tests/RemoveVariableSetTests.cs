using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
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

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (RemoveVariableSet));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _sets.Clear();
            _sets.Add(new LibraryVariableSetResource {Id = "LibraryVariableSets-1", Name = "ConnectionStrings"});
            _sets.Add(_set);
            _sets.Add(new LibraryVariableSetResource {Id = "LibraryVariableSets-3", Name = "Service Endpoints"});

            octoRepo.Setup(o => o.LibraryVariableSets.Delete(It.IsAny<LibraryVariableSetResource>()));

            octoRepo.Setup(o => o.LibraryVariableSets.Get("LibraryVariableSets-2")).Returns(_set);
            octoRepo.Setup(o => o.LibraryVariableSets.Get(It.IsNotIn(new[] {"LibraryVariableSets-2"})))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            // Allow the FindOne predicate to operate on the collection
            octoRepo.Setup(o => o.LibraryVariableSets.FindOne(It.IsAny<Func<LibraryVariableSetResource, bool>>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(
                    (Func<LibraryVariableSetResource, bool> f, string path, object pathParams) =>
                        (from l in _sets where f(l) select l).FirstOrDefault());
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
            _ps.AddCommand(CmdletName).AddParameter("InputObject", _set);
            _ps.Invoke();

            Assert.AreEqual(2, _sets.Count);
            Assert.IsFalse(_sets.Contains(_set));
        }

        [TestMethod]
        public void With_Invalid_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("InputObject", new LibraryVariableSetResource());
            _ps.Invoke();

            Assert.AreEqual(3, _sets.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("The library variable set '' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "LibraryVariableSets-2");
            _ps.Invoke();

            Assert.AreEqual(2, _sets.Count);
            Assert.IsFalse(_sets.Contains(_set));
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            _ps.Invoke();

            Assert.AreEqual(3, _sets.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("The library variable set with id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Azure");
            _ps.Invoke();

            Assert.AreEqual(2, _sets.Count);
            Assert.IsFalse(_sets.Contains(_set));
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish");
            _ps.Invoke();

            Assert.AreEqual(3, _sets.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("The library variable set 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Azure");
            _ps.Invoke();

            Assert.AreEqual(2, _sets.Count);
            Assert.IsFalse(_sets.Contains(_set));
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish").AddParameter("Id", "Gibberish");
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Name_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Object_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Id", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            _ps.Invoke();
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Name_Id_And_Object()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter("Name", "Gibberish")
                .AddParameter("Id", "Gibberish")
                .AddParameter("InputObject", new LibraryVariableSetResource());
            _ps.Invoke();
        }
    }
}
