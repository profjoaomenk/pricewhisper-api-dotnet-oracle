namespace pricewhisper.Models.DTOs
{
    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public required string Nome { get; set; }
        public required string NomeUsuario { get; set; }
        public required string Senha { get; set; }
        public int EmpresaId { get; set; }
        public required string RazaoSocialEmpresa { get; set; }
    }
}
