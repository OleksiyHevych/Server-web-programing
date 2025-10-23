using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва фільму є обов’язковою")]
        [StringLength(100, ErrorMessage = "Назва не може бути довшою за 100 символів")]
        [Display(Name = "Назва фільму")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Жанр є обов’язковим")]
        [StringLength(50, ErrorMessage = "Жанр не може бути довшим за 50 символів")]
        [Display(Name = "Жанр")]
        public string? Genre { get; set; }

        [Required(ErrorMessage = "Дата виходу є обов’язковою")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата виходу")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Тривалість фільму є обов’язковою")]
        [Range(1, 600, ErrorMessage = "Тривалість повинна бути від 1 до 600 хвилин")]
        [Display(Name = "Тривалість (хвилини)")]
        public int DurationMinutes { get; set; }

        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        [Display(Name = "Опис фільму")]
        public string? Description { get; set; }

        // many-to-many
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
