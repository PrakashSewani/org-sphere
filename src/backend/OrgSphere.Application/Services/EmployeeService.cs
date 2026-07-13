using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class EmployeeService(
    IEmployeeRepository repository,
    IEmployeeDocumentRepository documentRepository,
    IEventBus eventBus,
    ITenantContext tenantContext) : IEmployeeService
{
    private readonly IEmployeeRepository _repository = repository;
    private readonly IEmployeeDocumentRepository _documentRepository = documentRepository;
    private readonly IEventBus _eventBus = eventBus;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var existing = await _repository.GetByEmailAsync(request.Email, tenantId, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Employee with email '{request.Email}' already exists");

        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId,
            ManagerId = request.ManagerId,
            OfficeId = request.OfficeId,
            EmploymentType = request.EmploymentType,
            StartDate = request.StartDate,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity, ct);

        await _eventBus.PublishAsync(
            new EmployeeCreatedEvent(tenantId, created.Id, created.Email), ct);

        return MapToDto(created);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<EmployeeDto?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByEmailAsync(email, tenantId, ct);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _repository.GetAllAsync(tenantId, ct);
        return [.. entities.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _repository.GetByDepartmentAsync(departmentId, tenantId, ct);
        return [.. entities.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetByManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _repository.GetByManagerAsync(managerId, tenantId, ct);
        return [.. entities.Select(MapToDto)];
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found");

        entity.Phone = request.Phone;
        entity.Title = request.Title;
        entity.DepartmentId = request.DepartmentId;
        entity.TeamId = request.TeamId;
        entity.ManagerId = request.ManagerId;
        entity.OfficeId = request.OfficeId;
        entity.EmploymentType = request.EmploymentType;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);

        await _eventBus.PublishAsync(
            new EmployeeUpdatedEvent(tenantId, entity.Id), ct);

        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found");

        await _repository.DeleteAsync(id, tenantId, ct);

        await _eventBus.PublishAsync(
            new EmployeeDeletedEvent(tenantId, entity.Id), ct);
    }

    public async Task<EmployeeDto> UpdateProfileAsync(Guid employeeId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByIdAsync(employeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        if (request.Bio is not null)
            entity.Bio = request.Bio;
        if (request.Phone is not null)
            entity.Phone = request.Phone;
        if (request.Skills is not null)
            entity.Skills = request.Skills;

        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);

        await _eventBus.PublishAsync(
            new EmployeeUpdatedEvent(tenantId, entity.Id), ct);

        return MapToDto(entity);
    }

    public async Task<EmployeeDto> UpdatePreferencesAsync(Guid employeeId, UpdatePreferencesRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _repository.GetByIdAsync(employeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        if (request.EmailNotifications.HasValue)
            entity.Preferences.EmailNotifications = request.EmailNotifications.Value;
        if (request.PushNotifications.HasValue)
            entity.Preferences.PushNotifications = request.PushNotifications.Value;
        if (request.ContactEmail is not null)
            entity.Preferences.ContactEmail = request.ContactEmail;
        if (request.ContactPhone is not null)
            entity.Preferences.ContactPhone = request.ContactPhone;

        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);

        return MapToDto(entity);
    }

    public async Task<EmployeeDocumentDto> UploadDocumentAsync(
        Guid employeeId,
        Stream fileStream,
        string fileName,
        string contentType,
        string? description,
        CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        _ = await _repository.GetByIdAsync(employeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        var storagePath = Path.Combine("uploads", tenantId.Value.ToString(), employeeId.ToString(), $"{Guid.NewGuid()}_{fileName}");

        var directory = Path.GetDirectoryName(storagePath)!;
        Directory.CreateDirectory(directory);

        await using var fileStreamOutput = File.Create(storagePath);
        await fileStream.CopyToAsync(fileStreamOutput, ct);

        var document = new EmployeeDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileStreamOutput.Length,
            StoragePath = storagePath,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _documentRepository.CreateAsync(document, ct);

        return MapToDocumentDto(created);
    }

    public async Task<IReadOnlyList<EmployeeDocumentDto>> GetDocumentsAsync(Guid employeeId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var documents = await _documentRepository.GetByEmployeeAsync(employeeId, tenantId, ct);
        return [.. documents.Select(MapToDocumentDto)];
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var document = await _documentRepository.GetByIdAsync(documentId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Document {documentId} not found");

        if (File.Exists(document.StoragePath))
            File.Delete(document.StoragePath);

        await _documentRepository.DeleteAsync(documentId, tenantId, ct);
    }

    private static EmployeeDto MapToDto(Employee entity) => new()
    {
        Id = entity.Id,
        EmployeeId = entity.EmployeeId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        Phone = entity.Phone,
        Title = entity.Title,
        DepartmentId = entity.DepartmentId,
        TeamId = entity.TeamId,
        ManagerId = entity.ManagerId,
        OfficeId = entity.OfficeId,
        EmploymentType = entity.EmploymentType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        Bio = entity.Bio,
        ProfilePictureUrl = entity.ProfilePictureUrl,
        Skills = entity.Skills,
        Preferences = new EmployeePreferencesDto
        {
            EmailNotifications = entity.Preferences.EmailNotifications,
            PushNotifications = entity.Preferences.PushNotifications,
            ContactEmail = entity.Preferences.ContactEmail,
            ContactPhone = entity.Preferences.ContactPhone
        },
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static EmployeeDocumentDto MapToDocumentDto(EmployeeDocument doc) => new()
    {
        Id = doc.Id,
        EmployeeId = doc.EmployeeId,
        FileName = doc.FileName,
        ContentType = doc.ContentType,
        FileSize = doc.FileSize,
        Description = doc.Description,
        CreatedAt = doc.CreatedAt
    };
}
