using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using FluentAssertions;
using Xunit;

namespace Challenger.Tests.Integration
{
    public class CouriersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public CouriersIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_And_UploadCnh_Png_Should_Work()
        {
            var client = _factory.CreateClient();

            var req = new CreateCourierRequest
            {
                Name = "Nicholas Balbino",
                Email = "nicholas@example.com",
                Password = "Senha@123",
                CnhNumber = "1234567890",
                CnhType = "A",
                Cnpj = "12345678000199",
                BirthDate = new DateTime(1990, 1, 15)
            };

            var createResp = await client.PostAsJsonAsync("/couriers", req);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var courier = await createResp.Content.ReadFromJsonAsync<CourierResponse>();
            courier.Should().NotBeNull();

            var content = new MultipartFormDataContent();

            var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=");
            var fileContent = new ByteArrayContent(pngBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(fileContent, "file", "cnh.png");

            var uploadResp = await client.PostAsync($"/couriers/{courier!.Id}/cnh", content);
            uploadResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await uploadResp.Content.ReadFromJsonAsync<CourierResponse>();
            updated!.CnhImagePath.Should().NotBeNullOrWhiteSpace();
        }
    }
}
