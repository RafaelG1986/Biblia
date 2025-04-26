using System.Collections.Generic;

namespace BibliaApp.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Abreviatura { get; set; } = string.Empty;
        public int Numero { get; set; }
        public List<Capitulo> Capitulos { get; set; } = new List<Capitulo>();
    }
}