using RecognizeShapes.ViewModels;
using Windows.UI.Core;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Controls;

namespace RecognizeShapes
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = this.Resources["viewmodel"] as MainViewModel;
            this.ViewModel.GraphicElementDeleteRequested += ViewModel_GraphicElementDeleteRequested;
        }

        private void ViewModel_GraphicElementDeleteRequested(object sender, System.EventArgs e)
        {
            this.canvas.Children.Remove(this.ViewModel.Selected.Element);
        }

        private void ClearButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.canvas.Children.Clear();
            this.ViewModel.ResetCommand.Execute(null);
        }
    }
}