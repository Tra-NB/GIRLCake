namespace Asm.Server.Dtos.ComboDtos
{
    public class ComboResponDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? DeletedAt { get; set; }

        public string? Description { get; set; }
        public bool IsAvailable { get; set; }


        public List<FoodInComboDto>? Foods { get; set; }
    }
}
