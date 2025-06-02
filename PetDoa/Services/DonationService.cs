// PetDoa/Services/DonationService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Services.Interfaces;

namespace PetDoa.Services
{
    public class DonationService : IDonationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private const decimal MinDonationAmount = 0.01M; // Define o valor mínimo

        public DonationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DonationReadDTO> CreateDonationAsync(CreateDonationDTO createDto, int donorId)
        {
            // 1. Validar valor mínimo da doação
            if (createDto.Amount < MinDonationAmount)
            {
                throw new ArgumentException($"O valor mínimo para doação é R$ {MinDonationAmount}.");
            }

            // 2. Verificar se a ONG existe
            var ongExists = await _context.ONGs.AnyAsync(o => o.ID == createDto.OngID);
            if (!ongExists)
            {
                throw new ArgumentException($"ONG com ID {createDto.OngID} não encontrada.");
            }

            // 3. Verificar se o Doador existe (embora deva vir de um usuário autenticado)
            var donorExists = await _context.Donors.AnyAsync(d => d.ID == donorId);
            if (!donorExists)
            {
                // Isso não deveria acontecer se o donorId vem do token JWT válido,
                // mas é uma verificação de segurança extra.
                throw new ArgumentException($"Doador com ID {donorId} não encontrado.");
            }

            // 4. Criar a entidade Donation
            var donation = new Donation
            {
                Amount = createDto.Amount,
                Method = createDto.Method, // Considere validar os métodos permitidos
                IsRecurring = createDto.IsRecurring,
                Date = DateTime.UtcNow, // Usar data/hora do servidor
                DonorID = donorId,       // ID do doador autenticado
                OngId = createDto.OngID
            };

            // 5. Adicionar e Salvar
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // 6. Mapear para DTO e retornar
            // O DonationReadDTO atual só precisa dos IDs, então um mapeamento simples funciona.
            // Se precisasse de nomes, teríamos que buscar a 'donation' novamente com Includes.
            return _mapper.Map<DonationReadDTO>(donation);
        }

        public async Task<IEnumerable<DonationReadDTO>> GetAllDonationsAsync()
        {
            // Para GetAll, pode ser útil incluir Donor e ONG se o DTO precisasse dos nomes.
            // Como não precisa, a consulta simples é mais eficiente.
            var donations = await _context.Donations
                                          // .Include(d => d.Donor) // Se precisar do nome do Doador no DTO
                                          // .Include(d => d.ONG)   // Se precisar do nome da ONG no DTO
                                          .ToListAsync();
            return _mapper.Map<List<DonationReadDTO>>(donations);
        }

        public async Task<DonationReadDTO?> GetDonationByIdAsync(int id)
        {
            var donation = await _context.Donations
                                         // .Include(d => d.Donor) // Se precisar do nome do Doador no DTO
                                         // .Include(d => d.ONG)   // Se precisar do nome da ONG no DTO
                                         .FirstOrDefaultAsync(d => d.ID == id);

            return _mapper.Map<DonationReadDTO?>(donation);
        }

        public async Task<IEnumerable<DonationReadDTO>> GetDonationsByDonorAsync(int donorId)
        {
            var donations = await _context.Donations
                                          .Where(d => d.DonorID == donorId)
                                          // .Include(d => d.ONG) // Pode ser útil incluir a ONG aqui
                                          .OrderByDescending(d => d.Date) // Ordenar por data, mais recentes primeiro
                                          .ToListAsync();

            return _mapper.Map<List<DonationReadDTO>>(donations);
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return false;
            }

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}