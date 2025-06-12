using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;



namespace Rxserver.View
{
    public partial class panel : Window
    {
        public panel()
        {
            InitializeComponent();
            CargarImpresoras();
            var datos = new Dictionary<string, string>
            {
                { "FechaText", "12/06/2025" },
                { "HoraText", "14:50" },
                { "ProductoCantidadText", "2" },
                { "ProductoNombreText", "Chow Mein" },
                { "ProductoPrecioText", "240" },
                { "TotalText", "480" },
                { "FormaPagoText", "Efectivo" },
                { "ClienteText", "Luis Martínez" },
                { "TelefonoText", "55-11-22-33-44" }
            };
            RenderTicket(datos);
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

        
        public void RenderTicket(Dictionary<string, string> valores)
        {
            foreach (var kvp in valores)
            {
                if (FindName(kvp.Key) is TextBlock tb)
                {
                    tb.Text = kvp.Value;
                }
            }
        }


        private void printSend_Click(object sender, RoutedEventArgs e)
        {
            var nombreImpresora = printersActive.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(nombreImpresora))
            {
                MessageBox.Show("Selecciona una impresora válida.");
                return;
            }

            // Crear una copia visual en un contenedor temporal
            var contenedor = new Grid
            {
                Width = 302, // 80mm ≈ 302px
                Background = System.Windows.Media.Brushes.White
            };

            // Clonar TicketRoot para evitar moverlo del layout original
            var ticketClonado = ClonarElemento(TicketRoot);
            contenedor.Children.Add(ticketClonado);

            // Forzar medida
            contenedor.Measure(new System.Windows.Size(302, double.PositiveInfinity));
            contenedor.Arrange(new System.Windows.Rect(0, 0, 302, contenedor.DesiredSize.Height));
            contenedor.UpdateLayout();

            // Crear página fija de impresión
            var fixedPage = new FixedPage
            {
                Width = 302,
                Height = contenedor.DesiredSize.Height
            };
            FixedPage.SetLeft(contenedor, 0);
            FixedPage.SetTop(contenedor, 0);
            fixedPage.Children.Add(contenedor);

            // Documento paginado
            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);
            var documento = new FixedDocument();
            documento.Pages.Add(pageContent);

            // Imprimir en la impresora seleccionada
            var printServer = new LocalPrintServer();
            var cola = printServer.GetPrintQueues().FirstOrDefault(p => p.Name == nombreImpresora);
            if (cola == null)
            {
                MessageBox.Show("La impresora no está disponible.");
                return;
            }

            var dialogo = new PrintDialog { PrintQueue = cola };

            try
            {
                dialogo.PrintDocument(documento.DocumentPaginator, "Ticket");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}");
            }
        }

        private UIElement ClonarElemento(UIElement original)
        {
            string xaml = System.Windows.Markup.XamlWriter.Save(original);
            var reader = new System.IO.StringReader(xaml);
            var xmlReader = System.Xml.XmlReader.Create(reader);
            return (UIElement)System.Windows.Markup.XamlReader.Load(xmlReader);
        }

        private void previewPrintSend_Click(object sender, RoutedEventArgs e)
        {
            // Crear contenedor temporal
            var contenedor = new Grid
            {
                Width = 302,
                Background = System.Windows.Media.Brushes.White
            };

            // Clonar visual
            var ticketClonado = ClonarElemento(TicketRoot);
            contenedor.Children.Add(ticketClonado);

            contenedor.Measure(new Size(302, double.PositiveInfinity));
            contenedor.Arrange(new Rect(0, 0, 302, contenedor.DesiredSize.Height));
            contenedor.UpdateLayout();

            // Crear página fija
            var fixedPage = new FixedPage
            {
                Width = 302,
                Height = contenedor.DesiredSize.Height
            };
            FixedPage.SetLeft(contenedor, 0);
            FixedPage.SetTop(contenedor, 0);
            fixedPage.Children.Add(contenedor);

            // Documento paginado
            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);
            var documento = new FixedDocument();
            documento.Pages.Add(pageContent);

            // Mostrar vista previa
            var previewWindow = new Window
            {
                Title = "Vista previa de impresión",
                Width = 350,
                Height = 600,
                Content = new DocumentViewer
                {
                    Document = documento
                },
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            previewWindow.ShowDialog();
        }

    }
}