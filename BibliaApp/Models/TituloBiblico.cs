namespace BibliaApp.Models
{
    public class TituloBiblico
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string VersionId { get; set; } = string.Empty;
        public int LibroId { get; set; }
        public int CapituloId { get; set; }
        public int PosicionPrevia { get; set; } // Versículo después del cual aparece el título
    }
}