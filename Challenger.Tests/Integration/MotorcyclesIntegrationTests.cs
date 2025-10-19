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
    public class MotorcyclesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public MotorcyclesIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_And_Get_List_Should_Work()
        {
            var client = _factory.CreateClient();

            var req = new CreateMotorcycleRequest { Year = 2024, Model = "Honda CG 160", Plate = "ABC1D23" };
            var createResp = await client.PostAsJsonAsync("/motorcycles", req);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<MotorcycleResponse>();
            created.Should().NotBeNull();
            created!.Plate.Should().Be("ABC1D23");

            var listResp = await client.GetAsync("/motorcycles");
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await listResp.Content.ReadFromJsonAsync<MotorcycleResponse[]>();
            list.Should().NotBeNull();
            list!.Should().ContainSingle(m => m.Plate == "ABC1D23");

            var filteredResp = await client.GetAsync("/motorcycles?plate=ABC1D23");
            filteredResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var filtered = await filteredResp.Content.ReadFromJsonAsync<MotorcycleResponse[]>();
            filtered!.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdatePlate_And_Delete_Should_Work()
        {
            var client = _factory.CreateClient();

            var req = new CreateMotorcycleRequest { Year = 2023, Model = "Yamaha", Plate = "AAA1A11" };
            var createResp = await client.PostAsJsonAsync("/motorcycles", req);
            createResp.EnsureSuccessStatusCode();
            var created = await createResp.Content.ReadFromJsonAsync<MotorcycleResponse>();
            created.Should().NotBeNull();

            var upd = new UpdateMotorcyclePlateRequest { Plate = "BBB2B22" };
            var putResp = await client.PutAsJsonAsync($"/motorcycles/{created!.Id}", upd);
            putResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await putResp.Content.ReadFromJsonAsync<MotorcycleResponse>();
            updated!.Plate.Should().Be("BBB2B22");

            var delResp = await client.DeleteAsync($"/motorcycles/{created.Id}");
            delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var delAgainResp = await client.DeleteAsync($"/motorcycles/{created.Id}");
            delAgainResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Duplicate_Plate_Should_Return_Conflict()
        {
            var client = _factory.CreateClient();
            var r1 = new CreateMotorcycleRequest { Year = 2024, Model = "Honda", Plate = "ZZZ9Z99" };
            var r2 = new CreateMotorcycleRequest { Year = 2024, Model = "Honda", Plate = "ZZZ9Z99" };

            var resp1 = await client.PostAsJsonAsync("/motorcycles", r1);
            resp1.StatusCode.Should().Be(HttpStatusCode.Created);
            var resp2 = await client.PostAsJsonAsync("/motorcycles", r2);
            resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
