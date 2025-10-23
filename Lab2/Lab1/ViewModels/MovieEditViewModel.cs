using Lab1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab1.ViewModels
{
    public class MovieEditViewModel
    {
        public Movie Movie { get; set; }
        public List<SelectListItem> AllActors { get; set; } = new();
        public int[] SelectedActorIds { get; set; } = Array.Empty<int>();
    }

}
