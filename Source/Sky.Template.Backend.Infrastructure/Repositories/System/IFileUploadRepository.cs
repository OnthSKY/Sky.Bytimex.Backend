using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories.System;

public interface IFileUploadRepository
{
    Task<IEnumerable<FileUploadEntity>> GetAllAsync();
    Task<FileUploadEntity?> GetByIdAsync(Guid id);
    Task<FileUploadEntity> CreateAsync(FileUploadEntity entity);
    Task<FileUploadEntity> UpdateAsync(FileUploadEntity entity);
    Task<bool> DeleteAsync(Guid id);
}

public class FileUploadRepository : IFileUploadRepository
{
    private const string Table = "sys.file_uploads";

    public async Task<IEnumerable<FileUploadEntity>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {Table} WHERE status <> 'DELETED'";
        return await DbManager.ReadAsync<FileUploadEntity>(sql, new Dictionary<string, object>(), GlobalSchema.Name);
    }

    public async Task<FileUploadEntity?> GetByIdAsync(Guid id)
    {
        var sql = $"SELECT * FROM {Table} WHERE id = @id AND status <> 'DELETED'";
        var result = await DbManager.ReadAsync<FileUploadEntity>(sql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<FileUploadEntity> CreateAsync(FileUploadEntity entity)
    {
        var sql = $"INSERT INTO {Table} (id, file_name, file_extension, file_size, file_url, file_type, context, uploaded_by, uploaded_at, status, created_at, created_by) " +
                  "VALUES (@id, @file_name, @file_extension, @file_size, @file_url, @file_type, @context, @uploaded_by, @uploaded_at, @status, @created_at, @created_by) RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.Id},
            {"@file_name", entity.FileName},
            {"@file_extension", entity.FileExtension},
            {"@file_size", entity.FileSize},
            {"@file_url", entity.FileUrl},
            {"@file_type", entity.FileType},
            {"@context", entity.Context},
            {"@uploaded_by", entity.UploadedBy},
            {"@uploaded_at", entity.UploadedAt},
            {"@status", entity.Status},
            {"@created_at", entity.CreatedAt},
            {"@created_by", entity.CreatedBy}
        };
        var result = await DbManager.ReadAsync<FileUploadEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<FileUploadEntity> UpdateAsync(FileUploadEntity entity)
    {
        var sql = $"UPDATE {Table} SET file_name=@file_name, file_extension=@file_extension, file_size=@file_size, file_url=@file_url, file_type=@file_type, context=@context, status=@status, updated_at=@updated_at, updated_by=@updated_by WHERE id=@id RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.Id},
            {"@file_name", entity.FileName},
            {"@file_extension", entity.FileExtension},
            {"@file_size", entity.FileSize},
            {"@file_url", entity.FileUrl},
            {"@file_type", entity.FileType},
            {"@context", entity.Context},
            {"@status", entity.Status},
            {"@updated_at", entity.UpdatedAt},
            {"@updated_by", entity.UpdatedBy}
        };
        var result = await DbManager.ReadAsync<FileUploadEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = $"UPDATE {Table} SET status='DELETED' WHERE id=@id";
        return await DbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name);
    }
}

