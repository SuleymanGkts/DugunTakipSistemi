using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DugunTakipSistemi.Models
{
    [Table("Mekanlar")]
    public class Mekan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MekanAdi { get; set; }
    }
}