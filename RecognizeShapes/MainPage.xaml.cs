using Windows.UI.Core;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Controls;

namespace RecognizeShapes
{
    public sealed partial class MainPage : Page
    {
        private InkAnalyzer inkAnalyzer = new InkAnalyzer();

        public MainPage()
        {
            this.InitializeComponent();
            WritingArea.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch;
        }

        private void ClearButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.canvas.Children.Clear();
        }
    }
}