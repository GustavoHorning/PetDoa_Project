﻿using PetDoa.Models.Enums;

namespace PetDoa.DTOs
{
    public class DonationReadDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } 
        public bool IsRecurring { get; set; }
        public DateTime Date { get; set; }

        // Apenas o ID do doador e da ONG
        public int DonorID { get; set; }
        public int OngID { get; set; }
    }
}


