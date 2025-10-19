using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace pricewhisper.Models
{
    public class Empresa
    {
        public Empresa()
        {
            Usuarios = new List<Usuario>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmpresaId { get; set; }

        [Required]
        public required string CNPJ { get; set; }

        [Required]
        public required string RazaoSocial { get; set; }

        [Required]
        public required string NomeFantasia { get; set; }

        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; } 
    }
}
