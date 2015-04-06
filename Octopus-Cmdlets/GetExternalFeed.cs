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

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets the external feeds in the Octopus Deploy server.</para>
    /// <para type="description">The Get-OctoExternalFeed cmdlet gets the external feeds in the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>get-octoexternalfeed</code>
    ///   <para>This command gets all the external feeds.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "ExternalFeed")]
    public class GetExternalFeed : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project to get deployments for.</para>
        /// </summary>
        [Parameter(
           Position = 0,
           Mandatory = false,
           ValueFromPipeline = true,
           ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

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
            var feeds = Name != null ? 
                _octopus.Feeds.FindByNames(Name) : 
                _octopus.Feeds.FindAll();

            foreach (var feed in feeds)
                WriteObject(feed);
        }
    }
}
