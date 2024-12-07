using System;
using System.IO;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly EMRNextDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _storageBasePath;

        public DocumentService(EMRNextDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _storageBasePath = _configuration["DocumentStorage:BasePath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents");
        }

        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, string patientId)
        {
            try
            {
                // Create document record
                var document = new Document
                {
                    FileName = fileName,
                    ContentType = contentType,
                    PatientId = patientId,
                    UploadDate = DateTime.UtcNow,
                    Status = "Active"
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Create storage path
                var storagePath = Path.Combine(_storageBasePath, patientId, document.Id.ToString());
                Directory.CreateDirectory(Path.GetDirectoryName(storagePath));

                // Save file
                using (var fileStream2 = new FileStream(storagePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStream2);
                }

                return document.Id.ToString();
            }
            catch (Exception ex)
            {
                // Log error and rethrow
                throw new Exception("Error uploading document", ex);
            }
        }

        public async Task<Stream> GetDocumentAsync(string documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new FileNotFoundException("Document not found");

            var filePath = Path.Combine(_storageBasePath, document.PatientId, documentId);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Document file not found");

            return File.OpenRead(filePath);
        }

        public async Task DeleteDocumentAsync(string documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new FileNotFoundException("Document not found");

            // Soft delete in database
            document.Status = "Deleted";
            document.DeletedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Optionally move file to archive location
            var sourcePath = Path.Combine(_storageBasePath, document.PatientId, documentId);
            var archivePath = Path.Combine(_storageBasePath, "Archive", document.PatientId, documentId);

            if (File.Exists(sourcePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(archivePath));
                File.Move(sourcePath, archivePath);
            }
        }

        public async Task<DocumentMetadata> GetDocumentMetadataAsync(string documentId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id.ToString() == documentId);

            if (document == null)
                throw new FileNotFoundException("Document not found");

            return new DocumentMetadata
            {
                Id = document.Id.ToString(),
                FileName = document.FileName,
                ContentType = document.ContentType,
                PatientId = document.PatientId,
                UploadDate = document.UploadDate,
                Status = document.Status
            };
        }

        public async Task UpdateDocumentMetadataAsync(string documentId, DocumentMetadata metadata)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id.ToString() == documentId);

            if (document == null)
                throw new FileNotFoundException("Document not found");

            document.FileName = metadata.FileName;
            document.ContentType = metadata.ContentType;
            document.Status = metadata.Status;

            await _context.SaveChangesAsync();
        }
    }

    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string PatientId { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }
        public DateTime? DeletedDate { get; set; }
    }

    public class DocumentMetadata
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string PatientId { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }
    }
}
