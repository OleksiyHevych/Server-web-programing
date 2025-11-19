using Microsoft.AspNetCore.Identity;
using System;

namespace Lab1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Показник онлайн (синхронізується при підключенні/відключенні)
        public bool IsOnline { get; set; }

        // Останній час активності (UTC)
        public DateTime? LastActiveUtc { get; set; }
    }
}
