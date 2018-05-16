using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IdentityMvcWeb.IdentityConfig;
using IdentityMvcWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;

namespace IdentityMvcWeb.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AdministrationController : Controller
    {
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        public AdministrationController()
        {
        }

        public AdministrationController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(int? page, AdminMessageId? message)
        {
            SetStatusMessage(message);

            int pageSize = 10;
            var totalUsers = UserManager.Users.Count();
            int pageNumber = page ?? 1;

            var users = UserManager.Users
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedList = new StaticPagedList<ApplicationUser>(users, pageNumber, pageSize, totalUsers);
            return View(pagedList);
        }
        
        [HttpGet]
        public async Task<ActionResult> EditUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction("Index", new { Message = AdminMessageId.Error });

            ViewBag.RolesList = GetAllRolesOrdered();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.DeleteUser)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                    return RedirectToAction("Index", new { Message = AdminMessageId.Error });

                IdentityResult deleteUserResult = await UserManager.DeleteAsync(user);
                var message = deleteUserResult.Succeeded ? AdminMessageId.DeleteUserSucces : AdminMessageId.Error;

                return RedirectToAction("Index", new { Message = message });
            }
            catch
            {
                return RedirectToAction("Index", new { Message = AdminMessageId.Error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.AddUserToRole)]
        public async Task<ActionResult> AddUserToRole(string id, string role)
        {
            try
            {
                IdentityResult addUserToRoleResult = await UserManager.AddToRoleAsync(id, role);
                var message = addUserToRoleResult.Succeeded ? AdminMessageId.AddUserToRoleSucces : AdminMessageId.Error;

                return RedirectToAction("Index", new { Message = message });
            }
            catch
            {
                return RedirectToAction("Index", new { Message = AdminMessageId.Error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.DeleteUserFromRole)]
        public async Task<ActionResult> DeleteUserFromRole(string id,string role)
        {
            try
            {
                IdentityResult deleteUserFromRoleResult = await UserManager.RemoveFromRolesAsync(id, role);
                var message = deleteUserFromRoleResult.Succeeded ? AdminMessageId.DeleteUserFromRoleSucces : AdminMessageId.Error;

                return RedirectToAction("Index", new { Message = message });
            }
            catch
            {
                return RedirectToAction("Index", new { Message = AdminMessageId.Error });
            }
        }

        public enum AdminMessageId
        {
            None,
            DeleteUserSucces,
            AddUserToRoleSucces,
            DeleteUserFromRoleSucces,
            Error
        }

        private void SetStatusMessage(AdminMessageId? message)
        {
            ViewBag.StatusMessage = "";
            switch (message ?? AdminMessageId.None)
            {
                case AdminMessageId.DeleteUserFromRoleSucces:
                    ViewBag.StatusMessage = "User removed from role";
                    break;
                case AdminMessageId.AddUserToRoleSucces:
                    ViewBag.StatusMessage = "User added to role";
                    break;
                case AdminMessageId.DeleteUserSucces:
                    ViewBag.StatusMessage = "User deleted";
                    break;
                case AdminMessageId.Error:
                    ViewBag.StatusMessage = "There was an error";
                    break;
            }
        }
        
        private IEnumerable<IdentityRole> GetAllRolesOrdered()
        {
            return RoleManager.Roles.OrderBy(x => x.Name).ToList();
        }
    }
}