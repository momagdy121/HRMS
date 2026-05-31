using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.UserAccounts;

namespace HRSystem.Business.Interfaces.Services;

public interface IUserAccountService
{
    Task<PagedResult<UserAccountListItemDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
