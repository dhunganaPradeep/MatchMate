// AdminController.cs
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MatchMate.Models;

namespace MatchMate.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;

        public AdminController()
        {
            _context = new ApplicationDbContext();
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            var users = UserManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    LastLogin = user.LastLoginTime ?? DateTime.MinValue,
                    TotalGamesPlayed = _context.Matches.Count(m => m.Player1Id == user.Id || m.Player2Id == user.Id),
                    TotalWins = _context.Matches.Count(m => m.WinnerId == user.Id)
                });
            }

            var model = new AdminDashboardViewModel
            {
                Users = userViewModels,
                Games = _context.Games.ToList(),
                TotalUsers = users.Count,
                ActiveMatches = _context.Matches.Count(m => m.WinnerId == null)
            };
            return View(model);
        }

        // POST: Admin/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkDelete(string[] selectedUsers)
        {
            if (selectedUsers != null && selectedUsers.Any())
            {
                var currentUserId = User.Identity.GetUserId();
                selectedUsers = selectedUsers.Where(id => id != currentUserId).ToArray(); // Prevent self-deletion

                if (selectedUsers.Any())
                {
                    foreach (var userId in selectedUsers)
                    {
                        var user = await UserManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            // Delete related matches first to avoid FK constraint violation
                            var matches = _context.Matches.Where(m => m.Player1Id == userId || m.Player2Id == userId || m.WinnerId == userId);
                            _context.Matches.RemoveRange(matches);
                            await _context.SaveChangesAsync();

                            // Now delete the user
                            await UserManager.DeleteAsync(user);
                        }
                    }
                    TempData["SuccessMessage"] = "Selected users deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot delete your own account.";
                }
            }
            return RedirectToAction("Dashboard");
        }

        // GET: Admin/DeleteUser
        [HttpGet]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var currentUserId = User.Identity.GetUserId();
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Dashboard");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                // Delete related matches first to avoid FK constraint violation
                var matches = _context.Matches.Where(m => m.Player1Id == id || m.Player2Id == id || m.WinnerId == id);
                _context.Matches.RemoveRange(matches);
                await _context.SaveChangesAsync();

                // Now delete the user
                await UserManager.DeleteAsync(user);
                TempData["SuccessMessage"] = $"User {user.Username} deleted successfully.";
            }
            return RedirectToAction("Dashboard");
        }

        // GET: Admin/UserStats
        public ActionResult UserStats(string id)
        {
            var user = UserManager.FindById(id);
            if (user == null) return HttpNotFound();

            var games = _context.Games.ToList();
            var gameStats = games.Select(g => new GameStatsViewModel
            {
                GameName = g.GameName,
                TotalGames = _context.Matches.Count(m => m.GameId == g.GameId && (m.Player1Id == id || m.Player2Id == id)),
                Wins = _context.Matches.Count(m => m.GameId == g.GameId && m.WinnerId == id),
                Losses = _context.Matches.Count(m => m.GameId == g.GameId && (m.Player1Id == id || m.Player2Id == id) && m.WinnerId != id && m.WinnerId != null),
                Ties = _context.Matches.Count(m => m.GameId == g.GameId && (m.Player1Id == id || m.Player2Id == id) && m.WinnerId == null)
            }).ToList();

            var model = new UserStatsViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                GameStats = gameStats
            };
            return View(model);
        }

        // GET: Admin/AddUser
        public ActionResult AddUser()
        {
            return View();
        }

        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "User");
                    TempData["SuccessMessage"] = $"User {user.Username} added successfully.";
                    return RedirectToAction("Dashboard");
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: Admin/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Admin/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(AdminChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Dashboard");
            }
            AddErrors(result);
            return View(model);
        }

        // GET: Admin/ResetUserPassword
        public async Task<ActionResult> ResetUserPassword(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            var token = await UserManager.GeneratePasswordResetTokenAsync(id);
            var result = await UserManager.ResetPasswordAsync(id, token, "NewPassword123!");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Password reset for {user.Username} to 'NewPassword123!'";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reset password.";
            }
            return RedirectToAction("Dashboard");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
                _userManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}