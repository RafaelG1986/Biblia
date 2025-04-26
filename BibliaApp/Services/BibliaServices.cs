using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BibliaApp.Models;

namespace BibliaApp.Services
{
    public class BibliaService
    {
        private List<VersionBiblia> _versiones = new List<VersionBiblia>();
        private List<Libro> _libros = new List<Libro>();
        private List<Capitulo> _capitulos = new List<Capitulo>();
        private List<Versiculo> _versiculos = new List<Versiculo>();
        private List<TituloBiblico> _titulos = new List<TituloBiblico>();

        private readonly string _archivoVersiones;
        private readonly string _archivoLibros;
        private readonly string _archivoCapitulos;
        private readonly string _archivoVersiculos;
        private readonly string _archivoTitulos;
        
        public BibliaService()
        {
            // Crear directorio para datos en AppData
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BibliaApp");
                
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            // Configurar rutas de archivos
            _archivoVersiones = Path.Combine(appDataFolder, "versiones.json");
            _archivoLibros = Path.Combine(appDataFolder, "libros.json");
            _archivoCapitulos = Path.Combine(appDataFolder, "capitulos.json");
            _archivoVersiculos = Path.Combine(appDataFolder, "versiculos.json");
            _archivoTitulos = Path.Combine(appDataFolder, "titulos.json");
            
            // Cargar datos
            CargarDatos();
            
            // Crear datos de prueba si no hay datos
            if (!_versiones.Any())
            {
                CrearDatosDePrueba();
            }
        }
        
        private void CargarDatos()
        {
            _versiones = CargarJson<VersionBiblia>(_archivoVersiones);
            _libros = CargarJson<Libro>(_archivoLibros);
            _capitulos = CargarJson<Capitulo>(_archivoCapitulos);
            _versiculos = CargarJson<Versiculo>(_archivoVersiculos);
            _titulos = CargarJson<TituloBiblico>(_archivoTitulos);
        }
        
        private List<T> CargarJson<T>(string archivo)
        {
            if (File.Exists(archivo))
            {
                try
                {
                    string json = File.ReadAllText(archivo);
                    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar {archivo}: {ex.Message}");
                }
            }
            
            return new List<T>();
        }
        
