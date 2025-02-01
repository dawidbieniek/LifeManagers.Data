using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using LifeManagers.Data.Seeding;

namespace LifeManagers.Data.Tests
{
    [TestClass]
    public sealed class DataServicesOptionsTests
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Builder_ThrowsException_DataDirectoryPathIsInvalid(string? path)
        {
            DataServicesOptionsBuilder builder = new();

            if (path is not null)
                builder.WithDataDirectoryPath(path);

            Assert.ThrowsException<ValidationException>(() => builder.Build());
        }

        [TestMethod]
        public void Builder_ReturnsCorrectDataDirectoryPath_DataDirectoryPathIsSupplied()
        {
            string path = "somepath";
            DataServicesOptionsBuilder builder = new();

            builder.WithDataDirectoryPath(path);

            DataServicesOptions options = builder.Build();
            Assert.AreEqual(path, options.DataDirectoryPath);
        }

        [TestMethod]
        public void BuilderWithSeeder_CorrectyAddsSeederTypeToOptions()
        {
            DataServicesOptionsBuilder builder = new();
            builder.WithDataDirectoryPath("somepath");

            builder.WithSeeder<AppDbContextBase, TestSeeder>();
            DataServicesOptions options = builder.Build();

            Assert.AreEqual(typeof(TestSeeder), options.SeederType);
        }

        [TestMethod]
        public void CreateOptions_CopiesAllProperties()
        {
            DataServicesOptions target = new();
            DataServicesOptions source = new()
            {
                BackupDirectory = "some backup directory",
                BackupPeriod = TimeSpan.FromDays(365),
                DatabaseFileName = "some database name",
                DataDirectoryPath = "path that doesn't exist",
                DebugMode = true,
                LastBackupTimeFileName = "some filename",
                SeederType = typeof(TestSeeder)
            };

            target.CreateOptions(source);

            source.Should().BeEquivalentTo(target);
        }

        private class TestSeeder : ISeeder<AppDbContextBase>
        {
            public bool SeedDebugDataCalled { get; private set; } = false;
            public bool SeedRequiredDataCalled { get; private set; } = false;

            public Task SeedDebugDataAsync()
            {
                SeedDebugDataCalled = true;
                return Task.CompletedTask;
            }

            public Task SeedRequiredDataAsync()
            {
                SeedRequiredDataCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}