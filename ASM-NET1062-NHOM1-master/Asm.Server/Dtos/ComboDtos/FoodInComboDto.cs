namespace Asm.Server.Dtos.ComboDtos
{
    public class FoodInComboDto
    {
        public int FoodId { get; set; }
        public string Name { get; set; }       // tên món
        public string ImageUrl { get; set; }   // ảnh
        public decimal Price { get; set; }     // giá món
        public int Quantity { get; set; }      // số lượng trong combo
    }
}
