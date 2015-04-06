using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Client;

namespace Octopus_Cmdlets.Tests
{
    [TestClass]
    public class ConnectServerTests
    {
        private PowerShell _ps;
        private const string CmdletName = "Connect-OctoServer";

        [TestInitialize]
        public void Init()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(ConnectServer));
        }

        [TestMethod]
        public void Connection()
        {
            _ps.AddCommand(CmdletName)
                .AddArgument("http://localhost:8081")
                .AddArgument("API-ABCDEFGHIJKLMNOP");
            _ps.Invoke();

            Assert.IsNotNull(_ps.Runspace.SessionStateProxy.PSVariable.GetValue("OctopusRepository"));
            Assert.IsInstanceOfType(_ps.Runspace.SessionStateProxy.PSVariable.GetValue("OctopusRepository"),
                typeof (OctopusRepository));
        }

        [TestMethod, ExpectedException(typeof(ParameterBindingException))]
        public void Missing_Key()
        {
            _ps.AddCommand(CmdletName).AddArgument("http://localhost:8081");
            _ps.Invoke();
        }
    }
}
