using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using BibliaApp.Models;
using BibliaApp.Services;
using BibliaApp.Views;

namespace BibliaApp
{
    public partial class MainWindow : Window
    {
        private readonly BibliaService _bibliaService;
        private VersionBiblia? _versionActual;
        private Libro? _libroActual;
        private Capitulo? _capituloActual;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _bibliaService = new BibliaService();
            
            // Cargar versiones disponibles
            CargarVersiones();
        }
        
        private void CargarVersiones()
        {
            var versiones = _bibliaService.ObtenerVersiones();
            
            VersionesComboBox.ItemsSource = versiones;
            
            if (versiones.Any())
            {
                VersionesComboBox.SelectedIndex = 0;
            }
        }
        
        private void VersionesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _versionActual = VersionesComboBox.SelectedItem as VersionBiblia;
            
            if (_versionActual != null)
            {
                ActualizarListaLibros();
            }
        }
        
        private void ActualizarListaLibros()
        {
            if (_versionActual == null) return;
            
            var libros = _bibliaService.ObtenerLibros(_versionActual.Id);
            
            // Aplicar filtro si existe
            if (!string.IsNullOrWhiteSpace(FiltroTextBox.Text))
            {
                string filtro = FiltroTextBox.Text.ToLower();
                libros = libros.Where(l => 
                    l.Nombre.ToLower().Contains(filtro) || 
                    l.Abreviatura.ToLower().Contains(filtro)
                ).ToList();
            }
            
            LibrosListBox.ItemsSource = libros;
            
            if (libros.Any())
            {
                LibrosListBox.SelectedIndex = 0;
            }
            else
            {
                _libroActual = null;
                CargarCapitulos();
            }
        }
        
        private void LibrosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _libroActual = LibrosListBox.SelectedItem as Libro;
            
            CargarCapitulos();
        }
        
        private void CargarCapitulos()
        {
            if (_versionActual == null || _libroActual == null)
            {
                CapitulosComboBox.ItemsSource = null;
                VersiculosPanel.Children.Clear();
                TituloTextBlock.Text = string.Empty;
                return;
            }
            
            var capitulos = _bibliaService.ObtenerCapitulos(_versionActual.Id, _libroActual.Id);
            
            CapitulosComboBox.ItemsSource = capitulos;
            
            if (capitulos.Any())
            {
                CapitulosComboBox.SelectedIndex = 0;
            }
            else
            {
                _capituloActual = null;
                MostrarCapitulo(null);
            }
        }
        
        private void CapitulosComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _capituloActual = CapitulosComboBox.SelectedItem as Capitulo;
            
            MostrarCapitulo(_capituloActual);
        }
        
        private void MostrarCapitulo(Capitulo? capitulo)
        {
            if (_libroActual == null) return;
            
            if (capitulo != null)
            {
                TituloTextBlock.Text = $"{_libroActual.Nombre} {capitulo.Numero}";
                
                var versiculos = _bibliaService.ObtenerVersiculos(
                    _versionActual?.Id ?? string.Empty, 
                    _libroActual.Id, 
                    capitulo.Id)
                    .OrderBy(v => v.Numero)
                    .ToList();
                    
                // Obtener títulos para este capítulo
                var titulos = _bibliaService.ObtenerTitulos(
                    _versionActual?.Id ?? string.Empty,
                    _libroActual.Id,
                    capitulo.Id);
                    
                // Crear lista para visualización
                var stackPanel = new StackPanel();
                
                // Procesar versículos e insertar títulos en su posición
                int ultimoVersiculoProcesado = 0;
                
                foreach (var versiculo in versiculos)
                {
                    // Añadir títulos que van antes de este versículo
                    foreach (var titulo in titulos.Where(t => t.PosicionPrevia == ultimoVersiculoProcesado))
                    {
                        // Agregar título
                        var tituloTextBlock = new TextBlock
                        {
                            Text = titulo.Texto,
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            Margin = new Thickness(0, 10, 0, 5)
                        };
                        stackPanel.Children.Add(tituloTextBlock);
                    }
                    
                    // Añadir versículo
                    var versiculoPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 3, 0, 3)
                    };
                    
                    var numeroTextBlock = new TextBlock
                    {
                        Text = versiculo.Numero.ToString(),
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    
                    var textoTextBlock = new TextBlock
                    {
                        Text = versiculo.Texto,
                        TextWrapping = TextWrapping.Wrap
                    };
                    
                    versiculoPanel.Children.Add(numeroTextBlock);
                    versiculoPanel.Children.Add(textoTextBlock);
                    
                    stackPanel.Children.Add(versiculoPanel);
                    
                    ultimoVersiculoProcesado = versiculo.Numero;
                }
                
                // Añadir títulos al final si los hay
                foreach (var titulo in titulos.Where(t => t.PosicionPrevia == ultimoVersiculoProcesado))
                {
                    var tituloTextBlock = new TextBlock
                    {
                        Text = titulo.Texto,
                        FontWeight = FontWeights.Bold,
                        FontSize = 14,
                        Margin = new Thickness(0, 10, 0, 5)
                    };
                    stackPanel.Children.Add(tituloTextBlock);
                }
                
                // Crear ScrollViewer para permitir desplazamiento
                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.Content = stackPanel;
                
                // Actualizar el panel de versículos
                VersiculosPanel.Children.Clear();
                VersiculosPanel.Children.Add(scrollViewer);
            }
            else
            {
                TituloTextBlock.Text = string.Empty;
                VersiculosPanel.Children.Clear();
            }
        }
        
        private void FiltroTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarListaLibros();
        }
        
        private void MenuSalir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void MenuAgregarVersion_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarVersionWindow(_bibliaService);
            ventana.Owner = this;
            
            if (ventana.ShowDialog() == true)
            {
                CargarVersiones();
            }
        }
        
        private void MenuGestionarContenido_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GestionContenidoWindow(_bibliaService);
            ventana.Owner = this;
            
            if (ventana.ShowDialog() == true)
            {
                // Actualizar interfaz con posibles cambios
                CargarVersiones();
            }
        }
    }
}