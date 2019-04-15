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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets the certificates in the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoCertificate cmdlet gets the certificates in the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octocertificate</code>
    ///   <para>This command gets all the certificates.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Certificate", DefaultParameterSetName = "ByName")]
    public class GetCertificate : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the certificate to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the certificate to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string[] CertificateId { get; set; }

        /// <summary>
        /// <para type="description">The environments of the certificates to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByEnvironment",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Environment { get; set; }

        /// <summary>
        /// <para type="description">Determines whether to temporarily cache the results or not.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private List<CertificateResource> _certificates;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Connection established");

            if (!Cache || Utilities.Cache.Certificates.IsExpired)
                _certificates = _octopus.Certificates.FindAll();

            if (Cache)
            {
                if (Utilities.Cache.Certificates.IsExpired)
                    Utilities.Cache.Certificates.Set(_certificates);
                else
                    _certificates = Utilities.Cache.Certificates.Values;
            }

            WriteDebug("Loaded certificates");
        }

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
                case "ByEnvironment":
                    ProcessByEnvironment();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByEnvironment()
        {
            var certs = Environment == null
                ? _certificates
                : (from c in _certificates
                   from cenv in c.EnvironmentIds
                   from env in Environment
                   where cenv == env
                   select c);

            foreach (var cert in certs)
                WriteObject(cert);
        }

        private void ProcessByName()
        {
            var certs = Name == null
                ? _certificates
                : (from c in _certificates
                    from n in Name
                    where c.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                    select c);

            foreach (var cert in certs)
                WriteObject(cert);
        }

        private void ProcessById()
        {
            var certs = from c in _certificates
                       from id in CertificateId
                       where id == c.Id
                       select c;

            foreach (var cert in certs)
                WriteObject(cert);
        }
    }
}
