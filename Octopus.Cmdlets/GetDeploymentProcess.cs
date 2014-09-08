using System;
using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DeploymentProcess", DefaultParameterSetName = "ById")]
    public class GetDeploymentProcess : PSCmdlet
    {
        //[Parameter(
        //    ParameterSetName = "ByProjectName",
        //    Position = 0,
        //    Mandatory = true,
        //    ValueFromPipeline = true,
        //    ValueFromPipelineByPropertyName = true,
        //    HelpMessage = "The name of the project to retrieve the process for.")]
        //public string[] ProjectName { get; set; }

        //[Parameter(
        //    ParameterSetName = "ByProjectId",
        //    Mandatory = true,
        //    ValueFromPipeline = true,
        //    ValueFromPipelineByPropertyName = true,
        //    HelpMessage = "The id of the project to retrieve the process for.")]
        //public string[] ProjectId { get; set; }

        [Parameter(
            ParameterSetName = "ById",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The id of the deployment process to retrieve.")]
        public string[] DeploymentProcessId { get; set; }

        private OctopusRepository _octopus;
        
        protected override void BeginProcessing()
        {
            _octopus = (OctopusRepository) SessionState.PSVariable.GetValue("OctopusRepository");
            if (_octopus == null)
                throw new Exception(
                    "Connection not established. Please connect to your Octopus Deploy instance with Connect-OctoServer");

            WriteDebug("Connection established");
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                //case "ByProjectName":
                //    ProcessByProjectName();
                //    break;
                //case "ByProjectId":
                //    ProcessByProjectId();
                //    break;
                case "ById":
                    ProcessById();
                    break;
                default:
                    throw new Exception("Unknown ParameterSetName: " + ParameterSetName);
            }
        }

      
        private void ProcessById()
        {
            foreach (var id in DeploymentProcessId)
                WriteObject(_octopus.DeploymentProcesses.Get(id));
        }
    }
}
