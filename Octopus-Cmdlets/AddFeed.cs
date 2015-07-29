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
    /// <para type="synopsis">Add a new external feed to the Octopus Deploy server.</para>
    /// <para type="description">The Add-OctoFeed cmdlet adds a external feed to the Octopus Deploy server.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>add-octofeed DEV</code>
    ///   <para>
    ///      Add a new feed named 'DEV'.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>PS C:\>add-octofeed -Name DEV -Uri "\\test"</code>
    ///   <para>
    ///      Add a new feed named 'DEV' pointing to the path '\\test'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "Feed")]
    public class AddFeed : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the feed to create.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The URI of the feed to create.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public string Uri { get; set; }

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
            _octopus.Feeds.Create(new FeedResource
            {
                Name = Name,
                FeedUri = Uri
            });
        }
    }
}
