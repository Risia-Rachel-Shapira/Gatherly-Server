using AutoMapper;
using Gatherly.Core.DTOs.Admin;
using Gatherly.Core.DTOs.Common;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AdminService(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminUserDto>> GetUsersAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException(
                    "Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");

            var (users, totalCount) = await _userRepository.GetPagedAsync(
                pageNumber, pageSize, cancellationToken);

            return new PagedResult<AdminUserDto>
            {
                Items = _mapper.Map<List<AdminUserDto>>(users),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
