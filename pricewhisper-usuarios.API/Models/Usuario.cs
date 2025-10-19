using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace pricewhisper.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }

        [Required]
        public required string Nome { get; set; }

        [Required]
        public required string NomeUsuario { get; set; }

        [Required]
        public required string Senha { get; set; }

        public int EmpresaId { get; set; }

        [JsonIgnore]
        public Empresa? Empresa { get; set; }
    }
}
