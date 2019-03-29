using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

    
namespace StudentExerciseFive.Models
{
    
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string slackHandle { get; set; }

        public string cohortName { get; set; }
        public int cohortId { get; set; }
        

        public Cohort cohort { get; set; } = new Cohort();
        public List<Exercises> Exercises { get; set; } = new List<Exercises>();
    }
}