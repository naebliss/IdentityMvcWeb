using System.Collections.Generic;
using System.Linq;
using IdentityMvcWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Logging;
using Owin;

namespace IdentityMvcWeb.IdentityConfig
{
    public class RbacConfig
    {
        public static void CreateRolesandUsers(IAppBuilder app)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ILogger logger = app.CreateLogger<RbacConfig>();

            var seededRoles = new List<string>();
            SeedRole(roleManager, logger, seededRoles, UserRoles.Admin);
            SeedRole(roleManager, logger, seededRoles, UserRoles.AddUserToRole);
            SeedRole(roleManager, logger, seededRoles, UserRoles.DeleteUserFromRole);
            SeedRole(roleManager, logger, seededRoles, UserRoles.DeleteUser);

            SeedOrUpdateUser(userManager, logger, "admin@identitymvcweb.com", "1qaz!QAZ", seededRoles.ToArray());
        }

        private static void SeedRole(RoleManager<IdentityRole> roleManager, ILogger logger, List<string> seededRoles, string roleName)
        {
            if (roleManager.RoleExists(roleName))
                return;

            var role = new IdentityRole { Name = roleName };
            var roleCreatedResult = roleManager.Create(role);
            if (!roleCreatedResult.Succeeded)
                logger.WriteError($"{roleName} role could not be created: {roleCreatedResult.Errors.Aggregate((a, b) => $"{a}; {b}")}");
            else
                seededRoles.Add(roleName);
        }

        private static void SeedOrUpdateUser(UserManager<ApplicationUser> userManager, ILogger logger, string name, string userPwd, params string[] roles)
        {
            ApplicationUser user = userManager.FindByName(name);
            if (user  != null)
            {
                AddUserToRoles(userManager, logger, name, roles, user);
                return;
            }

            user = new ApplicationUser
            {
                UserName = name,
                Email = name,
                EmailConfirmed = true
            };

            var userCreatedResult = userManager.Create(user, userPwd);
            if (!userCreatedResult.Succeeded)
                return;

            AddUserToRoles(userManager, logger, name, roles, user);
        }

        private static void AddUserToRoles(UserManager<ApplicationUser> userManager, ILogger logger, string name, string[] roles,
            ApplicationUser user)
        {
            var userAddedToRoles = userManager.AddToRoles(user.Id, roles);
            if (userAddedToRoles.Succeeded)
                return;

            string rolesText = roles.Aggregate((a, b) => $"{a},{b}");
            string errorText = userAddedToRoles.Errors.Aggregate((a, b) => $"{a}; {b}");
            logger.WriteError($"User {name} could not be added to roles {rolesText}: {errorText}");
        }
    }
}