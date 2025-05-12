using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.Models;
using PetDoa.Services.Interfaces;
using PetDoa.DTOs;

namespace PetDoa.Services
{
    public class OngService : IOngService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        // Injete o DbContext e o AutoMapper
        public OngService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OngReadDTO>> GetAllOngsAsync()
        {
            // Lógica movida do Controller: Busca ONGs no DB
            var ongs = await _context.ONGs
                // Se precisar de Administrators aqui por algum motivo, inclua.
                // Caso contrário, remova o Include para melhor performance.
                // .Include(o => o.Administrators)
                .ToListAsync();

            // Mapeia para DTOs
            return _mapper.Map<List<OngReadDTO>>(ongs);
        }

        public async Task<OngReadDTO?> GetOngByIdAsync(int id)
        {
            // Lógica movida do Controller: Busca ONG por ID
            var ong = await _context.ONGs
                // Se precisar de Administrators aqui por algum motivo, inclua.
                // .Include(o => o.Administrators)
                .FirstOrDefaultAsync(o => o.ID == id);

            // Retorna o DTO mapeado ou null se a ONG não for encontrada
            return _mapper.Map<OngReadDTO?>(ong);
        }

        public async Task<OngReadDTO> CreateOngAsync(CreateOngDTO createDto)
        {
            // Mapeia o DTO de criação para o Modelo ONG
            // Ajuste: Não mapeie a RegistrationDate do DTO
            var ong = new ONG
            {
                Name = createDto.Name,
                Email = createDto.Email,
                Phone = createDto.Phone,
                Description = createDto.Description,
                // Define a data de registro AQUI no serviço!
                RegistrationDate = DateTime.UtcNow // Usar UtcNow é uma boa prática para servidores
            };

            // Adiciona ao context
            _context.ONGs.Add(ong);
            // Salva as mudanças no banco
            await _context.SaveChangesAsync();

            // Mapeia a ONG recém-criada (agora com ID) para o DTO de leitura
            return _mapper.Map<OngReadDTO>(ong);
        }

        public async Task<bool> DeleteOngAsync(int id)
        {
            // Lógica movida do Controller: Busca a ONG
            var ong = await _context.ONGs.FindAsync(id);

            if (ong == null)
            {
                // ONG não encontrada, retorna false
                return false;
            }

            // Remove a ONG
            _context.ONGs.Remove(ong);
            // Salva as mudanças
            await _context.SaveChangesAsync();

            // Retorna true indicando sucesso
            return true;
        }
    }
}