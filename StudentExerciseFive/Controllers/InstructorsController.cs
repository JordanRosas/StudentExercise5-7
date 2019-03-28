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
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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
        // GET: api/Instructors
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select Instructors.id, Instructors.FirstName, Instructors.LastName, instructors.SlackHandle, cohort.[Name]
                                        from Instructors left join cohort on Instructors.CohortId = cohort.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructors> AllInstructors = new List<Instructors>();
                    while(reader.Read())
                    {
                        Instructors instructor = new Instructors
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            slackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
                            CohortName = reader.GetString(reader.GetOrdinal("Name"))
                        };
                        AllInstructors.Add(instructor);
                    }
                    reader.Close();
                    return Ok(AllInstructors);
                }
            }
        }

        // GET: api/Instructors/5
        //[HttpGet("{id}", Name = "InstructorGet")]
        //public string Get(int id)
        //{ 
        //}

        // POST: api/Instructors
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Instructors/5
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
