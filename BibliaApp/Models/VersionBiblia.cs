using System.Collections.Generic;

namespace BibliaApp.Models
{
    public class VersionBiblia
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Idioma { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<Libro> Libros { get; set; } = new List<Libro>();
    }
}