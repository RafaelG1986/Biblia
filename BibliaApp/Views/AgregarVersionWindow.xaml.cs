using System;
using System.Linq;
using System.Windows;
using BibliaApp.Models;
using BibliaApp.Services;

namespace BibliaApp.Views
{
    public partial class AgregarVersionWindow : Window
    {
        private readonly BibliaService _bibliaService;
        private readonly VersionBiblia? _versionExistente;
        
        public AgregarVersionWindow(BibliaService bibliaService, VersionBiblia? version = null)
        {
            InitializeComponent();
            
            _bibliaService = bibliaService;
            _versionExistente = version;
            
            // Configurar título de la ventana
            Title = _versionExistente == null ? "Agregar Versión" : "Editar Versión";
            
            // Cargar datos si es edición
            if (_versionExistente != null)
            {
                IdTextBox.Text = _versionExistente.Id;
                IdTextBox.IsEnabled = false; // No permitir cambiar el ID
                
                NombreTextBox.Text = _versionExistente.Nombre;
                IdiomaTextBox.Text = _versionExistente.Idioma;
                DescripcionTextBox.Text = _versionExistente.Descripcion;
            }
        }
        
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar datos
            if (string.IsNullOrWhiteSpace(IdTextBox.Text))
            {
                MessageBox.Show("Debe ingresar un ID para la versión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IdTextBox.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                MessageBox.Show("Debe ingresar un nombre para la versión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NombreTextBox.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(IdiomaTextBox.Text))
            {
                MessageBox.Show("Debe ingresar el idioma de la versión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IdiomaTextBox.Focus();
                return;
            }
            
            try
            {
                // Crear nueva versión o actualizar existente
                var version = new VersionBiblia
                {
                    Id = IdTextBox.Text.Trim(),
                    Nombre = NombreTextBox.Text.Trim(),
                    Idioma = IdiomaTextBox.Text.Trim(),
                    Descripcion = DescripcionTextBox.Text?.Trim() ?? string.Empty
                };
                
                if (_versionExistente == null)
                {
                    // Verificar que no exista ya una versión con el mismo ID
                    var versiones = _bibliaService.ObtenerVersiones();
                    if (versiones.Any(v => v.Id.Equals(version.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"Ya existe una versión con el ID '{version.Id}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        IdTextBox.Focus();
                        return;
                    }
                    
                    _bibliaService.AgregarVersion(version);
                    MessageBox.Show("Versión agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _bibliaService.ActualizarVersion(version);
                    MessageBox.Show("Versión actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar versión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}