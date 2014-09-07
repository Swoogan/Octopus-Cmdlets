using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Extensions;
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
        public SwitchParameter NoCache { get; set; }

        private OctopusRepository _octopus;
        
        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");

            WriteDebug("Connection established");

            if (Cache.Environments.IsExpired || NoCache)
            {
                Cache.Environments.Set(_octopus.Environments.FindAll());
                WriteDebug("Cache miss");
            }
            else
            {
                WriteDebug("Cache hit");
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
                ? Cache.Environments.Values
                : (from e in Cache.Environments.Values
                   from sv in ScopeValue
                   where e.Id == sv
                   select e);

            foreach (var env in envs)
                WriteObject(env);
        }

        private void ProcessByName()
        {
            var envs = Name == null
                ? Cache.Environments.Values
                : (from e in Cache.Environments.Values
                    from n in Name
                    where e.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                    select e);

            foreach (var env in envs)
                WriteObject(env);
        }

        private void ProcessById()
        {
            var envs = from e in Cache.Environments.Values
                       from id in EnvironmentId
                       where id == e.Id
                       select e;

            foreach (var env in envs)
                WriteObject(env);
        }
    }
}
