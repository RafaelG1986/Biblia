using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                VersiculosListView.ItemsSource = null;
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
                    capitulo.Id);
                
                VersiculosListView.ItemsSource = versiculos.OrderBy(v => v.Numero).ToList();
            }
            else
            {
                TituloTextBlock.Text = string.Empty;
                VersiculosListView.ItemsSource = null;
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