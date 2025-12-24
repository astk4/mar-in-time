using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarInTime.Domain.Entities
{
    [Table("Countries")]
    public class Country
    {
        [Key]
        [Length(2, 2)]
        public string? IsoId {  get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
