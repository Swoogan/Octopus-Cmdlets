using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using System.Text;

namespace Octopus_Cmdlets.Tests
{
    public class AddCertificateTests
    {
        private const string CmdletName = "Add-OctoCertificate";
        private PowerShell _ps;
        private readonly List<CertificateResource> _certs = new List<CertificateResource>();

        public AddCertificateTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof (AddCertificate));
            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            _certs.Clear();

            var envRepo = new Mock<ICertificateRepository>();
            envRepo.Setup(c => c.Create(It.IsAny<CertificateResource>(), It.IsAny<object>()))
                .Returns((CertificateResource c, object o) =>
                {
                    _certs.Add(c);
                    return c;
                });

            octoRepo.Setup(o => o.Certificates).Returns(envRepo.Object);
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter(nameof(AddCertificate.Name), "Octopus_Dev");

            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_CertificateData()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter(nameof(AddCertificate.CertificateData), "CertData");

            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Name_And_CertificateData()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter(nameof(AddCertificate.Name), "Octopus_Dev")
                .AddParameter(nameof(AddCertificate.CertificateData), "CertData");
            _ps.Invoke();

            Assert.Single(_certs);
            Assert.Equal("Octopus_Dev", _certs[0].Name);
            Assert.Equal("CertData", _certs[0].CertificateData.NewValue);
        }

        [Fact]
        public void With_Name_And_CertificateData_And_Notes()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter(nameof(AddCertificate.Name), "Octopus_Dev")
                .AddParameter(nameof(AddCertificate.CertificateData), "CertData")
                .AddParameter(nameof(AddCertificate.Notes), "Octopus Development certificate");
            _ps.Invoke();

            Assert.Single(_certs);
            Assert.Equal("Octopus_Dev", _certs[0].Name);
            Assert.Equal("CertData", _certs[0].CertificateData.NewValue);
            Assert.Equal("Octopus Development certificate", _certs[0].Notes);
        }

        [Fact]
        public void With_Name_And_CertificateData_And_Environments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddParameter(nameof(AddCertificate.Name), "Octopus_Dev")
                .AddParameter(nameof(AddCertificate.CertificateData), "CertData")
                .AddParameter(nameof(AddCertificate.Environments), new[] { "Env1", "Env2" });
            _ps.Invoke();

            Assert.Single(_certs);
            Assert.Equal("Octopus_Dev", _certs[0].Name);
            Assert.Equal("CertData", _certs[0].CertificateData.NewValue);
            Assert.True(_certs[0].EnvironmentIds.SetEquals(new[] { "Env1", "Env2" }));
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName)
                .AddArgument("Octopus_Dev")
                .AddParameter(nameof(AddCertificate.CertificateData), "CertData");
            _ps.Invoke();

            Assert.Single(_certs);
            Assert.Equal("Octopus_Dev", _certs[0].Name);
            Assert.Equal("CertData", _certs[0].CertificateData.NewValue);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}
