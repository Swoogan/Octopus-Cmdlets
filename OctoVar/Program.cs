using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Platform.Model;

namespace OctoVar
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var octopusServerEndpoint = new OctopusServerEndpoint("http://aprdappvm030:81/", "API-IQJURANHERTYDKLAZG9CKUBQHY");
            var octopus = new OctopusRepository(octopusServerEndpoint);

            // Find the project that owns the variables we want to edit
            var project = octopus.Projects.FindByName("CSIS");

            // Get the variables for editing
            var variableSet = octopus.VariableSets.Get(project.Link("Variables"));

            // Add a new variable
            variableSet.Variables.Add(new VariableResource
            {
                Name = "JimmyCrackedCorn",
                Value = "AndIDontCare",
                Scope = new ScopeSpecification
                {
                    // Scope the variable to two environments using their environment ID
                    {ScopeField.Environment, new ScopeValue("Environments-1", "Environments-2")}
                },
            });

            // Save the variables
            octopus.VariableSets.Modify(variableSet);
        }
    }
}
