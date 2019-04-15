using System.Collections.Generic;
using System.Management.Automation;
using Xunit;
using Moq;
using Octopus.Client.Exceptions;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Tests
{
    public class RemoveCertificateTests
    {
        private const string CmdletName = "Remove-OctoCertificate";
        private PowerShell _ps;
        private readonly List<CertificateResource> _certs = new List<CertificateResource>();

        private readonly CertificateResource _cert = new CertificateResource("CERT2", "CertData2");

        public RemoveCertificateTests()
        {
            _ps = Utilities.CreatePowerShell(CmdletName, typeof(RemoveCertificate));

            var octoRepo = Utilities.AddOctopusRepo(_ps.Runspace.SessionStateProxy.PSVariable);

            // Create some library variable sets
            _certs.Clear();
            _certs.Add(new CertificateResource("CERT 1", "CertData1") { Id = "CERT1" });
            _certs.Add(_cert);
            _certs.Add(new CertificateResource("CERT 3", "CertData3") { Id = "CERT3" });

            octoRepo.Setup(o => o.Certificates.Delete(It.IsAny<CertificateResource>())).Callback(
                (CertificateResource set) =>
                {
                    if (_certs.Contains(set))
                        _certs.Remove(set);
                    else
                        throw new KeyNotFoundException("The given key was not present in the dictionary.");
                }
                );

            octoRepo.Setup(o => o.Certificates.Get("CERT1")).Returns(_cert);
            octoRepo.Setup(o => o.Certificates.Get(It.IsNotIn(new[] { "CERT1" })))
                .Throws(new OctopusResourceNotFoundException("Not Found"));

            octoRepo.Setup(o => o.Certificates.FindByName("CERT 2", It.IsAny<string>(), It.IsAny<object>())).Returns(_cert);
            octoRepo.Setup(o => o.Certificates.FindByName("Gibberish", It.IsAny<string>(), It.IsAny<object>())).Returns((CertificateResource) null);
        }

        [Fact]
        public void No_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName);
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }

        [Fact]
        public void With_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new [] {"CERT1"});
            _ps.Invoke();

            Assert.Equal(2, _certs.Count);
            Assert.DoesNotContain(_cert, _certs);
        }

        [Fact]
        public void With_Invalid_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Id", new[] {"Gibberish"});
            _ps.Invoke();

            Assert.Equal(3, _certs.Count);
            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("A certificate with the id 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] {"CERT 2"});
            _ps.Invoke();

            Assert.Equal(2, _certs.Count);
            Assert.DoesNotContain(_cert, _certs);
        }

        [Fact]
        public void With_Invalid_Name()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "Gibberish" });
            _ps.Invoke();

            Assert.Equal(3, _certs.Count);
            Assert.Single(_ps.Streams.Warning);
            Assert.Equal("The certificate 'Gibberish' does not exist.", _ps.Streams.Warning[0].ToString());
        }

        [Fact]
        public void With_Valid_And_Invalid_Names()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", new[] { "CERT 2", "Gibberish" });
            _ps.Invoke();

            Assert.Equal(2, _certs.Count);
            Assert.DoesNotContain(_cert, _certs);
        }

        [Fact]
        public void With_Arguments()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddArgument(new[] { "CERT 2" });
            _ps.Invoke();

            Assert.Equal(2, _certs.Count);
            Assert.DoesNotContain(_cert, _certs);
        }

        [Fact]
        public void With_Name_And_Id()
        {
            // Execute cmdlet
            _ps.AddCommand(CmdletName).AddParameter("Name", "Gibberish").AddParameter("Id", "Gibberish");
            Assert.Throws<ParameterBindingException>(() => _ps.Invoke());
        }
    }
}