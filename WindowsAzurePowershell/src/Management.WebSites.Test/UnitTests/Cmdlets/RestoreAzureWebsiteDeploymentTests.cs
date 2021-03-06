﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Cmdlets
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Management.Services;
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Model;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services.DeploymentEntities;
    using Websites.Services.WebEntities;

    [TestClass]
    public class RestoreAzureWebsiteDeploymentTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void RestoreAzureWebsiteDeploymentTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();

            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1", SiteProperties = new SiteProperties
                        {
                            Properties = new List<NameValuePair>
                            {
                                new NameValuePair { Name = "repositoryuri", Value = "http" },
                                new NameValuePair { Name = "PublishingUsername", Value = "user1" },
                                new NameValuePair { Name = "PublishingPassword", Value = "password1" }
                            }
                        }} 
                    });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
            };

            SimpleDeploymentServiceManagement deploymentChannel = new SimpleDeploymentServiceManagement();

            var deployments = new List<DeployResult> { new DeployResult { Id = "id1", Current = false }, new DeployResult { Id = "id2", Current = true } };
            deploymentChannel.GetDeploymentsThunk = ar => deployments;
            deploymentChannel.DeployThunk = ar =>
            {
                // Keep track of currently deployed id
                DeployResult newDeployment = deployments.FirstOrDefault(d => d.Id.Equals(ar.Values["commitId"]));
                if (newDeployment != null)
                {
                    // Make all inactive
                    deployments.ForEach(d => d.Complete = false);
                    
                    // Set new to active
                    newDeployment.Complete = true;
                }
            };

            // Test
            RestoreAzureWebsiteDeploymentCommand restoreAzureWebsiteDeploymentCommand =
                new RestoreAzureWebsiteDeploymentCommand(channel, deploymentChannel)
            {
                Name = "website1",
                CommitId = "id2",
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = "RestoreAzureWebsiteDeploymentTests_RestoreAzureWebsiteDeploymentTest2" }
            };

            restoreAzureWebsiteDeploymentCommand.ExecuteCommand();
            Assert.AreEqual(1, ((MockCommandRuntime) restoreAzureWebsiteDeploymentCommand.CommandRuntime).WrittenObjects.Count);
            var responseDeployments = (IEnumerable<DeployResult>)((MockCommandRuntime) restoreAzureWebsiteDeploymentCommand.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.IsNotNull(responseDeployments);
            Assert.AreEqual(2, responseDeployments.Count());
            Assert.IsTrue(responseDeployments.Any(d => d.Id.Equals("id2") && d.Complete));
            Assert.IsTrue(responseDeployments.Any(d => d.Id.Equals("id1") && !d.Complete));

            // Change active deployment to id1
            restoreAzureWebsiteDeploymentCommand = new RestoreAzureWebsiteDeploymentCommand(channel, deploymentChannel)
            {
                Name = "website1",
                CommitId = "id1",
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = "RestoreAzureWebsiteDeploymentTests_RestoreAzureWebsiteDeploymentTest1" }
            };

            restoreAzureWebsiteDeploymentCommand.ExecuteCommand();
            Assert.AreEqual(1, ((MockCommandRuntime)restoreAzureWebsiteDeploymentCommand.CommandRuntime).WrittenObjects.Count);
            responseDeployments = (IEnumerable<DeployResult>)((MockCommandRuntime)restoreAzureWebsiteDeploymentCommand.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.IsNotNull(responseDeployments);
            Assert.AreEqual(2, responseDeployments.Count());
            Assert.IsTrue(responseDeployments.Any(d => d.Id.Equals("id1") && d.Complete));
            Assert.IsTrue(responseDeployments.Any(d => d.Id.Equals("id2") && !d.Complete));
        }
    }
}
