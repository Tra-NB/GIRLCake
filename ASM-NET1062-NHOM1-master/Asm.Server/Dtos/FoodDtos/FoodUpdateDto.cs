using System.ComponentModel.DataAnnotations;

namespace Asm.Server.Dtos.FoodDtos;

public sealed record FoodUpdateDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }

    public string? ImageUrl { get; init; }

    public bool IsAvailable { get; init; }

    [Required]
    public int CategoryId { get; init; }

    public IFormFile? FImageFile { get; init; }
}
