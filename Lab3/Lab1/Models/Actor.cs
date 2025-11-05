using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab1.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ім’я є обов’язковим")]
        [StringLength(50, ErrorMessage = "Ім’я не може бути довше за 50 символів")]
        [Display(Name = "Ім’я")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Прізвище є обов’язковим")]
        [StringLength(50, ErrorMessage = "Прізвище не може бути довше за 50 символів")]
        [Display(Name = "Прізвище")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Дата народження є обов’язковою")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата народження")]
        public DateTime BirthDate { get; set; }
        
        [Required(ErrorMessage = "Актор не може бути без країни")]
        [StringLength(50, ErrorMessage = "Назва країни не може перевищувати 50 символів")]
        [Display(Name = "Країна")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Біографія актора обов'язкова")]
        [StringLength(1000, ErrorMessage = "Біографія не може перевищувати 1000 символів")]
        [Display(Name = "Біографія")]
        public string? Biography { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

        public string Name => $"{FirstName} {LastName}";
    }
}
