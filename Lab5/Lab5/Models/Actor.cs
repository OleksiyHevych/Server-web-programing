using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab5.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string? FirstName { get; set; }

        [Required, StringLength(50)]
        public string? LastName { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required, StringLength(50)]
        public string? Country { get; set; }

        [Required, StringLength(1000)]
        public string? Biography { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
