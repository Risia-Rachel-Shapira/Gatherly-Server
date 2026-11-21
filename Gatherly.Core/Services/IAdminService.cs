using Gatherly.Core.DTOs.Admin;
using Gatherly.Core.DTOs.Common;

namespace Gatherly.Core.Services
{
    public interface IAdminService
    {
        Task<PagedResult<AdminUserDto>> GetUsersAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
