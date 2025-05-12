using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Services.Interfaces;
using PetDoa.Util;
using AutoMapper; 

namespace PetDoa.Services
{
    public class DonorService : IDonorService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Donor> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DonorService> _logger;


    public DonorService(AppDbContext context, IPasswordHasher<Donor> passwordHasher, IConfiguration configuration, IMapper mapper, IFileStorageService fileStorageServic, ILogger<DonorService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _mapper = mapper;
            _fileStorageService = fileStorageServic;
            _logger = logger;
        }

        public async Task<ServiceResponse<Donor>> RegisterDonorAsync(RegisterDonorDto dto)
        {
            var response = new ServiceResponse<Donor>();

            if (dto.Password != dto.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "As senhas não coincidem.";
                return response;
            }

            var passwordErrors = new List<string>();
            if (!PasswordValidator.IsValid(dto.Password, out passwordErrors))
            {
                response.Success = false;
                response.Message = "A senha não atende aos requisitos:";
                response.Errors = passwordErrors;
                return response;
            }

            var existing = await _context.Donors.FirstOrDefaultAsync(d => d.Email == dto.Email);
            if (existing != null)
            {
                response.Success = false;
                response.Message = "Já existe um doador com este e-mail.";
                return response;
            }

            var donor = new Donor
            {
                Name = dto.FullName,
                Email = dto.Email,
                Registration_Date = DateTime.UtcNow
            };

            donor.Password_Hash = _passwordHasher.HashPassword(donor, dto.Password);

            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            response.Data = donor;
            response.Message = "Doador registrado com sucesso!";
            response.Success = true;

            return response;
        }

