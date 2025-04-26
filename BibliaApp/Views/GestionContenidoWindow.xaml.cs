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
        private TituloBiblico? _tituloSeleccionado;
        private bool _modoEdicionTitulo = false;

        // Variables para gestión de libros y capítulos
        private Libro? _libroSeleccionadoAdmin;
        private Capitulo? _capituloSeleccionadoAdmin;
        private bool _modoEdicionLibro = false;
        private bool _modoEdicionCapitulo = false;

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
            // Cargamos también en la lista de administración
            lstLibrosAdmin.ItemsSource = libros;
            
            if (libros.Any())
            {
                cmbLibros.SelectedIndex = 0;
            }
            else
            {
                cmbLibros.ItemsSource = null;
                cmbCapitulos.ItemsSource = null;
                lstCapitulosAdmin.ItemsSource = null;
                
                _libroSeleccionado = null;
                _capituloSeleccionado = null;
                
                MessageBox.Show("No hay libros disponibles en esta versión. Puede agregar un nuevo libro usando la pestaña Libros.", 
                    "Sin libros", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void CargarCapitulos()
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null) return;
            
            var capitulos = _bibliaService.ObtenerCapitulos(_versionSeleccionada.Id, _libroSeleccionado.Id);
            cmbCapitulos.ItemsSource = capitulos;
            // Cargamos también en la lista de administración
            lstCapitulosAdmin.ItemsSource = capitulos.Select(c => new {
                c.Id,
                c.Numero,
                NumeroVersiculos = _bibliaService.ObtenerVersiculos(_versionSeleccionada.Id, _libroSeleccionado.Id, c.Id).Count
            }).ToList();
            
            if (capitulos.Any())
            {
                cmbCapitulos.SelectedIndex = 0;
            }
            else
            {
                cmbCapitulos.ItemsSource = null;
                _capituloSeleccionado = null;
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

        private void CargarTitulos()
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null || _capituloSeleccionado == null) return;
            
            var titulos = _bibliaService.ObtenerTitulos(
                _versionSeleccionada.Id, 
                _libroSeleccionado.Id, 
                _capituloSeleccionado.Id);
                
            lstTitulos.ItemsSource = titulos;
            
            // Cargar versículos para el selector de posición
            var versiculos = _bibliaService.ObtenerVersiculos(
                _versionSeleccionada.Id,
                _libroSeleccionado.Id,
                _capituloSeleccionado.Id);
                
            cmbPosicionTitulo.ItemsSource = versiculos.OrderBy(v => v.Numero).ToList();
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
            
            // Cargar títulos del capítulo seleccionado
            CargarTitulos();
            
            // Limpiar los formularios
            LimpiarFormularioTitulo();
        }

        private void lstVersiculos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar/deshabilitar botones según haya selección
            btnEliminarVersiculo.IsEnabled = lstVersiculos.SelectedItem != null;
            btnEditarVersiculo.IsEnabled = lstVersiculos.SelectedItem != null;
        }

        private void lstTitulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tituloSeleccionado = lstTitulos.SelectedItem as TituloBiblico;
            
            btnEliminarTitulo.IsEnabled = _tituloSeleccionado != null;
            btnEditarTitulo.IsEnabled = _tituloSeleccionado != null;
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

        private void btnGuardarTitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null || _capituloSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar una versión, libro y capítulo.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (cmbPosicionTitulo.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar después de qué versículo aparecerá el título.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtTextoTitulo.Text))
            {
                MessageBox.Show("Debe ingresar el texto del título.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTextoTitulo.Focus();
                return;
            }
            
            try
            {
                var versiculo = cmbPosicionTitulo.SelectedItem as Versiculo;
                if (versiculo == null) return;
                
                if (_modoEdicionTitulo && _tituloSeleccionado != null)
                {
                    // Actualizar título existente
                    _tituloSeleccionado.Texto = txtTextoTitulo.Text.Trim();
                    _tituloSeleccionado.PosicionPrevia = versiculo.Numero;
                    
                    _bibliaService.ActualizarTitulo(_tituloSeleccionado);
                    MessageBox.Show("Título actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Crear nuevo título
                    var titulo = new TituloBiblico
                    {
                        Texto = txtTextoTitulo.Text.Trim(),
                        VersionId = _versionSeleccionada.Id,
                        LibroId = _libroSeleccionado.Id,
                        CapituloId = _capituloSeleccionado.Id,
                        PosicionPrevia = versiculo.Numero
                    };
                    
                    _bibliaService.AgregarTitulo(titulo);
                    MessageBox.Show("Título agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Limpiar y recargar
                LimpiarFormularioTitulo();
                CargarTitulos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar título: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiarTitulo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormularioTitulo();
        }

        private void LimpiarFormularioTitulo()
        {
            txtTextoTitulo.Text = string.Empty;
            cmbPosicionTitulo.SelectedIndex = -1;
            _modoEdicionTitulo = false;
            _tituloSeleccionado = null;
            btnGuardarTitulo.Content = "Guardar";
        }

        private void btnEliminarTitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_tituloSeleccionado == null) return;
            
            var mensaje = "¿Está seguro de que desea eliminar este título?";
            var resultado = MessageBox.Show(mensaje, "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _bibliaService.EliminarTitulo(_tituloSeleccionado.Id);
                    
                    MessageBox.Show("Título eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LimpiarFormularioTitulo();
                    CargarTitulos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar título: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditarTitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_tituloSeleccionado == null) return;
            
            _modoEdicionTitulo = true;
            btnGuardarTitulo.Content = "Actualizar";
            
            txtTextoTitulo.Text = _tituloSeleccionado.Texto;
            
            // Seleccionar el versículo correspondiente
            var versiculos = cmbPosicionTitulo.ItemsSource as IEnumerable<Versiculo>;
            if (versiculos != null)
            {
                var versiculo = versiculos.FirstOrDefault(v => v.Numero == _tituloSeleccionado.PosicionPrevia);
                if (versiculo != null)
                {
                    cmbPosicionTitulo.SelectedItem = versiculo;
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

        // Métodos para gestionar libros
        private void lstLibrosAdmin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _libroSeleccionadoAdmin = lstLibrosAdmin.SelectedItem as Libro;
            
            btnEliminarLibro.IsEnabled = _libroSeleccionadoAdmin != null;
            btnEditarLibro.IsEnabled = _libroSeleccionadoAdmin != null;
        }

        private void btnLimpiarLibro_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormularioLibro();
        }

        private void LimpiarFormularioLibro()
        {
            txtNumeroLibro.Text = string.Empty;
            txtNombreLibro.Text = string.Empty;
            txtAbreviaturaLibro.Text = string.Empty;
            
            _modoEdicionLibro = false;
            _libroSeleccionadoAdmin = null;
            btnGuardarLibro.Content = "Guardar";
        }

        private void btnGuardarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (_versionSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una versión.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtNumeroLibro.Text))
            {
                MessageBox.Show("Debe ingresar el número del libro.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumeroLibro.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtNombreLibro.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del libro.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreLibro.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtAbreviaturaLibro.Text))
            {
                MessageBox.Show("Debe ingresar la abreviatura del libro.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAbreviaturaLibro.Focus();
                return;
            }
            
            if (!int.TryParse(txtNumeroLibro.Text, out int numeroLibro) || numeroLibro <= 0)
            {
                MessageBox.Show("El número del libro debe ser un valor numérico positivo.", "Datos incorrectos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumeroLibro.Focus();
                return;
            }
            
            try
            {
                if (_modoEdicionLibro && _libroSeleccionadoAdmin != null)
                {
                    // Actualizar libro existente
                    _libroSeleccionadoAdmin.Numero = numeroLibro;
                    _libroSeleccionadoAdmin.Nombre = txtNombreLibro.Text.Trim();
                    _libroSeleccionadoAdmin.Abreviatura = txtAbreviaturaLibro.Text.Trim();
                    
                    _bibliaService.ActualizarLibro(_versionSeleccionada.Id, _libroSeleccionadoAdmin);
                    MessageBox.Show("Libro actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Crear nuevo libro
                    var libro = new Libro
                    {
                        Numero = numeroLibro,
                        Nombre = txtNombreLibro.Text.Trim(),
                        Abreviatura = txtAbreviaturaLibro.Text.Trim()
                    };
                    
                    _bibliaService.AgregarLibro(_versionSeleccionada.Id, libro);
                    MessageBox.Show("Libro agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Limpiar y recargar
                LimpiarFormularioLibro();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar libro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (_libroSeleccionadoAdmin == null) return;
            
            _modoEdicionLibro = true;
            btnGuardarLibro.Content = "Actualizar";
            
            txtNumeroLibro.Text = _libroSeleccionadoAdmin.Numero.ToString();
            txtNombreLibro.Text = _libroSeleccionadoAdmin.Nombre;
            txtAbreviaturaLibro.Text = _libroSeleccionadoAdmin.Abreviatura;
        }

        private void btnEliminarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (_libroSeleccionadoAdmin == null || _versionSeleccionada == null) return;
            
            var mensaje = $"¿Está seguro de que desea eliminar el libro {_libroSeleccionadoAdmin.Nombre}? Se eliminarán todos los capítulos y versículos asociados.";
            var resultado = MessageBox.Show(mensaje, "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _bibliaService.EliminarLibro(_versionSeleccionada.Id, _libroSeleccionadoAdmin.Id);
                    
                    MessageBox.Show("Libro eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LimpiarFormularioLibro();
                    CargarLibros();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar libro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Métodos para gestionar capítulos
        private void lstCapitulosAdmin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = lstCapitulosAdmin.SelectedItem;
            if (item != null)
            {
                // Como estamos usando un objeto anónimo para mostrar info adicional, extraemos el Id
                int capituloId = (int)item.GetType().GetProperty("Id").GetValue(item);
                _capituloSeleccionadoAdmin = _bibliaService.ObtenerCapituloPorId(
                    _versionSeleccionada?.Id ?? string.Empty, 
                    _libroSeleccionado?.Id ?? 0, 
                    capituloId);
            }
            else
            {
                _capituloSeleccionadoAdmin = null;
            }
            
            btnEliminarCapitulo.IsEnabled = _capituloSeleccionadoAdmin != null;
            btnEditarCapitulo.IsEnabled = _capituloSeleccionadoAdmin != null;
        }

        private void btnLimpiarCapitulo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormularioCapitulo();
        }

        private void LimpiarFormularioCapitulo()
        {
            txtNumeroCapitulo.Text = string.Empty;
            
            _modoEdicionCapitulo = false;
            _capituloSeleccionadoAdmin = null;
            btnGuardarCapitulo.Content = "Guardar";
        }

        private void btnGuardarCapitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_versionSeleccionada == null || _libroSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar una versión y un libro.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtNumeroCapitulo.Text))
            {
                MessageBox.Show("Debe ingresar el número del capítulo.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumeroCapitulo.Focus();
                return;
            }
            
            if (!int.TryParse(txtNumeroCapitulo.Text, out int numeroCapitulo) || numeroCapitulo <= 0)
            {
                MessageBox.Show("El número del capítulo debe ser un valor numérico positivo.", "Datos incorrectos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumeroCapitulo.Focus();
                return;
            }
            
            try
            {
                if (_modoEdicionCapitulo && _capituloSeleccionadoAdmin != null)
                {
                    // Actualizar capítulo existente
                    _capituloSeleccionadoAdmin.Numero = numeroCapitulo;
                    
                    _bibliaService.ActualizarCapitulo(_versionSeleccionada.Id, _libroSeleccionado.Id, _capituloSeleccionadoAdmin);
                    MessageBox.Show("Capítulo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Crear nuevo capítulo
                    var capitulo = new Capitulo
                    {
                        Numero = numeroCapitulo,
                        LibroId = _libroSeleccionado.Id
                    };
                    
                    _bibliaService.AgregarCapitulo(_versionSeleccionada.Id, _libroSeleccionado.Id, capitulo);
                    MessageBox.Show("Capítulo agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Limpiar y recargar
                LimpiarFormularioCapitulo();
                CargarCapitulos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar capítulo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditarCapitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_capituloSeleccionadoAdmin == null) return;
            
            _modoEdicionCapitulo = true;
            btnGuardarCapitulo.Content = "Actualizar";
            
            txtNumeroCapitulo.Text = _capituloSeleccionadoAdmin.Numero.ToString();
        }

        private void btnEliminarCapitulo_Click(object sender, RoutedEventArgs e)
        {
            if (_capituloSeleccionadoAdmin == null || _versionSeleccionada == null || _libroSeleccionado == null) return;
            
            var mensaje = $"¿Está seguro de que desea eliminar el capítulo {_capituloSeleccionadoAdmin.Numero}? Se eliminarán todos los versículos asociados.";
            var resultado = MessageBox.Show(mensaje, "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _bibliaService.EliminarCapitulo(
                        _versionSeleccionada.Id, 
                        _libroSeleccionado.Id, 
                        _capituloSeleccionadoAdmin.Id);
                    
                    MessageBox.Show("Capítulo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LimpiarFormularioCapitulo();
                    CargarCapitulos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar capítulo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}