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
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
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
        // GET: api/Student
        // GET api/students
        [HttpGet]
        public IEnumerable<Student> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"select s.id as StudentId,
                                               s.FirstName,
                                               s.LastName,
                                               s.SlackHandle,
                                               s.CohortId,
                                               c.[Name] as CohortName,
                                               e.id as ExerciseId,
                                               e.ExerciseName,
                                               e.ExerciseLanguage   
                                          from students s
                                               left join Cohort c on s.CohortId = c.id
                                               left join StudentExercise se on s.id = se.studentid
                                               left join Exercise e on se.exerciseid = e.id
                                         WHERE 1 = 1";
                    }
                    else
                    {
                        cmd.CommandText = @"select s.id as StudentId,
                                               s.FirstName,
                                               s.LastName,
                                               s.SlackHandle,
                                               s.CohortId,
                                               c.[Name] as CohortName
                                          from students s
                                               left join Cohort c on s.CohortId = c.id
                                         WHERE 1 = 1";
                    }

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" AND 
                                             (s.FirstName LIKE @q OR
                                              s.LastName LIKE @q OR
                                              s.SlackHandle LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Student> students = new Dictionary<int, Student>();
                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        if (!students.ContainsKey(studentId))
                        {
                            Student newStudent = new Student
                            {
                                Id = studentId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                slackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                cohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                cohortName = reader.GetString(reader.GetOrdinal("cohortName")),
                                cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    cohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };

                            students.Add(studentId, newStudent);
                        }

                        if (include == "exercise")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Student currentStudent = students[studentId];
                                currentStudent.Exercises.Add(
                                    new Exercises
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                        Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                                    }
                                );
                            }
                        }
                    }

                    reader.Close();

                    return students.Values.ToList();
                }
            }
        }


        // GET: api/Student/5
        [HttpGet("{id}", Name = "GetStudent")]
        public Student Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, s.firstName, s.lastName,
                                               s.slackHandle, s.cohortId, c.[name] as cohortName
                                        FROM Students s INNER JOIN Cohort c ON s.cohortId = c.id
                                        WHERE s.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            slackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
                            cohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                            cohortName = reader.GetString(reader.GetOrdinal("cohortName")),
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                cohortName = reader.GetString(reader.GetOrdinal("cohortName"))
                            }
                        };
                    }
                    reader.Close();
                    return student;
                }
            }
        }
        // POST: api/Student
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student person)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO students (FirstName, LastName, SlackHandle, cohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", person.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", person.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", person.slackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", person.cohortId));
                    

                    int newId = (int)cmd.ExecuteScalar();
                    person.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, person);

                }
            }
        }

        // PUT: api/Student/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Students
                                            SET FirstName = @FirstName,
                                                LastName = @LastName,
                                                slackHandle = @slackHandle,
                                                cohortId = @cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.slackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.cohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", student.Id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Students WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //If Student exists returns true or false
        private bool StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, firstname, lastname, slackHandle, cohortId
                        FROM students
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
