using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Extensions
{
    public class ActionTemplate
    {
        public static ActionTemplateResource GetActionTemplate(OctopusRepository repo, string id)
        {
            return repo.Client.Get<ActionTemplateResource>("/api/actiontemplates/{id}", id);
        }

        public static ResourceCollection<ActionTemplateResource> GetActionTemplates(OctopusRepository repo)
        {
            return repo.Client.List<ActionTemplateResource>("/api/actiontemplates/");
        }
    }
}
