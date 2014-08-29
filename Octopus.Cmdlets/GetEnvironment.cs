using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

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
            HelpMessage = "The name of the environment to look for.")]
        public string[] Name { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the environment to look for.")]
        public string[] Id { get; set; }

        [Parameter(
            Mandatory = false)]
        public SwitchParameter NoCache { get; set; }

        private OctopusRepository _octopus;

        private const int CacheDuration = 60;
        private static DateTime _age = DateTime.MinValue;
        private static List<EnvironmentResource> _environments;

        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to you Octopus Deploy instance with Connect-OctoServer");

            WriteDebug("Connection established");

            if (_environments == null || _age < DateTime.Now.AddSeconds(-CacheDuration) || NoCache)
            {
                _age = DateTime.Now;
                _environments = _octopus.Environments.FindAll();
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
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

        private void ProcessByName()
        {
            IEnumerable<EnvironmentResource> envs;

            if (Name == null)
            {
                envs = _environments;
            }
            else
            {
                envs = from e in _environments
                    from n in Name
                    where e.Name.Equals(n, StringComparison.InvariantCultureIgnoreCase)
                    select e;
            }

            foreach (var env in envs)
                WriteObject(env);
        }

        private void ProcessById()
        {
            var envs = from e in _environments
                       from id in Id
                       where id == e.Id
                       select e;

            foreach (var env in envs)
                WriteObject(env);
        }
    }
}
