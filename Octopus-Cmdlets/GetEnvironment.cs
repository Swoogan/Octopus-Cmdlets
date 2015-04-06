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
using Octopus.Platform.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets the environments in the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoEnvironment cmdlet gets the environments in the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octoenvironment</code>
    ///   <para>This command gets all the environments.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "Environment", DefaultParameterSetName = "ByName")]
    public class GetEnvironment : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the environment to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">The id of the environment to retrieve.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string[] EnvironmentId { get; set; }

        /// <summary>
        /// <para type="description">The environment to retrieve by ScopeValue.</para>
        /// </summary>
        [Parameter(
            ParameterSetName = "ByScope",
            Mandatory = true)]
        [Alias("Scope")]
        public ScopeValue ScopeValue { get; set; }

        /// <summary>
        /// <para type="description">Determines whether to temporarily cache the results or not.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private List<EnvironmentResource> _environments;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            WriteDebug("Connection established");

            if (!Cache || Extensions.Cache.Environments.IsExpired)
                _environments = _octopus.Environments.FindAll();

            if (Cache)
            {
                if (Extensions.Cache.Environments.IsExpired)
                    Extensions.Cache.Environments.Set(_environments);
                else
                    _environments = Extensions.Cache.Environments.Values;
            }

            WriteDebug("Loaded environments");
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
                case "ByScope":
                    ProcessByScope();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByScope()
        {
            var envs = ScopeValue == null
                ? _environments
                : (from e in _environments
                   from sv in ScopeValue
                   where e.Id == sv
                   select e);

            foreach (var env in envs)
                WriteObject(env);
        }

        private void ProcessByName()
        {
            var envs = Name == null
                ? _environments
                : (from e in _environments
                    from n in Name
                    where e.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                    select e);

            foreach (var env in envs)
                WriteObject(env);
        }

        private void ProcessById()
        {
            var envs = from e in _environments
                       from id in EnvironmentId
                       where id == e.Id
                       select e;

            foreach (var env in envs)
                WriteObject(env);
        }
    }
}
