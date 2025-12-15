namespace Asm.Server.Dtos.ComboDtos
{
    public class ComboCreateDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }

        public bool IsAvailable { get; set; }
        public List<ComboFoodCreateDto> Foods { get; set; }
    }
}
