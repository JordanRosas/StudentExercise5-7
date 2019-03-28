using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseFive.Models
{
    public class Exercises
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Language { get; set; }
    }
}
