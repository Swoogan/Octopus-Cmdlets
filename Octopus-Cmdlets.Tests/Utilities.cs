using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Moq;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories;

namespace Octopus_Cmdlets.Tests
{
    class Utilities
    {
        /// <summary>
        /// Create the runspace and add the cmdlet to it
        /// </summary>
        private static Runspace CreateRunspace(string name, Type cmdlet)
        {
            var config = RunspaceConfiguration.Create();
            config.Cmdlets.Append(new [] { new CmdletConfigurationEntry(name, cmdlet, "") });
            return RunspaceFactory.CreateRunspace(config); 
        }

        /// <summary>
        /// Create hosted PowerShell session
        /// </summary>
        public static PowerShell CreatePowerShell(string name, Type cmdlet)
        {
            var ps = PowerShell.Create();
            ps.Runspace = CreateRunspace(name, cmdlet);
            ps.Runspace.Open();
            return ps;
        }

        public static Mock<IOctopusRepository> AddOctopusRepo(PSVariableIntrinsics psVariable)
        {
            var octoRepo = new Mock<IOctopusRepository>();

            // Add the mock repository to the session to simulate 'Connect-OctoServer'
            psVariable.Set("OctopusRepository", octoRepo.Object);

            return octoRepo;
        }
    }
}
