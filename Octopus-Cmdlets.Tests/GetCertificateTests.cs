using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class GetCertificateTests
    {
        private const string CmdletName = "Get-OctoCertificate";
        private PowerShell _ps;

        public GetCertificateTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(GetCertificate));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some certificates
            var certificates = new List<CertificateResource>
            {
                new CertificateResource("Octopus", "OctopusData") { Id = "certificates-1", EnvironmentIds = new ReferenceCollection("env1")},
                new CertificateResource("Deploy", "DeployData") { Id = "certificates-2", EnvironmentIds = new ReferenceCollection("env2")},
                new CertificateResource("Automation", "AutomationData") { Id = "certificates-3", EnvironmentIds = new ReferenceCollection("env3")},
                new CertificateResource("Server", "ServerData") { Id = "certificates-4", EnvironmentIds = new ReferenceCollection("env4")},
            };
            octoRepo.Setup(o => o.Certificates.FindAll(null, null)).Returns(certificates);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Equal(4, certificates.Count);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Octopus");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Single(certificates);
            Assert.Equal("Octopus", certificates[0].Name);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument("Gibberish");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Empty(certificates);
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "certificates-1");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Single(certificates);
            Assert.Equal("Octopus", certificates[0].Name);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", "Gibberish");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Empty(certificates);
        }

        [Fact]
        public void With_Environment()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Environment", "env1");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Single(certificates);
            Assert.Equal("Octopus", certificates[0].Name);
        }

        [Fact]
        public void With_Invalid_EnvironmentValue()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Environment", "Gibberish");
            var certificates = _ps.Invoke<CertificateResource>();

            Assert.Empty(certificates);
        }

        [Fact]
        public void With_Id_And_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Name").AddParameter("Id", "Id");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
