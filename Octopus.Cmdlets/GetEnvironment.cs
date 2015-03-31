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

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Environment", DefaultParameterSetName = "ByName")]
    public class GetEnvironment : PSCmdlet
    {
        [Parameter(
            ParameterSetName = "ByName",
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the environment to retrieve.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the environment to retrieve.")]
        [Alias("Id")]
        public string[] EnvironmentId { get; set; }

        [Parameter(
            ParameterSetName = "ByScope",
            Mandatory = true,
            HelpMessage = "The environment to retrieve by ScopeValue.")]
        [Alias("Scope")]
        public ScopeValue ScopeValue { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Cache { get; set; }

        private IOctopusRepository _octopus;
        private List<EnvironmentResource> _environments;

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
