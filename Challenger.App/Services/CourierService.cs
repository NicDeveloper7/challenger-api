using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using Challenger.Domain.Enums;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Challenger.App.Services
{
    public interface ICourierService
    {
        Task<CourierResponse> CreateAsync(CreateCourierRequest request);
    Task<CourierResponse?> UploadCnhAsync(Guid courierId, Stream content, string contentType, string fileName);
    }

    public class CourierService : ICourierService
    {
        private readonly ICourierRepository _courierRepo;
        private readonly IStorageService _storage;

    private static readonly string[] AllowedCnhTypes = new[] { "A", "B", "A+B", "AB" };
    private static readonly string[] AllowedMimeTypes = new[] { "image/png", "image/bmp" };
    private static readonly string[] AllowedExtensions = new[] { ".png", ".bmp" };

        public CourierService(ICourierRepository courierRepo, IStorageService storage)
        {
            _courierRepo = courierRepo;
            _storage = storage;
        }

        public async Task<CourierResponse> CreateAsync(CreateCourierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Name, Email and Password are required.");

            if (string.IsNullOrWhiteSpace(request.CnhNumber) || string.IsNullOrWhiteSpace(request.Cnpj) || string.IsNullOrWhiteSpace(request.CnhType))
                throw new ArgumentException("CNH number, CNH type and CNPJ are required.");

            if (!AllowedCnhTypes.Contains(request.CnhType.ToUpper()))
                throw new ArgumentException("CNH type must be A, B or A+B.");

            if (await _courierRepo.ExistsCnhAsync(request.CnhNumber))
                throw new InvalidOperationException("CNH must be unique.");

            if (await _courierRepo.ExistsCnpjAsync(request.Cnpj))
                throw new InvalidOperationException("CNPJ must be unique.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Password = request.Password, 
                Role = UserRole.Deliveryman
            };

            var profile = new DeliverymanProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                CnhNumber = request.CnhNumber,
                CnhType = NormalizeCnhType(request.CnhType),
                Cnpj = request.Cnpj,
                BirthDate = request.BirthDate
            };

            await _courierRepo.AddAsync(profile);
            await _courierRepo.SaveChangesAsync();

            return Map(profile);
        }

        public async Task<CourierResponse?> UploadCnhAsync(Guid courierId, Stream content, string contentType, string fileName)
        {
            if (content == null)
                throw new ArgumentException("Arquivo é obrigatório.");

            if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.Contains(contentType.ToLower()))
                throw new ArgumentException("O formato do arquivo deve ser PNG ou BMP. SOMENTE.");

            var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException("O formato do arquivo deve ser PNG ou BMP. SOMENTE.");

            Stream workStream = content;
            if (!content.CanSeek)
            {
                var ms = new MemoryStream();
                content.CopyTo(ms);
                ms.Position = 0;
                workStream = ms;
            }

            long originalPosition = workStream.CanSeek ? workStream.Position : 0;
            var header = new byte[8];
            int read = workStream.Read(header, 0, header.Length);
            bool isPng = read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            bool isBmp = read >= 2 && header[0] == 0x42 && header[1] == 0x4D; 

            if (workStream.CanSeek)
                workStream.Position = originalPosition;

            if (!isPng && !isBmp)
                throw new ArgumentException("O formato do arquivo deve ser PNG ou BMP. SOMENTE.");

            var profile = await _courierRepo.GetByIdAsync(courierId);
            if (profile == null) return null;

            if (workStream.CanSeek)
                workStream.Position = 0;

            var safeFileName = string.IsNullOrWhiteSpace(fileName) ? $"upload{ext}" : fileName;
            var path = await _storage.SaveAsync(workStream, contentType, "cnh", safeFileName);
            profile.CnhImagePath = path;
            await _courierRepo.UpdateAsync(profile);
            await _courierRepo.SaveChangesAsync();

            return Map(profile);
        }

        private static string NormalizeCnhType(string type)
        {
            type = type.Trim().ToUpper();
            if (type == "AB") return "A+B";
            return type;
        }

        private static CourierResponse Map(DeliverymanProfile p)
        {
            return new CourierResponse
            {
                Id = p.Id,
                Name = p.User?.Name ?? string.Empty,
                Email = p.User?.Email ?? string.Empty,
                CnhNumber = p.CnhNumber,
                CnhType = p.CnhType,
                Cnpj = p.Cnpj,
                BirthDate = p.BirthDate,
                CnhImagePath = p.CnhImagePath
            };
        }
    }
}
