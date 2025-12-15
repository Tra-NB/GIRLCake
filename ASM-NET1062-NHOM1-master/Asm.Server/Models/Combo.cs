using System.ComponentModel.DataAnnotations;

namespace Asm.Server.Models
{
	public class Combo : Product
	{
        public ICollection<ComboFood>? ComboFoods { get; set; }
	}
}
