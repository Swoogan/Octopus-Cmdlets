using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Octopus.Cmdlets.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private PowerShell _ps;

        [TestInitialize]
        public void Init()
        {
            _ps = PowerShell.Create();
            _ps.AddCommand("Import-Module").AddArgument("Octopus.Cmdlets");
            _ps.AddCommand("Connect-OctoServer").AddArgument("http://localhost:8081/app#/").AddArgument("API-RPOIJGMFC3PKAYEGKJRXZ50ODQ");
        }
        
        [TestMethod]
        public void TestMethod1()
        {
            _ps.AddCommand("Get-OctoFeeds");
            _ps.Invoke();
        }
    }
}
