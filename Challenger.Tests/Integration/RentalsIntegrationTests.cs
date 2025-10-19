using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using FluentAssertions;
using Xunit;

namespace Challenger.Tests.Integration
{
    public class RentalsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public RentalsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<Guid> CreateCourierAsync(HttpClient client)
        {
            var req = new CreateCourierRequest
            {
                Name = "Patricya Balbino",
                Email = "Patricya@example.com",
                Password = "Senha@123",
                CnhNumber = Guid.NewGuid().ToString("N").Substring(0,10),
                CnhType = "A",
                Cnpj = "98765432000188",
                BirthDate = new DateTime(1992, 5, 20)
            };
            var resp = await client.PostAsJsonAsync("/couriers", req);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<CourierResponse>();
            return body!.Id;
        }

        [Fact]
        public async Task Create_Get_Return_Rental_Should_Work()
        {
            var client = _factory.CreateClient();
            var courierId = await CreateCourierAsync(client);

            var createReq = new CreateRentalRequest
            {
                CourierId = courierId,
                PlanDays = 7,
                ExpectedEndDate = DateTime.UtcNow.Date.AddDays(8) 
            };

            var createResp = await client.PostAsJsonAsync("/rentals", createReq);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var rental = await createResp.Content.ReadFromJsonAsync<RentalResponse>();
            rental.Should().NotBeNull();

            var getResp = await client.GetAsync($"/rentals/{rental!.Id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var fetched = await getResp.Content.ReadFromJsonAsync<RentalResponse>();
            fetched!.Id.Should().Be(rental.Id);

            var returnReq = new ReturnRentalRequest
            {
                ActualEndDate = rental.ExpectedEndDate 
            };
            var returnResp = await client.PutAsJsonAsync($"/rentals/{rental.Id}/return", returnReq);
            returnResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var returned = await returnResp.Content.ReadFromJsonAsync<RentalResponse>();
            returned!.FinalAmount.Should().BeGreaterThan(0);
            returned.Status.Should().BeOneOf("completed", "late");
        }
    }
}
