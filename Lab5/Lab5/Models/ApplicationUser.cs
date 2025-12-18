using Microsoft.AspNetCore.Identity;
using System;

namespace Lab5.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsOnline { get; set; }
        public DateTime? LastActiveUtc { get; set; }
    }
}
