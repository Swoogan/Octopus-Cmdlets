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
using Octopus.Client.Model;

namespace Octopus.Extensions
{
    public static class Cache
    {
        public const int Duration = 60;
        public static readonly CacheNode<EnvironmentResource> Environments;
        public static readonly CacheNode<LibraryVariableSetResource> LibraryVariableSets;
        public static readonly CacheNode<ProjectResource> Projects;
        public static readonly CacheNode<ReleaseResource> Releases;

        static Cache()
        {
            LibraryVariableSets = new CacheNode<LibraryVariableSetResource>();
            Environments = new CacheNode<EnvironmentResource>();
            Projects = new CacheNode<ProjectResource>();
            Releases = new CacheNode<ReleaseResource>();
        }
    }

    public class CacheNode<T>
    {
        public int Duration { get; private set; }
        public DateTime Age { get; private set; }
        public List<T> Values { get; private set; }

        public CacheNode()
        {
            Age = DateTime.MinValue;
            Duration = Cache.Duration;
            // Values = new List<T>();
        }

        public void Set(List<T> values)
        {
            Age = DateTime.Now;
            Values = values;
        }

        public bool IsExpired
        {
            get { return Age < DateTime.Now.AddSeconds(-Duration); }
        }
    }

    /*
    public static class Cache
    {
        public const int CacheDuration = 60;
        public static readonly Dictionary<string, CacheNode<EnvironmentResource>> Environments;
        public static readonly Dictionary<string, CacheNode<LibraryVariableSetResource>> LibraryVariableSets;

        static Cache()
        {
            LibraryVariableSets = new Dictionary<string, CacheNode<LibraryVariableSetResource>>();
            Environments = new Dictionary<string, CacheNode<EnvironmentResource>>();
        }
    }

    public class CacheNode<T>
    {
        public int CacheDuration { get; set; }
        public DateTime Age { get; set; }
        public T Value { get; set; }

        public CacheNode()
        {
            Age = DateTime.MinValue;
            CacheDuration = Cache.CacheDuration;
        }

        public CacheNode(T value) : this()
        {
            Value = value;
        }

        public bool IsExpired()
        {
            return Age < DateTime.Now.AddSeconds(-CacheDuration);
        }
    }
     */
}
