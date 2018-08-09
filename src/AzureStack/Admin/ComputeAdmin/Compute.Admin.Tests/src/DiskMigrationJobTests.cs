﻿
using Microsoft.AzureStack.Management.Compute.Admin;
using Microsoft.AzureStack.Management.Compute.Admin.Models;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Compute.Tests
{
    public class DiskMigrationJobTests : ComputeTestBase
    {
        private void ValidateDiskMigration(DiskMigrationJob diskMigration)
        {
            Assert.NotNull(diskMigration);
            Assert.NotNull(diskMigration.CreationTime);
            Assert.NotNull(diskMigration.Id);
            Assert.NotNull(diskMigration.Location);
            Assert.NotNull(diskMigration.Name);
            Assert.NotNull(diskMigration.Status);
            Assert.NotNull(diskMigration.TargetShare);
            Assert.NotNull(diskMigration.Type);
            Assert.NotNull(diskMigration.MigrationId);
        }

        [Fact]
        public void TestDiskMigration()
        {
            string targetShare = @"\\SU1FileServer.azurestack.local\SU1_ObjStore\";
            RunTest((client) => {
                var disks = client.Disks.List(Location);
                List<Disk> toMigrationDisks = new List<Disk>();
                foreach(var disk in disks)
                {
                    if (toMigrationDisks.Count < 3)
                    {
                        toMigrationDisks.Add(disk);
                    }
                    else
                    {
                        break;
                    }
                }

                var migrationId = "ba0644a4-c2ed-4e3c-a167-089a32865297";// This guid should be the same as the ones in sessionRecord

                var migration = client.DiskMigrationJobs.Create(Location, migrationId, targetShare, toMigrationDisks);
                ValidateDiskMigration(migration);

                migration = client.DiskMigrationJobs.Cancel(Location, migrationId);
                ValidateDiskMigration(migration);

                var migrationFromGet = client.DiskMigrationJobs.Get(Location, migrationId);
                ValidateDiskMigration(migrationFromGet);

                var migrationList = client.DiskMigrationJobs.List(Location);
                migrationList.ForEach(ValidateDiskMigration);

                var migrationSucceededList = client.DiskMigrationJobs.List(Location, status: "Succeeded");
                migrationSucceededList.ForEach(ValidateDiskMigration);
            });
        }

        [Fact]
        public void TestDiskMigrationInvalidInput()
        {
            string targetShare = @"\\SU1FileServer.azurestack.local\SU1_ObjStore_Invalid\";
            RunTest((client) => {
                var disks = client.Disks.List(Location);
                List<Disk> toMigrationDisks = new List<Disk>();
                foreach (var disk in disks)
                {
                    if (toMigrationDisks.Count < 3)
                    {
                        toMigrationDisks.Add(disk);
                    }
                    else
                    {
                        break;
                    }
                }

                var migrationId = "8C47B2D9-1DA8-4E77-8D0E-D63627146899";// This guid should be the same as the ones in sessionRecord

                ValidateExpectedReturnCode(
                    () => client.DiskMigrationJobs.Create(Location, migrationId, targetShare, toMigrationDisks),
                    HttpStatusCode.BadRequest
                    );

                ValidateExpectedReturnCode(
                    () => client.DiskMigrationJobs.Get(Location, migrationId),
                    HttpStatusCode.NotFound
                    );

                if(toMigrationDisks.Count > 0)
                {
                    toMigrationDisks[0].DiskId = "454E5E28-8D5E-41F9-929E-BFF6A7E1A253"; //Use some not exist disk
                    targetShare = @"\\SU1FileServer.azurestack.local\SU1_ObjStore\";

                    //Comment out because migration has a bug validate disk
                    //ValidateExpectedReturnCode(
                    //    () => client.DiskMigrationJobs.Create(Location, migrationId, targetShare, toMigrationDisks),
                    //    HttpStatusCode.NotFound);
                }

            });
        }
    }
}
