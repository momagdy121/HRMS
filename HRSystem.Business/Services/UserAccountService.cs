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
    private readonly UserManager<Employee> _userManager;

    public UserAccountService(IUnitOfWork unitOfWork, UserManager<Employee> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<PagedResult<UserAccountListItemDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var pageResult = await _unitOfWork.Employees.GetAllPagedAsync(page, pageSize, cancellationToken);
        var userIds = pageResult.Items.Select(e => e.Id).ToList();
        var roleMap = await _unitOfWork.Employees.GetRolesByUserIdsAsync(userIds, cancellationToken);

        var items = pageResult.Items.Select(employee =>
        {
            roleMap.TryGetValue(employee.Id, out var role);
            return UserAccountMapper.ToDto(employee, role ?? string.Empty);
        }).ToList();

        return new PagedResult<UserAccountListItemDto>
        {
            Items = items,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize,
            TotalCount = pageResult.TotalCount
        };
    }
}
