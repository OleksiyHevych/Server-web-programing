using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab5.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Title { get; set; }

        [Required, StringLength(50)]
        public string? Genre { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Required, Range(1, 600)]
        public int DurationMinutes { get; set; }

        [Required, StringLength(500)]
        public string? Description { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
