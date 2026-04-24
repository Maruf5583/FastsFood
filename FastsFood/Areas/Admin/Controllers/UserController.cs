using FastsFood.Models;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastsFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();
            var userRoleList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "No Role";
                userRoles[user.Id] = role;

                userRoleList.Add(new UserRoleViewModel
                {
                    User = user,
                    Role = role
                });
            }

            ViewBag.UserRoles = userRoles;
            return View(userRoleList);
        }

        // GET: Admin/User/Create
        public async Task<IActionResult> Create()
        {
            await EnsureRolesExist();
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // POST: Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVm vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = vm.Email,
                    Email = vm.Email,
                    Name = vm.Name,
                    City = vm.City,
                    Address = vm.Address,
                    PostalCode = vm.PostalCode,
                    EmailConfirmed = true,
                    
                };

                var result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(vm.Role))
                    {
                        await _userManager.AddToRoleAsync(user, vm.Role);
                    }
                    else
                    {
                        // Assign default role
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    TempData["Success"] = $"User {user.Email} created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await EnsureRolesExist();
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", vm.Role);
            return View(vm);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();

            var vm = new UserEditVm
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                City = user.City,
                Address = user.Address,
                PostalCode = user.PostalCode,
                Role = userRole
            };

            await EnsureRolesExist();
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", userRole);
            return View(vm);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditVm vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.Name = vm.Name;
                user.City = vm.City;
                user.Address = vm.Address;
                user.PostalCode = vm.PostalCode;

                // Only update email if changed
                if (user.Email != vm.Email)
                {
                    user.Email = vm.Email;
                    user.UserName = vm.Email;
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update role if changed
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var currentRole = currentRoles.FirstOrDefault();

                    if (currentRole != vm.Role)
                    {
                        if (!string.IsNullOrEmpty(currentRole))
                        {
                            await _userManager.RemoveFromRoleAsync(user, currentRole);
                        }
                        if (!string.IsNullOrEmpty(vm.Role))
                        {
                            await _userManager.AddToRoleAsync(user, vm.Role);
                        }
                    }

                    TempData["Success"] = $"User {user.Email} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await EnsureRolesExist();
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", vm.Role);
            return View(vm);
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault();

            return View(user);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id == user.Id)
                {
                    TempData["Error"] = "You cannot delete your own account!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"User {user.Email} deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user!";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/User/ResetPassword/5
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var vm = new ResetPasswordVm
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };

            return View(vm);
        }

        // POST: Admin/User/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Password reset for {user.Email} successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(vm);
        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault();

            return View(user);
        }

        private async Task EnsureRolesExist()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
        }
    }

    // ViewModel for User Index
    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
    }
}