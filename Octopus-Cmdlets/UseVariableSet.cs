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
using Octopus_Cmdlets.Extensions;
using Octopus_Cmdlets.Utilities;

namespace Octopus_Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Include a variable set in your project.</para>
    /// <para type="description">The Use-OctoVariableSet cmdlet includes a variable set in your project.</para>
    /// </summary>
    [Cmdlet(VerbsOther.Use, "VariableSet")]
    public class UseVariableSet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The project to include the variable set into.</para>
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Project { get; set; }

        /// <summary>
        /// <para type="description">The variable set names to include.</para>
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        private IOctopusRepository _octopus;
        private ProjectResource _project;

        /// <summary>
        /// BeginProcessing
        /// </summary>
        protected override void BeginProcessing()
        {
            _octopus = Session.RetrieveSession(this);
            // Find the project that owns the variables we want to edit
            _project = _octopus.Projects.FindByName(Project);

            if (_project == null)
                throw new Exception(string.Format("Project '{0}' was not found.", Project));

            WriteDebug("Found project" + _project.Id);
        }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Cache.LibraryVariableSets.IsExpired)
                Cache.LibraryVariableSets.Set(_octopus.LibraryVariableSets.FindAll());

            var varSets = new List<LibraryVariableSetResource>();

            foreach (var name in Name)
            {
                var nameForClosure = name;
                var varSet = (from v in Cache.LibraryVariableSets.Values
                    where v.Name.Equals(nameForClosure, StringComparison.InvariantCultureIgnoreCase)
                    select v).FirstOrDefault();

                if (varSet == null)
                    throw new Exception(string.Format("VariableSet '{0}' was not found.", name));

                varSets.Add(varSet);
            }

            foreach (var varSet in varSets)
                _project.IncludedLibraryVariableSetIds.Add(varSet.Id);
        }

        /// <summary>
        /// EndProcessing
        /// </summary>
        protected override void EndProcessing()
        {
            _octopus.Projects.Modify(_project);
            WriteVerbose("Wrote the project changes");
        }
    }
}
