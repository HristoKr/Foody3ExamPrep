using Foody3.DTOs;
using Foody3.Tests.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Foody3

{
    public class Foody3Tests
    {
        private RestClient client;
        private static string foodId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("HristoKr1", "1234567");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:81")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);

        }

        private string GetJwtToken(string username, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:81");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }


        [Order(1)]
        [Test]
        public void CreateFood_WithRequiredFields_ShouldSuccess()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "Soup",
                Description = "Soup with chicken and potatoes",
                Url = ""
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);



            foodId = readyResponse.FoodId;

        }

        [Order(2)]
        [Test]
        public void EditFoodTitle_ShouldChangeTitle()
        {
            RestRequest request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);
            request.AddBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Chicken Soup"
                }
            });

            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            //response.Content = {msg: "Successfully edited", ... foodId: "34"}
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //readyResponse
            //Msg = "Successfully edited"
            //FoodId = "34"
            Assert.That(readyResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllFood_ShouldReturnNonEmptyArray()
        {
            RestRequest request = new RestRequest("/api/Food/All", Method.Get);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //response.Content =
            //[
            /* {
                "id": "06edbaaf-3bb4-42e3-8d26-08de7690bc20",
                "name": "Soup",
                "description": "Soup with chicken and potatoes.",
                "url": null
            }]*/
            List<FoodDTO> readyResponse = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));

        }

        [Order(4)]
        [Test]
        public void DeleteExistingFood_ShouldSucceed()
        {
            RestRequest request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //response.Content = { "msg": "Deleted successfully!" }
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //readyResponse
            //Msg = "Deleted successfully!"
            //FoodId = null
            Assert.That(readyResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "",
                Description = ""
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddBody(food);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditTitleOfNonExistingFood_ShouldReturnNotFound()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
            request.AddBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Chicken Soup"
                }
            });
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            //response.Content = { "msg": "No food revues..." }
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //readyResponse
            //Msg = "No food revues..."
            //FoodId = null
            Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingFood_ShouldReturnNotFound()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            //response.Content = { "msg": "No food revues..." }
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //readyResponse
            //Msg = "No food revues..."
            //FoodId = null
            Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));

        }

        [OneTimeTearDown] //¬≈ƒÕ⁄∆ –¿«◊»—“¬¿Ã≈ —À≈ƒ »«œ⁄ÀÕ≈Õ»≈“Œ Õ¿ ¬—»◊ » “≈—“Œ¬≈
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
