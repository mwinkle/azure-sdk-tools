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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.AzureTools
{
    using System.IO;
    using CloudService.Model;
    using CloudService.Properties;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsPackTests : TestBase
    {
        private const string serviceName = "AzureService";

        [TestMethod]
        public void CreateLocalPackageWithOneNodeWebRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                RoleInfo webRoleInfo = service.AddWebRole(Resources.NodeScaffolding);
                string logsDir = Path.Combine(service.Paths.RootPath, webRoleInfo.Name, "server.js.logs");
                string logFile = Path.Combine(logsDir, "0.txt");
                string targetLogsFile = Path.Combine(service.Paths.LocalPackage, "roles", webRoleInfo.Name, @"approot\server.js.logs\0.txt");
                files.CreateDirectory(logsDir);
                files.CreateEmptyFile(logFile);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
                Assert.IsTrue(File.Exists(targetLogsFile));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithOnePHPWebRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                RoleInfo webRoleInfo = service.AddWebRole(Resources.PHPScaffolding);
                string logsDir = Path.Combine(service.Paths.RootPath, webRoleInfo.Name, "server.js.logs");
                string logFile = Path.Combine(logsDir, "0.txt");
                string targetLogsFile = Path.Combine(service.Paths.LocalPackage, "roles", webRoleInfo.Name, @"approot\server.js.logs\0.txt");
                files.CreateDirectory(logsDir);
                files.CreateEmptyFile(logFile);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WebRole));
                Assert.IsTrue(File.Exists(targetLogsFile));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithNodeWorkerRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWorkerRole(Resources.NodeScaffolding);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithPHPWorkerRoleTest()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWorkerRole(Resources.PHPScaffolding);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WorkerRole));
            }
        }

        [TestMethod]
        public void CreateLocalPackageWithMultipleRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string standardOutput;
                string standardError;
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWorkerRole(Resources.NodeScaffolding);
                service.AddWebRole(Resources.NodeScaffolding);
                service.AddWorkerRole(Resources.PHPScaffolding);
                service.AddWebRole(Resources.PHPScaffolding);
                service.CreatePackage(DevEnv.Local, out standardOutput, out standardError);

                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole1\approot"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WorkerRole2\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WorkerRole));
                AzureAssert.ScaffoldingExists(Path.Combine(service.Paths.LocalPackage, @"roles\WebRole2\approot"), Path.Combine(Resources.PHPScaffolding, Resources.WebRole));
            }
        }
    }
}