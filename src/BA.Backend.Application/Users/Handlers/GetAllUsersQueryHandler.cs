using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Users.Queries;
using BA.Backend.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BA.Backend.Application.Users.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResultDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly string _connectionString;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<PagedResultDto<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            // 1. Contar total
            var totalCountSQL = "SELECT COUNT(*) FROM dbo.Users WHERE TenantId = @TenantId";
            var totalCount = await connection.ExecuteScalarAsync<int>(totalCountSQL, new { request.TenantId });

            // 2. Obtener página
            var offset = (request.PageNumber - 1) * request.PageSize;
            var usersSQL = @"
                SELECT 
                    Id, Email, FullName, Role, IsActive, IsLocked, LastLoginAt
                FROM dbo.Users 
                WHERE TenantId = @TenantId
                ORDER BY Email
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var users = await connection.QueryAsync<UserDto>(
                usersSQL,
                new { request.TenantId, Offset = offset, PageSize = request.PageSize }
            );

            // 3. Retornar resultado
            return new PagedResultDto<UserDto>
            {
                Items = users.ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
