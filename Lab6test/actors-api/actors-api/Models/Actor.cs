using System;
using System.ComponentModel.DataAnnotations;

namespace ActorsApi.Models;

public class Actor
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = default!;

    [Required, StringLength(50)]
    public string LastName { get; set; } = default!;

    [Required, DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required, StringLength(50)]
    public string Country { get; set; } = default!;

    [Required, StringLength(1000)]
    public string Biography { get; set; } = default!;
}
