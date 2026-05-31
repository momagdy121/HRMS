using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.UserAccounts;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Mapping;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.Business.Services;

public class UserAccountService : IUserAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<PagedResult<UserAccountListItemDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var pageResult = await _unitOfWork.ApplicationUsers.GetAllWithEmployeePagedAsync(page, pageSize, cancellationToken);
        var items = new List<UserAccountListItemDto>();

        foreach (var row in pageResult.Items)
        {
            var roles = await _userManager.GetRolesAsync(row.User);
            var role = roles.FirstOrDefault() ?? string.Empty;
            items.Add(UserAccountMapper.ToDto(row, role));
        }

        return new PagedResult<UserAccountListItemDto>
        {
            Items = items,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize,
            TotalCount = pageResult.TotalCount
        };
    }
}
