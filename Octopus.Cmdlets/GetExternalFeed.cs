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

        private IOctopusRepository _octopus;

        public GetExternalFeed() {  }

        public GetExternalFeed(IOctopusRepository repo)
        {
            _octopus = repo;
        }

        protected override void BeginProcessing()
        {
            if (_octopus == null)
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
