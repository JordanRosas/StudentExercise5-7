using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExerciseFive.Models;

namespace StudentExerciseFive.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: api/Cohort
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select students.id as studentId, 
                                        students.FirstName, 
                                        students.LastName, 
                                        students.SlackHandle, 
                                        cohort.id as cohortId, 
                                        cohort.[name] as cohortName, 
                                        Instructors.id as instructorId,
                                        Instructors.FirstName as instructorFirstName, 
                                        Instructors.LastName as instructorLastName,
                                        Instructors.slackHandle as instructorSlack
                                            from Students 
                                            left join cohort on students.CohortId = cohort.id
                                            left join instructors on cohort.id = Instructors.CohortId";
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();
                    Dictionary<int, Student> students = new Dictionary<int, Student>();
                    Dictionary<int, Instructors> instructors = new Dictionary<int, Instructors>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("cohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort NewCohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                cohortName = reader.GetString(reader.GetOrdinal("cohortName"))
                            };
                            cohorts.Add(cohortId, NewCohort);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("studentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("studentId"));
                            if (!students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student
                                {
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    slackHandle = reader.GetString(reader.GetOrdinal("slackHandle"))
                                };
                                students.Add(studentId, newStudent);

                                Cohort currentCohort = cohorts[cohortId];
                                currentCohort.studentsInCohort.Add(
                                    new Student
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("studentId")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        slackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
                                        cohortName = reader.GetString(reader.GetOrdinal("cohortName")),
                                        cohortId = reader.GetInt32(reader.GetOrdinal("cohortId"))
                                    }
                                );
                            }
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal(("InstructorId"))))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!instructors.ContainsKey(instructorId))
                            {
                                Instructors newInstructor = new Instructors
                                {
                                    id = instructorId,
                                    FirstName = reader.GetString(reader.GetOrdinal("instructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("instructorLastName")),
                                    slackHandle = reader.GetString(reader.GetOrdinal("instructorSlack")),
                                    CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                                };
                                instructors.Add(instructorId, newInstructor);

                                Cohort currentCohort = cohorts[cohortId];
                                currentCohort.instructorList.Add(
                                    new Instructors
                                    {
                                        id = instructorId,
                                        FirstName = reader.GetString(reader.GetOrdinal("instructorFirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("instructorLastName")),
                                        slackHandle = reader.GetString(reader.GetOrdinal("instructorSlack")),
                                        CohortName = reader.GetString(reader.GetOrdinal("CohortName")),

                                    }
                                );
                            }

                        }
                    }
                        reader.Close();
                        return Ok(cohorts.Values.ToList());
                }
            }
        }

        // GET: api/Cohort/5
        [HttpGet("{id}", Name = "getCohorts")]
        public async Task<IActionResult> Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT id, [name] from cohort where Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort NewCohort = null;

                    if (reader.Read())
                    {
                        NewCohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            cohortName = reader.GetString(reader.GetOrdinal("name"))
                        };
                    }
                    reader.Close();

                    return Ok(NewCohort);
                }
            }
        }

        // POST: api/Cohort
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Cohort/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
