using Dapper;
using System.Data;
using Student_Onboarding_Platform.Data;
using Student_Onboarding_Platform.Models.DTOs;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Student_Onboarding_Platform.Services.Implementations;

public class EnquiryService : IEnquiryService
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly ILogger<EnquiryService> _logger;

    public EnquiryService(DbConnectionFactory connectionFactory, ILogger<EnquiryService> logger)
    {
        _connectionFactory = connectionFactory ?? throw new InvalidOperationException("Database connection factory is missing.");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

    public async Task<ApiResponse<EnquiryDto>> CreateEnquiryAsync(EnquiryRequestDto requestDto)
    {
        try
        {
            var enquiry = new Enquiry
            {
                Id = Guid.NewGuid(),
                Name = requestDto.Name,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                Message = requestDto.Message,
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Enquiries (Id, Name, Email, PhoneNumber, Message, Status, CreatedAt)
                VALUES (@Id, @Name, @Email, @PhoneNumber, @Message, @Status, @CreatedAt)";

            await connection.ExecuteAsync(sql, enquiry);

            var dto = new EnquiryDto
            {
                Id = enquiry.Id,
                Name = enquiry.Name,
                Email = enquiry.Email,
                PhoneNumber = enquiry.PhoneNumber,
                Message = enquiry.Message,
                Status = enquiry.Status,
                CreatedAt = enquiry.CreatedAt
            };

            return ApiResponse<EnquiryDto>.Ok(dto, "Enquiry submitted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enquiry");
            return ApiResponse<EnquiryDto>.Fail("An error occurred while submitting your enquiry.");
        }
    }

    public async Task<ApiResponse<IEnumerable<EnquiryDto>>> GetAllEnquiriesAsync()
    {
        try
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Enquiries ORDER BY CreatedAt DESC";
            var enquiries = await connection.QueryAsync<Enquiry>(sql);

            var dtos = enquiries.Select(e => new EnquiryDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                Message = e.Message,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                ResolvedAt = e.ResolvedAt
            });

            return ApiResponse<IEnumerable<EnquiryDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching enquiries");
            return ApiResponse<IEnumerable<EnquiryDto>>.Fail("An error occurred while fetching enquiries.");
        }
    }

    public async Task<ApiResponse<bool>> ResolveEnquiryAsync(Guid id)
    {
        try
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE Enquiries 
                SET Status = 'Resolved', ResolvedAt = @ResolvedAt 
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, ResolvedAt = DateTime.UtcNow });

            if (rowsAffected == 0)
                return ApiResponse<bool>.Fail("Enquiry not found.");

            return ApiResponse<bool>.Ok(true, "Enquiry resolved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving enquiry {EnquiryId}", id);
            return ApiResponse<bool>.Fail("An error occurred while resolving the enquiry.");
        }
    }
}
