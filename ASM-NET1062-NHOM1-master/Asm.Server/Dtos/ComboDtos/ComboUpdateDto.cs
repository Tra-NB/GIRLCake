namespace Asm.Server.Dtos.ComboDtos
{
    public class ComboUpdateDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; }
        public bool IsAvailable { get; set; }
        public List<ComboFoodUpdateDto> Foods { get; set; }
        public string? Description { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
