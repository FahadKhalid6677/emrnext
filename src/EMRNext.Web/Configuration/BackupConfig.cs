using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EMRNext.Web.Configuration
{
    public static class BackupConfig
    {
        public static IServiceCollection AddBackupConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IBackupService, BackupService>();
            services.AddHostedService<AutomaticBackupService>();
            
            return services;
        }
    }

    public interface IBackupService
    {
        Task<BackupResult> CreateBackupAsync(BackupType type);
        Task<RestoreResult> RestoreFromBackupAsync(string backupId);
        Task<IEnumerable<BackupInfo>> ListBackupsAsync();
        Task<bool> DeleteBackupAsync(string backupId);
    }

    public class BackupService : IBackupService
    {
        private readonly EMRNextContext _dbContext;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BackupService> _logger;
        private const string ContainerName = "emrnext-backups";

        public BackupService(
            EMRNextContext dbContext,
            BlobServiceClient blobServiceClient,
            ILogger<BackupService> logger)
        {
            _dbContext = dbContext;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<BackupResult> CreateBackupAsync(BackupType type)
        {
            try
            {
                var backupId = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{type}";
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                await containerClient.CreateIfNotExistsAsync();

                switch (type)
                {
                    case BackupType.Database:
                        await BackupDatabaseAsync(containerClient, backupId);
                        break;
                    case BackupType.Files:
                        await BackupFilesAsync(containerClient, backupId);
                        break;
                    case BackupType.Full:
                        await BackupDatabaseAsync(containerClient, backupId);
                        await BackupFilesAsync(containerClient, backupId);
                        break;
                }

                return new BackupResult
                {
                    BackupId = backupId,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup failed");
                return new BackupResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<RestoreResult> RestoreFromBackupAsync(string backupId)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                
                // Restore database
                if (await containerClient.GetBlobClient($"{backupId}_db.bak").ExistsAsync())
                {
                    await RestoreDatabaseAsync(containerClient, backupId);
                }

                // Restore files
                if (await containerClient.GetBlobClient($"{backupId}_files.zip").ExistsAsync())
                {
                    await RestoreFilesAsync(containerClient, backupId);
                }

                return new RestoreResult
                {
                    Success = true,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Restore failed");
                return new RestoreResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<IEnumerable<BackupInfo>> ListBackupsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var backups = new List<BackupInfo>();

            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                var backupId = blob.Name.Split('_')[0];
                var existing = backups.FirstOrDefault(b => b.BackupId == backupId);
                
                if (existing == null)
                {
                    backups.Add(new BackupInfo
                    {
                        BackupId = backupId,
                        Timestamp = blob.Properties.CreatedOn?.UtcDateTime ?? DateTime.UtcNow,
                        Size = blob.Properties.ContentLength ?? 0,
                        Type = DetermineBackupType(blob.Name)
                    });
                }
            }

            return backups.OrderByDescending(b => b.Timestamp);
        }

        public async Task<bool> DeleteBackupAsync(string backupId)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                
                await containerClient.DeleteBlobIfExistsAsync($"{backupId}_db.bak");
                await containerClient.DeleteBlobIfExistsAsync($"{backupId}_files.zip");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete backup failed");
                return false;
            }
        }

        private async Task BackupDatabaseAsync(BlobContainerClient containerClient, string backupId)
        {
            // Implementation for database backup
            var backupPath = Path.GetTempFileName();
            try
            {
                // Create backup using SQL Server SMO
                await _dbContext.Database.ExecuteSqlRawAsync(
                    $"BACKUP DATABASE EMRNext TO DISK = '{backupPath}'");

                // Upload to blob storage
                await using var stream = File.OpenRead(backupPath);
                await containerClient.UploadBlobAsync(
                    $"{backupId}_db.bak",
                    stream);
            }
            finally
            {
                File.Delete(backupPath);
            }
        }

        private async Task BackupFilesAsync(BlobContainerClient containerClient, string backupId)
        {
            var tempZipPath = Path.GetTempFileName();
            try
            {
                // Create zip file of uploads directory
                ZipFile.CreateFromDirectory(
                    "uploads",
                    tempZipPath,
                    CompressionLevel.Optimal,
                    false);

                // Upload to blob storage
                await using var stream = File.OpenRead(tempZipPath);
                await containerClient.UploadBlobAsync(
                    $"{backupId}_files.zip",
                    stream);
            }
            finally
            {
                File.Delete(tempZipPath);
            }
        }

        private async Task RestoreDatabaseAsync(BlobContainerClient containerClient, string backupId)
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                // Download backup file
                await containerClient.GetBlobClient($"{backupId}_db.bak")
                    .DownloadToAsync(tempPath);

                // Restore database
                await _dbContext.Database.ExecuteSqlRawAsync(
                    $"RESTORE DATABASE EMRNext FROM DISK = '{tempPath}' WITH REPLACE");
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        private async Task RestoreFilesAsync(BlobContainerClient containerClient, string backupId)
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                // Download zip file
                await containerClient.GetBlobClient($"{backupId}_files.zip")
                    .DownloadToAsync(tempPath);

                // Extract files
                ZipFile.ExtractToDirectory(tempPath, "uploads", true);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        private static BackupType DetermineBackupType(string blobName)
        {
            if (blobName.Contains("_db.bak") && blobName.Contains("_files.zip"))
                return BackupType.Full;
            if (blobName.Contains("_db.bak"))
                return BackupType.Database;
            return BackupType.Files;
        }
    }

    public class AutomaticBackupService : BackgroundService
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<AutomaticBackupService> _logger;

        public AutomaticBackupService(
            IBackupService backupService,
            ILogger<AutomaticBackupService> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create full backup daily at 2 AM
                    if (DateTime.UtcNow.Hour == 2)
                    {
                        _logger.LogInformation("Starting automatic backup");
                        await _backupService.CreateBackupAsync(BackupType.Full);
                        _logger.LogInformation("Automatic backup completed");
                    }

                    // Wait for an hour before checking again
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during automatic backup");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }

    public enum BackupType
    {
        Database,
        Files,
        Full
    }

    public class BackupResult
    {
        public string BackupId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RestoreResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class BackupInfo
    {
        public string BackupId { get; set; }
        public DateTime Timestamp { get; set; }
        public long Size { get; set; }
        public BackupType Type { get; set; }
    }
}
