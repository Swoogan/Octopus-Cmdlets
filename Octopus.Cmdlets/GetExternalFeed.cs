using System.Management.Automation;
using Octopus.Client;

namespace Octopus.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ExternalFeed")]
    public class GetExternalFeed : PSCmdlet
    {
        [Parameter(
           Position = 0,
           Mandatory = false,
           ValueFromPipeline = true,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The name of the project to get deployments for.")]
        public string[] Name { get; set; }

        private OctopusRepository _octopus;

        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
        }

        protected override void ProcessRecord()
        {
            var feeds = Name != null ? 
                _octopus.Feeds.FindByNames(Name) : 
                _octopus.Feeds.FindAll();

            foreach (var feed in feeds)
                WriteObject(feed);
        }
    }
}
