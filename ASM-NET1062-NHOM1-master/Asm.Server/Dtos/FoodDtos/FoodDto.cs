namespace Asm.Server.Dtos.FoodDtos;

public sealed record FoodDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsAvailable { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; set; }

    public int AverageRating { get; init; }
    }
