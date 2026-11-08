using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Gatherly.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gatherly.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddData(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventMemberRepository, EventMemberRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IItemClaimRepository, ItemClaimRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IContributionRepository, ContributionRepository>();

            return services;
        }
    }
}