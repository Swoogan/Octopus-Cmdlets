using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Extensions
{
    public static class Release
    {
        // Wanted to do this as an extension method, but IReleaseRepository doesn't have the info
        // needed to actually make a call the api
        public static ReleaseResource FindByVersion(OctopusRepository octopus, string projectId, string version)
        {
            var uri = string.Format("/api/projects/{0}/releases/{1}", projectId, version);
            return octopus.Client.Get<ReleaseResource>(uri);
        }
    }
}
