using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Printing;

namespace Rxserver.View
{
    public partial class panel : Window
    {
        public panel()
        {
            InitializeComponent();
            CargarImpresoras();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CargarImpresoras()
        {
            printersActive.Items.Clear();
            var printServer = new LocalPrintServer();
            var printQueues = printServer.GetPrintQueues();
            var impresoraPredeterminada = LocalPrintServer.GetDefaultPrintQueue().Name;

            foreach (var printer in printQueues)
                printersActive.Items.Add(printer.Name);

            if (printersActive.Items.Contains(impresoraPredeterminada))
                printersActive.SelectedItem = impresoraPredeterminada;
            else if (printersActive.Items.Count > 0)
                printersActive.SelectedIndex = 0;
        }

        private void printSend_Click(object sender, RoutedEventArgs e)
        {
            var nombreImpresora = printersActive.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(nombreImpresora))
            {
                MessageBox.Show("Selecciona una impresora antes de imprimir.");
                return;
            }

            var documento = new FlowDocument(new Paragraph(new Run("prueba1")))
            {
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                PageWidth = 10
            };

            var paginador = (IDocumentPaginatorSource)documento;
            var servidor = new LocalPrintServer();
            var cola = servidor.GetPrintQueues().FirstOrDefault(p => p.Name == nombreImpresora);

            if (cola == null)
            {
                MessageBox.Show("No se encontró la impresora seleccionada.");
                return;
            }

            var dialogo = new PrintDialog { PrintQueue = cola };

            try
            {
                dialogo.PrintDocument(paginador.DocumentPaginator, "Prueba directa");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}");
            }
        }
    }
}