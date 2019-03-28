using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseFive.Models
{
    public class Cohort
    {
        public int Id { get; set; }

        [Required]
        [StringLength( 11, MinimumLength=5)]
        public string cohortName { get; set; }
        public List<Student> studentsInCohort { get; set; } = new List<Student>();
        public List<Instructors> instructorList { get; set; } = new List<Instructors>();
    }
}
