#region License
// Copyright 2014 Colin Svingen

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Exceptions;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Remove a certificate from the Octopus Deploy server.</para>
    /// <para type="description">The Remove-OctoCertificate cmdlet removes a certificate from the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>remove-octocertificate CERT1</code>
    ///   <para>
    ///      Remove the certificate named 'CERT1'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "Certificate")]
    public class RemoveCertificate : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the certificate to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the certificate to remove.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = false)]
        [Alias("CertificateId")]
        public string[] Id { get; set; }

        private IOctopusRepository _octopus;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    ProcessByName();
                    break;
                case "ById":
                    ProcessById();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessById()
        {
            foreach (var id in Id)
            {
                try
                {
                    var cert = _octopus.Certificates.Get(id);
                    WriteVerbose("Deleting certificate: " + cert.Name);
                    _octopus.Certificates.Delete(cert);
                }
                catch (OctopusResourceNotFoundException)
                {
                    WriteWarning(string.Format("A certificate with the id '{0}' does not exist.", id));
                }
            }
        }

        private void ProcessByName()
        {
            foreach (var name in Name)
            {
                var cert = _octopus.Certificates.FindByName(name);
                if (cert != null)
                {
                    WriteVerbose("Deleting certificate: " + cert.Name);
                    _octopus.Certificates.Delete(cert);
                }
                else
                {
                    WriteWarning(string.Format("The certificate '{0}' does not exist.", name));
                }
            }
        }
    }
}
