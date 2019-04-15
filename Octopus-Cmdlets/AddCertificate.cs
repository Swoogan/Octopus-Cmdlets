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

using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Add a new certificate to the Octopus Deploy server.</para>
    /// <para type="description">The Add-OctoCertificate cmdlet adds a new certificate to the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>add-octocertificate CERT1 -FilePath .\cert1.cer</code>
    ///   <para>
    ///      Uploads a new certificate named 'CERT1' from file '.\cert1.cer'.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>add-octocertificate CERT2 -FilePath .\cert1.pfx -Password MyPassword</code>
    ///   <para>
    ///      Uploads a new certificate named 'CERT2' from file '.\cert1.pfx' that is encrypted using password 'MyPassword'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "Certificate")]
    public class AddCertificate : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the certificate to create.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The certificate data.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string CertificateData { get; set; }

        /// <summary>
        /// <para type="description">Notes about the certificate to create.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Notes { get; set; }

        /// <summary>
        /// <para type="description">The password protecting the certificate data.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">The environment scopes of the certificate to create.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] Environments { get; set; }

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
            var certificate = new CertificateResource(
                Name,
                CertificateData,
                Password)
            {
                Notes = Notes,
                EnvironmentIds = new ReferenceCollection(Environments)
            };

            _octopus.Certificates.Create(
                certificate,
                null);
        }
    }
}
