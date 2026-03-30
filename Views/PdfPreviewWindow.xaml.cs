using System.Windows;
using System.Windows.Documents;

namespace Top5.Views
{
    public partial class PdfPreviewWindow : Window
    {
        public PdfPreviewWindow(FlowDocument document)
        {
            InitializeComponent();
            Viewer.Document = document;
        }

        // NOUVEAU : Le clic du bouton lance directement la boîte de dialogue d'impression
        private void BtnImprimer_Click(object sender, RoutedEventArgs e)
        {
            Viewer.Print();
        }
    }
}