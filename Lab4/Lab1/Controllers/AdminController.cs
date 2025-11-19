using Lab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lab1.Controllers
{
    [Authorize(Roles = "Admin")] // доступ лише для адміністраторів
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await Task.FromResult(_userManager.Users.ToList());
            return View(users);
        }

        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = new List<RoleCheckbox>();
            foreach (var role in _roleManager.Roles)
            {
                var isInRole = await _userManager.IsInRoleAsync(user, role.Name!);
                roles.Add(new RoleCheckbox
                {
                    RoleName = role.Name!,
                    Selected = isInRole
                });
            }

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoles(EditRolesViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            var selectedRoles = vm.Roles.Where(r => r.Selected).Select(r => r.RoleName);
            await _userManager.AddToRolesAsync(user, selectedRoles);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class EditRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<RoleCheckbox> Roles { get; set; } = new();
    }

    public class RoleCheckbox
    {
        public string RoleName { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
