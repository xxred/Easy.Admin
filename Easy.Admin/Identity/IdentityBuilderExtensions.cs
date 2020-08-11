using System;
using System.Reflection;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Extensions;
using Easy.Admin.Identity;
using Easy.Admin.Identity.IAM;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XCode.Membership;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddAdminIdentity<TApplicationUser, TApplicationRole>(this IServiceCollection services, Action<IdentityOptions> setupAction = null)
            where TApplicationUser : User<TApplicationUser>, new()
            where TApplicationRole : Role<TApplicationRole>, new()
        {
            //services.AddIAMService<TApplicationUser>(options =>
            //{
            //    options.Url = "http://127.0.0.1:44337";
            //});

            var builder = services.AddIdentityCore<TApplicationUser>(setupAction).AddRoles<TApplicationRole>().AddStores()
                .AddUserManager()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services.AddUserServiceCollection<TApplicationUser>();

            return builder;
        }

        public static IdentityBuilder AddStores(this IdentityBuilder builder)
        {
            AddStores(builder.Services, builder.UserType, builder.RoleType);
            return builder;
        }

        private static IdentityBuilder AddUserManager(this IdentityBuilder builder)
        {
            builder.Services.AddScoped(typeof(UserManager<>).MakeGenericType(builder.UserType),
                typeof(ApplicationUserManager<>).MakeGenericType(builder.UserType));

            return builder;
        }

        private static void AddStores(IServiceCollection services, Type userType, Type roleType)
        {
            if (FindGenericBaseType(userType, typeof(User<>)) == null)
            {
                throw new InvalidOperationException("只能使用从User<TEntity>派生的用户调用AddStores");
            }
            if (roleType != null)
            {
                if (FindGenericBaseType(roleType, typeof(Role<>)) == null)
                {
                    throw new InvalidOperationException("只能使用从Role<TEntity>派生的角色调用AddStores");
                }
                Type implementationType = typeof(UserStore<>).MakeGenericType(userType);
                Type implementationType2 = typeof(RoleStore<>).MakeGenericType(roleType);
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), implementationType);
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), implementationType2);
            }
            else
            {
                Type implementationType3 = typeof(UserStore<>).MakeGenericType(userType);
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), implementationType3);
            }
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            Type type = currentType;
            while (type != null)
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                Type left = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (left != null && left == genericBaseType)
                {
                    return typeInfo;
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}
