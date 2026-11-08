using Gatherly.Core.Services;
using Gatherly.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Gatherly.Core.Models;
using FluentValidation;
using System.Reflection;

namespace Gatherly.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services)
        {
            services.AddScoped<IEventAccessService, EventAccessService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<IExpenseShareService, ExpenseShareService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IItemClaimService, ItemClaimService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IContributionService, ContributionService>();

            services.AddValidatorsFromAssembly(
                Assembly.GetExecutingAssembly());

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }
    }
}