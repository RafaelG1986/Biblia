using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BibliaApp.Models;
using BibliaApp.Services;
using Microsoft.VisualBasic; // Añade esta línea para usar InputBox

namespace BibliaApp.Views
{
    public partial class GestionContenidoWindow : Window
    {
        private readonly BibliaService _bibliaService;
        private VersionBiblia? _versionSeleccionada;
        private Libro? _libroSeleccionado;
        private Capitulo? _capituloSeleccionado;

        public GestionContenidoWindow(BibliaService bibliaService)
        {
            InitializeComponent();
            
            _bibliaService = bibliaService;
            
            // Cargar datos iniciales
            CargarVersiones();
        }
        
        private void CargarVersiones()
        {
            var versiones = _bibliaService.ObtenerVersiones();
            cmbVersiones.ItemsSource = versiones;
            
            if (versiones.Any())
            {
                cmbVersiones.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("No hay versiones disponibles. Por favor, cree una versión primero.", 
                    "Sin versiones", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void CargarLibros()
        {
            if (_versionSeleccionada == null) return;
            
            var libros = _bibliaService.ObtenerLibros(_versionSeleccionada.Id);
            cmbLibros.ItemsSource = libros;
            
            if (libros.Any())
            {
                cmbLibros.SelectedIndex = 0;
            }
            else
            {
                cmbLibros.ItemsSource = null;
                cmbCapitulos.ItemsSource = null;
                
                _libroSeleccionado = null;
                _capituloSeleccionado = null;
                
                MessageBox.Show("No hay libros disponibles en esta versión. Puede agregar un nuevo libro usando la opción correspondiente.", 
                    "Sin libros", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void CargarCapitulos()
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null) return;
            
            var capitulos = _bibliaService.ObtenerCapitulos(_versionSeleccionada.Id, _libroSeleccionado.Id);
            cmbCapitulos.ItemsSource = capitulos;
            
            if (capitulos.Any())
            {
                cmbCapitulos.SelectedIndex = 0;
            }
            else
            {
                cmbCapitulos.ItemsSource = null;
                
                _capituloSeleccionado = null;
                
                MessageBox.Show("No hay capítulos disponibles en este libro. Puede agregar un nuevo capítulo usando la opción correspondiente.", 
                    "Sin capítulos", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void CargarVersiculos()
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null || _capituloSeleccionado == null) return;
            
            var versiculos = _bibliaService.ObtenerVersiculos(
                _versionSeleccionada.Id, 
                _libroSeleccionado.Id, 
                _capituloSeleccionado.Id);
                
            lstVersiculos.ItemsSource = versiculos.OrderBy(v => v.Numero).ToList();
        }

        private void cmbVersiones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _versionSeleccionada = cmbVersiones.SelectedItem as VersionBiblia;
            _libroSeleccionado = null;
            _capituloSeleccionado = null;
            
            CargarLibros();
        }
        
        private void cmbLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _libroSeleccionado = cmbLibros.SelectedItem as Libro;
            _capituloSeleccionado = null;
            
            CargarCapitulos();
        }
        
        private void cmbCapitulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _capituloSeleccionado = cmbCapitulos.SelectedItem as Capitulo;
            
            // Cargar versículos del capítulo seleccionado
            CargarVersiculos();
            
            // Actualizar texto ejemplo en el campo de rango
            if (_libroSeleccionado != null && _capituloSeleccionado != null)
            {
                txtRangoVersiculos.Text = $"{_libroSeleccionado.Nombre} {_capituloSeleccionado.Numero}:1-";
            }
        }

        private void lstVersiculos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar/deshabilitar botones según haya selección
            btnEliminarVersiculo.IsEnabled = lstVersiculos.SelectedItem != null;
            btnEditarVersiculo.IsEnabled = lstVersiculos.SelectedItem != null;
        }

        private void btnEliminarVersiculo_Click(object sender, RoutedEventArgs e)
        {
            var versiculoSeleccionado = lstVersiculos.SelectedItem as Versiculo;
            if (versiculoSeleccionado == null || _capituloSeleccionado == null || 
                _libroSeleccionado == null || _versionSeleccionada == null) return;
            
            var mensaje = $"¿Está seguro de que desea eliminar el versículo {versiculoSeleccionado.Numero}?";
            var resultado = MessageBox.Show(mensaje, "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _bibliaService.EliminarVersiculo(
                        _versionSeleccionada.Id,
                        _libroSeleccionado.Id,
                        _capituloSeleccionado.Id,
                        versiculoSeleccionado.Id);
                        
                    MessageBox.Show("Versículo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Recargar versículos
                    CargarVersiculos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar versículo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditarVersiculo_Click(object sender, RoutedEventArgs e)
        {
            var versiculoSeleccionado = lstVersiculos.SelectedItem as Versiculo;
            if (versiculoSeleccionado == null || _capituloSeleccionado == null || 
                _libroSeleccionado == null || _versionSeleccionada == null) return;
            
            // Aquí podrías abrir una ventana de edición o implementar edición en línea
            // Por ahora haremos una edición simple con input dialog
            string nuevoTexto = Microsoft.VisualBasic.Interaction.InputBox(
                $"Editar versículo {versiculoSeleccionado.Numero}:", 
                "Editar versículo", 
                versiculoSeleccionado.Texto);
            
            if (!string.IsNullOrWhiteSpace(nuevoTexto) && nuevoTexto != versiculoSeleccionado.Texto)
            {
                try
                {
                    var versiculo = new Versiculo
                    {
                        Id = versiculoSeleccionado.Id,
                        Numero = versiculoSeleccionado.Numero,
                        Texto = nuevoTexto.Trim(),
                        CapituloId = _capituloSeleccionado.Id
                    };
                    
                    _bibliaService.ActualizarVersiculo(
                        _versionSeleccionada.Id, 
                        _libroSeleccionado.Id, 
                        _capituloSeleccionado.Id, 
                        versiculo);
                        
                    MessageBox.Show("Versículo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Recargar versículos
                    CargarVersiculos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar versículo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAgregarMultiplesVersiculos_Click(object sender, RoutedEventArgs e)
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null || _capituloSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar una versión, libro y capítulo.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Verificar si hay texto ingresado
            if (string.IsNullOrWhiteSpace(txtMultiplesVersiculos.Text))
            {
                MessageBox.Show("Debe ingresar el texto de los versículos.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMultiplesVersiculos.Focus();
                return;
            }
            
            try
            {
                // Parsear versículos ingresados
                var lineas = txtMultiplesVersiculos.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Lista para los versículos procesados
                var versiculosAgregados = new List<Versiculo>();
                var versiculosFallidos = new List<string>();
                
                foreach (var linea in lineas)
                {
                    // Buscar el primer espacio que separa el número del texto
                    int primerEspacio = linea.IndexOf(' ');
                    if (primerEspacio <= 0) 
                    {
                        versiculosFallidos.Add(linea);
                        continue;
                    }
                    
                    // Intentar parsear el número del versículo
                    if (!int.TryParse(linea.Substring(0, primerEspacio), out int numeroVersiculo) || numeroVersiculo <= 0)
                    {
                        versiculosFallidos.Add(linea);
                        continue;
                    }
                    
                    // Extraer el texto del versículo
                    string textoVersiculo = linea.Substring(primerEspacio + 1).Trim();
                    if (string.IsNullOrWhiteSpace(textoVersiculo))
                    {
                        versiculosFallidos.Add(linea);
                        continue;
                    }
                    
                    // Crear el versículo
                    var versiculo = new Versiculo
                    {
                        Numero = numeroVersiculo,
                        Texto = textoVersiculo,
                        CapituloId = _capituloSeleccionado.Id
                    };
                    
                    // Agregar a la base de datos
                    _bibliaService.AgregarVersiculo(
                        _versionSeleccionada.Id, 
                        _libroSeleccionado.Id, 
                        _capituloSeleccionado.Id, 
                        versiculo);
                        
                    versiculosAgregados.Add(versiculo);
                }
                
                // Mostrar resultados
                if (versiculosAgregados.Count > 0)
                {
                    MessageBox.Show($"Se agregaron {versiculosAgregados.Count} versículos correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Limpiar el texto ingresado
                    txtMultiplesVersiculos.Clear();
                    txtRangoVersiculos.Clear();
                    
                    // Recargar la lista de versículos
                    CargarVersiculos();
                }
                
                if (versiculosFallidos.Count > 0)
                {
                    string mensaje = $"No se pudieron procesar {versiculosFallidos.Count} líneas. Asegúrese de que cada línea comience con un número seguido de un espacio y luego el texto del versículo.";
                    MessageBox.Show(mensaje, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar versículos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}