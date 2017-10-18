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
using System.Linq;
using System.Management.Automation;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Copy a channel in an Octopus Deploy project.</para>
    /// <para type="description">The Get-OctoCopyChannel cmdlet copies a channel in an Octopus Deploy project.</para>
    /// </summary>
    /// <example>
    ///   <code>PS C:\>copy-octochannel Example Website</code>
    ///   <para>
    ///      Make a copy the channel named 'Priority' in the project named 'Example'. Since no
    ///      destination argument was specified, the new channel will be named 'Priority - Copy'.
    ///   </para>
    /// </example>
    [Cmdlet(VerbsCommon.Copy, "Channel")]
    public class CopyChannel : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name of the project containing the channel to copy.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The name of the channel to copy.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The name of the new channel.</para>
        /// </summary>
        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the new channel.")]
        public string Destination { get; set; }

        private IOctopusRepository _octopus;

        private ChannelResource _channel;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);

            // Find the project that owns the channel we want to get
            var project = _octopus.Projects.FindByName(Project);

            if (project == null)
            {
                throw new Exception($"Project '{Project}' was not found.");
            }
            
            _channel = _octopus.Channels.FindByName(project, Name);

            if (_channel == null)
            {
                throw new Exception($"Channel '{Name}' was not found.");
            }
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            var clone = new ChannelResource
            {
                Name = GetName(_channel.Name),
                ProjectId = _channel.ProjectId,
                Description = _channel.Description,
                LifecycleId = _channel.LifecycleId,
                Rules = _channel.Rules.Select(CloneRule).ToList(),
                TenantTags = _channel.TenantTags.Clone()
            };

            foreach (var link in _channel.Links)
            {
                clone.Links.Add(link.Key, link.Value);
            }

            _octopus.Channels.Create(clone);
        }

        private static ChannelVersionRuleResource CloneRule(ChannelVersionRuleResource rule)
        {
            var clone = new ChannelVersionRuleResource
            {
                Tag = rule.Tag,
                VersionRange = rule.VersionRange,
                Actions = rule.Actions.Clone()
            };

            foreach (var link in rule.Links)
            {
                clone.Links.Add(link.Key, link.Value);
            }

            return clone;
        }

        private string GetName(string name) => string.IsNullOrWhiteSpace(Destination) ? name + " - Copy" : Destination;
    }
}
