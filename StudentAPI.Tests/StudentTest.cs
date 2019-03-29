using Newtonsoft.Json;
using StudentExerciseFive.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentAPI.Tests
{
    public class StudentTest
    {
        [Fact]
        //Get all students
        public async Task Get_student()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*============================
                 * Arrange
                 ========================*/
                var response = await client.GetAsync("api/student");

                string responseBody = await response.Content.ReadAsStringAsync();
                var StudentList = JsonConvert.DeserializeObject<List<Student>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(StudentList.Count > 0);

            }
        }
        [Fact]
        public async Task Get_specific_student()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("api/student/1");
                string responseBody = await response.Content.ReadAsStringAsync();
                var specificStudent = JsonConvert.DeserializeObject<Student>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hunter", specificStudent.FirstName);
            }
        }

        [Fact]
        public async Task Create_Student()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Construct a new student object to be sent to the API
                Student harvey = new Student
                {
                    FirstName = "Harvey",
                    LastName = "Spaniel",
                    slackHandle = "@loser",
                    cohortName = "Cohort 29",
                    cohortId = 1
                };

                // Serialize the C# object into a JSON string
                var harveyAsJson = JsonConvert.SerializeObject(harvey);


                /*
                    ACT
                */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/student",
                    new StringContent(harveyAsJson, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var newHarvey = JsonConvert.DeserializeObject<Student>(responseBody);


                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Harvey", newHarvey.FirstName);
                Assert.Equal("Spaniel", newHarvey.LastName);
                Assert.Equal("@loser", newHarvey.slackHandle);
            }
        }
        [Fact]
        public async Task Modify_Student()
        {
            using (var client = new APIClientProvider().Client)
            {
                Student modifiedStudent = new Student
                {
                    FirstName = "Hunter",
                    LastName = "Metts",
                    slackHandle = "@SlackSlack",
                    cohortName = "Cohort29",
                    cohortId = 1
                };
                var modifiedStudentAsJSON = JsonConvert.SerializeObject(modifiedStudent);

                var response = await client.PutAsync(
                    "/api/student/1",
                    new StringContent(modifiedStudentAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getUpdateStudent = await client.GetAsync("api/student/1");
                getUpdateStudent.EnsureSuccessStatusCode();

                string getTheNew = await getUpdateStudent.Content.ReadAsStringAsync();
                Student newStudent = JsonConvert.DeserializeObject<Student>(getTheNew);

                Assert.Equal(HttpStatusCode.OK, getUpdateStudent.StatusCode);
                Assert.Equal(getTheNew, newStudent.slackHandle);
            }
        }

    }
}
