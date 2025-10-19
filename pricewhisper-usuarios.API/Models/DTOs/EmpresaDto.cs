namespace pricewhisper.Models.DTOs
{
    public class EmpresaDto
    {
        public int EmpresaId { get; set; }
        public required string CNPJ { get; set; }
        public required string RazaoSocial { get; set; }
        public required string NomeFantasia { get; set; }
        public required List<UsuarioDto> Usuarios { get; set; }
    }
}
