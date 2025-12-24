using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarInTime.Domain.Entities
{
    [PrimaryKey(nameof(Id), nameof(CountryId))]
    public class Port
    {
        [Length(3, 3)]
        public string? Id { get; set; }

        [Required]  
        public string? Name { get; set; }

        [Precision(8, 6)]
        public decimal Latitude { get; set; }
        
        [Precision(9, 6)]
        public decimal Longitude { get; set; }

        [ForeignKey(nameof(Country))]
        public string? CountryId { get; set; }

        public virtual Country? Country { get; set; }

        public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
    }
}
