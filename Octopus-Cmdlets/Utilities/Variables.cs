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

using System.Collections.Generic;
using Octopus.Client.Model;

namespace Octopus_Cmdlets.Utilities
{
    delegate void Warn(string text);

    delegate ScopeValue CopyScopeValue(KeyValuePair<ScopeField, ScopeValue> scope);

    class Variables
    {
        private readonly IList<VariableResource> _variableSet;
        private readonly Warn _writeWarning;

        public Variables(IList<VariableResource> destination, Warn writeWarning)
        {
            _variableSet = destination;
            _writeWarning = writeWarning;
        }

        public void CopyVariables(IList<VariableResource> source, CopyScopeValue copyAction = null)
        {
            foreach (var variable in source)
            {
                if (variable.IsSensitive)
                {
                    const string warning =
                        "Variable '{0}' was sensitive. Sensitive flag has been removed and the value has been set to an empty string.";
                    _writeWarning(string.Format(warning, variable.Name));
                }

                var newVariable = new VariableResource
                {
                    Name = variable.Name,
                    IsEditable = variable.IsEditable,
                    IsSensitive = false,
                    Prompt = variable.Prompt,
                    Value = variable.IsSensitive ? "" : variable.Value,
                    Scope = CreateScope(variable.Scope, copyAction)
                };

                _variableSet.Add(newVariable);
            }
        }

        private static ScopeSpecification CreateScope(ScopeSpecification scopeSpec, CopyScopeValue copyAction = null)
        {
            var spec = new ScopeSpecification();

            foreach (var scope in scopeSpec)
            {
                if (scope.Key != ScopeField.Action)
                    spec.Add(scope.Key, new ScopeValue(scope.Value));
                else if (copyAction != null)
                    spec.Add(scope.Key, copyAction(scope));
            }

            return spec;
        }
    }
}
