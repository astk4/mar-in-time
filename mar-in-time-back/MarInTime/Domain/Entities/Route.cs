using System.ComponentModel.DataAnnotations;

namespace MarInTime.Domain.Entities
{
    public class Route
    {
        [Key]
        public byte Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public virtual ICollection<Port> Ports { get; set; } = new List<Port>();
    }
}
