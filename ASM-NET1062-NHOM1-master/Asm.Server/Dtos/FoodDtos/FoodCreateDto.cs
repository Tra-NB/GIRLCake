using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Asm.Server.Dtos.FoodDtos
{
    public sealed class FoodCreateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? FImageFile { get; set; }
    }
}
