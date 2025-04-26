using System.Collections.Generic;

namespace BibliaApp.Models
{
    public class Capitulo
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int LibroId { get; set; }
        public List<Versiculo> Versiculos { get; set; } = new List<Versiculo>();
    }
}