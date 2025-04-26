namespace BibliaApp.Models
{
    public class Versiculo
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int CapituloId { get; set; }
    }
}