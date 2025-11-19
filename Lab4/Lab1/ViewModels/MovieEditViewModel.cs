using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Lab1.ViewModels
{
    public class MovieEditViewModel
    {
        public int Id { get; set; }                 // Id фільму
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<SelectListItem> AllActors { get; set; } = new();
        public int[] SelectedActorIds { get; set; } = Array.Empty<int>();

        // Для чернеток
        public DateTime DraftCreatedAt { get; set; } = DateTime.UtcNow;
    }
}