        private void GuardarJson<T>(List<T> lista, string archivo)
        {
            try
            {
                string json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(archivo, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar {archivo}: {ex.Message}");
            }
        }
        
        private void GuardarVersiones() => GuardarJson(_versiones, _archivoVersiones);
        private void GuardarLibros() => GuardarJson(_libros, _archivoLibros);
        private void GuardarCapitulos() => GuardarJson(_capitulos, _archivoCapitulos);
        private void GuardarVersiculos() => GuardarJson(_versiculos, _archivoVersiculos);
        private void GuardarTitulos() => GuardarJson(_titulos, _archivoTitulos);
        
        private void CrearDatosDePrueba()
        {
            // Crear una versión de prueba
            var versionRVR = new VersionBiblia
            {
                Id = "RVR1960",
                Nombre = "Reina Valera 1960",
                Idioma = "Español",
                Descripcion = "Versión clásica en español"
            };
            
            // Agregar versión
            _versiones.Add(versionRVR);
            
            // Crear libro Génesis
            var genesis = new Libro
            {
                Id = 1,
                Nombre = "Génesis",
                Abreviatura = "Gn",
                Numero = 1
            };
            
            // Agregar libro
            _libros.Add(genesis);
            
            // Crear capítulo 1
            var capitulo1 = new Capitulo
            {
                Id = 1,
                Numero = 1,
                LibroId = genesis.Id
            };
            
            // Agregar capítulo
            _capitulos.Add(capitulo1);
            
            // Crear versículos
            _versiculos.Add(new Versiculo
            {
                Id = 1,
                Numero = 1,
                Texto = "En el principio creó Dios los cielos y la tierra.",
                CapituloId = capitulo1.Id
            });
            
            _versiculos.Add(new Versiculo
            {
                Id = 2,
                Numero = 2,
                Texto = "Y la tierra estaba desordenada y vacía, y las tinieblas estaban sobre la faz del abismo, y el Espíritu de Dios se movía sobre la faz de las aguas.",
                CapituloId = capitulo1.Id
            });
            
            // Guardar datos
            GuardarDatos();
        }
        
        private void GuardarDatos()
        {
            GuardarVersiones();
            GuardarLibros();
            GuardarCapitulos();
            GuardarVersiculos();
            GuardarTitulos();
        }
        
        // Métodos para Versiones
        public List<VersionBiblia> ObtenerVersiones() => _versiones;
        
        public VersionBiblia? ObtenerVersionPorId(string id)
        {
            return _versiones.FirstOrDefault(v => v.Id == id);
        }
        
        public void AgregarVersion(VersionBiblia version)
        {
            if (_versiones.Any(v => v.Id == version.Id))
                throw new Exception($"Ya existe una versión con el ID {version.Id}");
                
            _versiones.Add(version);
            GuardarVersiones();
        }
        
        public void ActualizarVersion(VersionBiblia version)
        {
            var versionExistente = _versiones.FirstOrDefault(v => v.Id == version.Id);
            if (versionExistente == null)
                throw new Exception($"No se encontró la versión con ID {version.Id}");
                
            versionExistente.Nombre = version.Nombre;
            versionExistente.Idioma = version.Idioma;
            versionExistente.Descripcion = version.Descripcion;
            
            GuardarVersiones();
        }
        
        // Métodos para Libros
        public List<Libro> ObtenerLibros(string versionId)
        {
            return _libros.Where(l => l.Id > 0).ToList();
        }
        
        public Libro? ObtenerLibroPorId(string versionId, int libroId)
        {
            return _libros.FirstOrDefault(l => l.Id == libroId);
        }
        
        public void AgregarLibro(string versionId, Libro libro)
        {
            // Asignar ID
            libro.Id = _libros.Count > 0 ? _libros.Max(l => l.Id) + 1 : 1;
            
            _libros.Add(libro);
            GuardarLibros();
        }
        
        public void ActualizarLibro(string versionId, Libro libro)
        {
            var libroExistente = _libros.FirstOrDefault(l => l.Id == libro.Id);
            if (libroExistente == null)
                throw new Exception($"No se encontró el libro con ID {libro.Id}");
                
            libroExistente.Nombre = libro.Nombre;
            libroExistente.Abreviatura = libro.Abreviatura;
            libroExistente.Numero = libro.Numero;
            
            GuardarLibros();
        }
        
        public void EliminarLibro(string versionId, int libroId)
        {
            var libro = _libros.FirstOrDefault(l => l.Id == libroId);
            if (libro == null)
                throw new Exception($"No se encontró el libro con ID {libroId}");
                
            // Primero eliminar capítulos
            var capitulosDelLibro = _capitulos.Where(c => c.LibroId == libroId).ToList();
            foreach (var capitulo in capitulosDelLibro)
            {
                EliminarCapitulo(versionId, libroId, capitulo.Id);
            }
            
            _libros.Remove(libro);
            GuardarLibros();
        }
        
        // Métodos para Capítulos
        public List<Capitulo> ObtenerCapitulos(string versionId, int libroId)
        {
            return _capitulos.Where(c => c.LibroId == libroId).ToList();
        }
        
        public Capitulo? ObtenerCapituloPorId(string versionId, int libroId, int capituloId)
        {
            return _capitulos.FirstOrDefault(c => c.Id == capituloId && c.LibroId == libroId);
        }
        
        public void AgregarCapitulo(string versionId, int libroId, Capitulo capitulo)
        {
            // Asignar ID
            capitulo.Id = _capitulos.Count > 0 ? _capitulos.Max(c => c.Id) + 1 : 1;
            capitulo.LibroId = libroId;
            
            _capitulos.Add(capitulo);
            GuardarCapitulos();
        }
        
        public void ActualizarCapitulo(string versionId, int libroId, Capitulo capitulo)
        {
            var capituloExistente = _capitulos.FirstOrDefault(c => c.Id == capitulo.Id);
            if (capituloExistente == null)
                throw new Exception($"No se encontró el capítulo con ID {capitulo.Id}");
                
            capituloExistente.Numero = capitulo.Numero;
            
            GuardarCapitulos();
        }
        
        public void EliminarCapitulo(string versionId, int libroId, int capituloId)
        {
            var capitulo = _capitulos.FirstOrDefault(c => c.Id == capituloId);
            if (capitulo == null)
                throw new Exception($"No se encontró el capítulo con ID {capituloId}");
                
            // Eliminar versículos
            var versiculosDelCapitulo = _versiculos.Where(v => v.CapituloId == capituloId).ToList();
            foreach (var versiculo in versiculosDelCapitulo)
            {
                _versiculos.Remove(versiculo);
            }
            
            // Eliminar títulos
            var titulosDelCapitulo = _titulos.Where(t => t.CapituloId == capituloId).ToList();
            foreach (var titulo in titulosDelCapitulo)
            {
                _titulos.Remove(titulo);
            }
            
            _capitulos.Remove(capitulo);
            
            GuardarCapitulos();
            GuardarVersiculos();
            GuardarTitulos();
        }
        
        // Métodos para Versículos
        public List<Versiculo> ObtenerVersiculos(string versionId, int libroId, int capituloId)
        {
            return _versiculos.Where(v => v.CapituloId == capituloId).ToList();
        }
        
        public void AgregarVersiculo(string versionId, int libroId, int capituloId, Versiculo versiculo)
        {
            // Asignar ID
            versiculo.Id = _versiculos.Count > 0 ? _versiculos.Max(v => v.Id) + 1 : 1;
            versiculo.CapituloId = capituloId;
            
            _versiculos.Add(versiculo);
            GuardarVersiculos();
        }
        
        public void ActualizarVersiculo(string versionId, int libroId, int capituloId, Versiculo versiculo)
        {
            var versiculoExistente = _versiculos.FirstOrDefault(v => v.Id == versiculo.Id);
            if (versiculoExistente == null)
                throw new Exception($"No se encontró el versículo con ID {versiculo.Id}");
                
            versiculoExistente.Numero = versiculo.Numero;
            versiculoExistente.Texto = versiculo.Texto;
            
            GuardarVersiculos();
        }
        
        public void EliminarVersiculo(string versionId, int libroId, int capituloId, int versiculoId)
        {
            var versiculo = _versiculos.FirstOrDefault(v => v.Id == versiculoId);
            if (versiculo == null)
                throw new Exception($"No se encontró el versículo con ID {versiculoId}");
                
            _versiculos.Remove(versiculo);
            GuardarVersiculos();
        }
        
        // Métodos para Títulos
        public List<TituloBiblico> ObtenerTitulos(string versionId, int libroId, int capituloId)
        {
            return _titulos
                .Where(t => t.VersionId == versionId && t.LibroId == libroId && t.CapituloId == capituloId)
                .OrderBy(t => t.PosicionPrevia)
                .ToList();
        }

        public void AgregarTitulo(TituloBiblico titulo)
        {
            // Asignar ID
            titulo.Id = _titulos.Count > 0 ? _titulos.Max(t => t.Id) + 1 : 1;
            
            _titulos.Add(titulo);
            GuardarTitulos();
        }

        public void ActualizarTitulo(TituloBiblico titulo)
        {
            var tituloExistente = _titulos.FirstOrDefault(t => t.Id == titulo.Id);
            if (tituloExistente == null)
                throw new Exception($"No se encontró el título con ID {titulo.Id}");
                
            tituloExistente.Texto = titulo.Texto;
            tituloExistente.PosicionPrevia = titulo.PosicionPrevia;
            
            GuardarTitulos();
        }

        public void EliminarTitulo(int tituloId)
        {
            var titulo = _titulos.FirstOrDefault(t => t.Id == tituloId);
            if (titulo == null)
                throw new Exception($"No se encontró el título con ID {tituloId}");
                
            _titulos.Remove(titulo);
            GuardarTitulos();
        }
    }
}