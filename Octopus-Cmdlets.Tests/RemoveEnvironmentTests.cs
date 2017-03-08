using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class RemoveEnvironmentTests
    {
        private const string CmdletName = "Remove-OctoEnvironment";
        private PowerShell _ps;
        private readonly List<EnvironmentResource> _envs = new List<EnvironmentResource>();

        private readonly EnvironmentResource _env = new EnvironmentResource
        {
            Id = "Environments-2",
            Name = "Test"
        };

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(RemoveEnvironment));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _envs.Clear();
            _envs.Add(new EnvironmentResource { Id = "Environments-1", Name = "Dev" });
            _envs.Add(_env);
            _envs.Add(new EnvironmentResource { Id = "Environments-3", Name = "Prod" });

            octoRepo.Setup(o => o.Environments.Delete(It.IsAny<EnvironmentResource>()));

            octoRepo.Setup(o => o.Environments.Get("Environments-2")).Returns(_env);
            octoRepo.Setup(o => o.Environments.Get(It.IsNotIn(new[] { "Environments-2" })))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            octoRepo.Setup(o => o.Environments.FindByName("Test", null, null)).Returns(_env);
            octoRepo.Setup(o => o.Environments.FindByName("Gibberish", null, null)).Returns((EnvironmentResource) null);
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            _ps.Invoke();
        }

        //[TestMethod]
        //public void With_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("InputObject", _env);
        //    _ps.Invoke();

        //    Assert.AreEqual(2, _envs.Count);
        //    Assert.IsFalse(_envs.Contains(_env));
        //}

        //[TestMethod]
        //public void With_Invalid_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName).AddParameter("InputObject", new EnvironmentResource());
        //    _ps.Invoke();

        //    Assert.AreEqual(3, _envs.Count);
        //    Assert.AreEqual(1, _ps.Streams.Warning.Count);
        //    Assert.AreEqual("The library variable set '' does not exist.", _ps.Streams.Warning[0].ToString());
        //}

        [TestMethod]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new [] {"Environments-2"});
            _ps.Invoke();

            Assert.AreEqual(2, _envs.Count);
            Assert.IsFalse(_envs.Contains(_env));
        }

        [TestMethod]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] {"Gibberish"});
            _ps.Invoke();

            Assert.AreEqual(3, _envs.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("An environment with the id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] {"Test"});
            _ps.Invoke();

            Assert.AreEqual(2, _envs.Count);
            Assert.IsFalse(_envs.Contains(_env));
        }

        [TestMethod]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.AreEqual(3, _envs.Count);
            Assert.AreEqual(1, _ps.Streams.Warning.Count);
            Assert.AreEqual("The environment 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [TestMethod]
        public void With_Valid_And_Invalid_Names()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Test", "Gibberish" });
            _ps.Invoke();

            Assert.AreEqual(2, _envs.Count);
            Assert.IsFalse(_envs.Contains(_env));
        }

        [TestMethod]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument(new[] { "Test" });
            _ps.Invoke();

            Assert.AreEqual(2, _envs.Count);
            Assert.IsFalse(_envs.Contains(_env));
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish").AddParameter("Id", "Gibberish");
            _ps.Invoke();
        }

        //[TestMethod, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Name_And_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Name", "Gibberish")
        //        .AddParameter("InputObject", new EnvironmentResource());
        //    _ps.Invoke();
        //}

        //[TestMethod, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Object_And_Id()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new EnvironmentResource());
        //    _ps.Invoke();
        //}

        //[TestMethod, ExpectedException(typeof(ParameterBindingException))]
        //public void With_Name_Id_And_Object()
        //{
        //    // Execute cmdlet
        //    _ps.AddCommand(CmdletName)
        //        .AddParameter("Name", "Gibberish")
        //        .AddParameter("Id", "Gibberish")
        //        .AddParameter("InputObject", new EnvironmentResource());
        //    _ps.Invoke();
        //}
    }
}
