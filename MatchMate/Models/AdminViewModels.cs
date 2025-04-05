
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MatchMate.Models
{
    public class AdminDashboardViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public List<Game> Games { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveMatches { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime LastLogin { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public bool IsSelected { get; set; }
    }

    public class UserStatsViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public List<GameStatsViewModel> GameStats { get; set; }
    }

    public class AdminChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}