using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        Task<Invitation?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default);
    }
}