        public async Task<string?> LoginDonorAsync(LoginDonorDto dto)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == dto.Email);

            if (donor == null || string.IsNullOrEmpty(donor.Password_Hash))
                return null;

            var result = _passwordHasher.VerifyHashedPassword(donor, donor.Password_Hash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;
            return await GenerateJwtForDonorAsync(donor);
        }

        public async Task<DonorReadDTO?> GetDonorByIdAsync(int id)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.ID == id);
            return _mapper.Map<DonorReadDTO?>(donor);
        }

        public async Task<IEnumerable<DonorReadDTO>> GetAllDonorsAsync()
        {
            var donors = await _context.Donors.ToListAsync();
            return _mapper.Map<List<DonorReadDTO>>(donors);
        }

        public async Task<bool> DeleteDonorAsync(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return false;
            }

            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GenerateJwtForDonorAsync(Donor donor)
        {

            if (donor == null)
            {
                // Logar um aviso seria bom aqui
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSection = _configuration.GetSection("Jwt");
                // Tratamento de nulo mais seguro ao pegar a chave
                var keyString = jwtSection["Key"];
                if (string.IsNullOrEmpty(keyString))
                {
                    throw new InvalidOperationException("Chave JWT não configurada.");
                }
                var key = Encoding.ASCII.GetBytes(keyString);

                var issuer = jwtSection["Issuer"];
                var audience = jwtSection["Audience"];
                var expiresInHours = Convert.ToDouble(jwtSection["ExpiresInHours"] ?? "2");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                // Usa as propriedades do objeto 'donor' recebido como parâmetro
                new Claim(ClaimTypes.NameIdentifier, donor.ID.ToString()),
                new Claim(ClaimTypes.Email, donor.Email ?? string.Empty), // Usa email do parâmetro
                new Claim(ClaimTypes.Name, donor.Name ?? string.Empty),   // Usa nome do parâmetro
                new Claim("DonorID", donor.ID.ToString())
            }),
                    Expires = DateTime.UtcNow.AddHours(expiresInHours),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                // Usamos await Task.FromResult para manter o método async (embora não seja estritamente necessário aqui)
                return await Task.FromResult(tokenHandler.WriteToken(token));
            }
            catch (Exception ex)
            {
                // TODO: Adicionar ILogger e logar o erro: _logger.LogError(ex, "Erro ao gerar JWT para Donor ID {DonorId}", donor.ID);
                Console.WriteLine($"Erro ao gerar JWT para Donor ID {donor.ID}: {ex.Message}"); // Log temporário
                return null;
            }
        }

        public async Task<OAuthSignInResult> FindOrCreateDonorByOAuthAsync(string provider, string providerKey, string email, string? name)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(providerKey) || string.IsNullOrEmpty(email))
            {
                return new OAuthSignInResult { ErrorMessage = "Dados insuficientes do provedor OAuth." };
            }

            try
            {
                var existingDonor = await _context.Donors.FirstOrDefaultAsync(d =>
                    d.OAuth_Provider == provider && d.OAuth_ID == providerKey);

                if (existingDonor != null)
                {
                    return new OAuthSignInResult { Donor = existingDonor }; 
                }

                var donorWithSameEmail = await _context.Donors.FirstOrDefaultAsync(d =>
                    d.Email == email && !string.IsNullOrEmpty(d.Password_Hash)); 

                if (donorWithSameEmail != null)
                {
                    return new OAuthSignInResult
                    {
                        RequiresPasswordLogin = true,
                        ErrorMessage = $"O e-mail '{email}' já está registrado. Por favor, faça login com sua senha."
                    };
                }

                var newDonor = new Donor
                {
                    Name = name ?? email, 
                    Email = email,
                    OAuth_Provider = provider, 
                    OAuth_ID = providerKey,    
                    Password_Hash = null,      
                    Registration_Date = DateTime.UtcNow
                };

                _context.Donors.Add(newDonor);
                await _context.SaveChangesAsync();

                return new OAuthSignInResult { Donor = newDonor }; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no FindOrCreateDonorByOAuthAsync: {ex.Message}");
                return new OAuthSignInResult { ErrorMessage = "Ocorreu um erro interno ao processar o login social." };
            }
        }

    
        public async Task<DonorReadDTO?> GetDonorProfileAsync(int donorId)
    {
      var donor = await _context.Donors
          .AsNoTracking()
          .FirstOrDefaultAsync(d => d.ID == donorId);

      if (donor == null)
      {
        return null;
      }
      return _mapper.Map<DonorReadDTO>(donor);
    }

        public async Task<bool> UpdateDonorProfileAsync(int donorId, UpdateDonorProfileDTO dto)
    {
      var donor = await _context.Donors.FirstOrDefaultAsync(d => d.ID == donorId);

      if (donor == null)
      {
        return false;
      }
      donor.Name = dto.Name;

      try
      {
        int changes = await _context.SaveChangesAsync();

        if (changes > 0)
        {
          return true;
        }
        else
        {
          return true;
        }
      }
      catch (DbUpdateConcurrencyException ex)
      {
        return false;
      }
      catch (DbUpdateException ex)
      {
        return false;
      }
      catch (Exception ex)
      {
        return false;
      }
    }


        public async Task<ServiceResponse<object?>> ChangePasswordAsync(int donorId, ChangePasswordDTO dto)
    {

      var donor = await _context.Donors.FirstOrDefaultAsync(d => d.ID == donorId);
      if (donor == null)
      {
        return new ServiceResponse<object?> { Success = false, Message = "Doador não encontrado." };
      }

      if (string.IsNullOrEmpty(donor.Password_Hash))
      {
        return new ServiceResponse<object?> { Success = false, Message = "Não é possível alterar a senha para esta conta (possivelmente vinculada a login social)." };
      }

      var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(donor, donor.Password_Hash, dto.CurrentPassword);
      if (passwordVerificationResult == PasswordVerificationResult.Failed)
      {
        return new ServiceResponse<object?> { Success = false, Message = "Senha atual incorreta." };
      }

      if (!PasswordValidator.IsValid(dto.NewPassword, out var passwordErrors))
      {
        return new ServiceResponse<object?> { Success = false, Message = "A nova senha não atende aos requisitos.", Errors = passwordErrors };
      }
      var newPasswordHash = _passwordHasher.HashPassword(donor, dto.NewPassword);

      donor.Password_Hash = newPasswordHash;

      try
      {
        await _context.SaveChangesAsync();
        return new ServiceResponse<object?> { Success = true, Message = "Senha alterada com sucesso." };
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao salvar nova senha para Donor ID {donorId}: {ex.Message}");
        return new ServiceResponse<object?> { Success = false, Message = "Ocorreu um erro ao salvar a nova senha. Tente novamente." };
      }
    }

    public async Task<string?> UpdateProfilePictureAsync(int donorId, IFormFile imageFile)
    {
      _logger?.LogInformation("Iniciando atualização de foto de perfil para Donor ID: {DonorId}", donorId); 

      if (imageFile == null || imageFile.Length == 0)
      {
        _logger?.LogWarning("Nenhum arquivo enviado para Donor ID: {DonorId}", donorId);
        return null;
      }

      const long maxFileSize = 5 * 1024 * 1024;
      if (imageFile.Length > maxFileSize)
      {
        _logger?.LogWarning("Arquivo muito grande enviado para Donor ID: {DonorId}. Tamanho: {FileSize}", donorId, imageFile.Length);
        throw new ArgumentException($"O arquivo excede o limite de tamanho de {maxFileSize / 1024 / 1024}MB.");
      }
      var allowedContentTypes = new[] { "image/jpeg", "image/png" };
      if (!allowedContentTypes.Contains(imageFile.ContentType.ToLowerInvariant()))
      {
        _logger?.LogWarning("Tipo de arquivo inválido enviado para Donor ID: {DonorId}. Tipo: {ContentType}", donorId, imageFile.ContentType);
        throw new ArgumentException("Tipo de arquivo inválido. Apenas JPEG e PNG são permitidos.");
      }

      var donor = await _context.Donors.FirstOrDefaultAsync(d => d.ID == donorId);
      if (donor == null)
      {
        _logger?.LogWarning("Doador não encontrado para atualizar foto. ID: {DonorId}", donorId);
        return null;
      }

      string containerName = _configuration.GetValue<string>("AzureStorage:ContainerName")
                             ?? throw new InvalidOperationException("AzureStorage:ContainerName não configurado.");

      var fileExtension = Path.GetExtension(imageFile.FileName);
      var uniqueFileName = $"donor-{donorId}-{Guid.NewGuid()}{fileExtension}";

      string? oldImageUrl = donor.ProfilePictureUrl;

      string? newImageUrl;
      try
      {
        await using var stream = imageFile.OpenReadStream();
        newImageUrl = await _fileStorageService.UploadFileAsync(
            blobContainerName: containerName,
            content: stream,
            contentType: imageFile.ContentType,
            fileName: uniqueFileName
        );
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Erro no serviço de storage ao fazer upload para Donor ID: {DonorId}", donorId);
        return null;
      }
      if (string.IsNullOrEmpty(newImageUrl))
      {
        _logger?.LogError("Serviço de storage retornou URL nula após upload para Donor ID: {DonorId}", donorId);
        return null;
      }
      donor.ProfilePictureUrl = newImageUrl;
      try
      {
        await _context.SaveChangesAsync();
        _logger?.LogInformation("URL da foto de perfil atualizada para Donor ID: {DonorId}. Nova URL: {NewUrl}", donorId, newImageUrl);
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Erro ao salvar URL da foto no DB para Donor ID: {DonorId}", donorId);
        await _fileStorageService.DeleteFileAsync(containerName, uniqueFileName);
        return null;
      }
      if (!string.IsNullOrEmpty(oldImageUrl))
      {
        try
        {         
          Uri oldUri = new Uri(oldImageUrl);
          string oldBlobName = Path.GetFileName(oldUri.LocalPath); 
          if (!string.IsNullOrEmpty(oldBlobName))
          {
            await _fileStorageService.DeleteFileAsync(containerName, oldBlobName);
            _logger?.LogInformation("Foto de perfil antiga deletada para Donor ID: {DonorId}. Blob: {OldBlobName}", donorId, oldBlobName);
          }
        }
        catch (Exception ex)
        {
          _logger?.LogError(ex, "Erro ao deletar foto de perfil antiga para Donor ID: {DonorId}. URL antiga: {OldUrl}", donorId, oldImageUrl); 
        }
      }
      return newImageUrl;
    }


    public async Task<bool> DeleteProfilePictureAsync(int donorId)
    {
      _logger?.LogInformation("Iniciando remoção de foto de perfil para Donor ID: {DonorId}", donorId);

      var donor = await _context.Donors.FirstOrDefaultAsync(d => d.ID == donorId);
      if (donor == null)
      {
        _logger?.LogWarning("Doador não encontrado para remover foto. ID: {DonorId}", donorId);
        return false;
      }

      string? imageUrlToDelete = donor.ProfilePictureUrl;
      if (string.IsNullOrEmpty(imageUrlToDelete))
      {
        _logger?.LogInformation("Nenhuma foto de perfil para remover para Donor ID: {DonorId}", donorId);
        return true;
      }

      try
      {
        string containerName = _configuration.GetValue<string>("AzureStorage:ContainerName")
                               ?? throw new InvalidOperationException("AzureStorage:ContainerName não configurado.");

        Uri uri = new Uri(imageUrlToDelete);
        string blobName = Path.GetFileName(uri.LocalPath);

        if (!string.IsNullOrEmpty(blobName))
        {
          await _fileStorageService.DeleteFileAsync(containerName, blobName);
          _logger?.LogInformation("Blob da foto de perfil deletado do Azure para Donor ID: {DonorId}. Blob: {BlobName}", donorId, blobName);
        }
        else
        {
          _logger?.LogWarning("Não foi possível extrair o nome do blob da URL para Donor ID: {DonorId}. URL: {ImageUrl}", donorId, imageUrlToDelete);
        }
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Erro ao tentar deletar blob no Azure para Donor ID: {DonorId}. URL: {ImageUrl}", donorId, imageUrlToDelete);
      }

      donor.ProfilePictureUrl = null;

      try
      {
        await _context.SaveChangesAsync();
         _logger?.LogInformation("URL da foto de perfil removida do banco para Donor ID: {DonorId}", donorId);
        return true;
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Erro ao salvar ProfilePictureUrl=null no DB para Donor ID: {DonorId}", donorId);
        return false; 
      }
    }

  }
}
