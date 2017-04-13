using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RecognizeShapes.Controls
{
    public sealed partial class InkDevicesToolbar : UserControl
    {
        public InkCanvas TargetInkCanvas
        {
            get { return (InkCanvas)GetValue(TargetInkCanvasProperty); }
            set { SetValue(TargetInkCanvasProperty, value); }
        }

        public static readonly DependencyProperty TargetInkCanvasProperty =
            DependencyProperty.Register("TargetInkCanvas", typeof(InkCanvas), typeof(InkDevicesToolbar), new PropertyMetadata(null, setup));


        public InkDevicesToolbar()
        {
            this.InitializeComponent();
        }

        private static void setup(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            InkDevicesToolbar toolbar = obj as InkDevicesToolbar;
            InkCanvas canvas = args.NewValue as InkCanvas;

            if (canvas.InkPresenter.InputDeviceTypes == CoreInputDeviceTypes.None)
            {
                return;
            }

            toolbar.MouseButton.IsChecked = canvas.InkPresenter.InputDeviceTypes.HasFlag(CoreInputDeviceTypes.Mouse);
            toolbar.PenButton.IsChecked = canvas.InkPresenter.InputDeviceTypes.HasFlag(CoreInputDeviceTypes.Pen);
            toolbar.TouchButton.IsChecked = canvas.InkPresenter.InputDeviceTypes.HasFlag(CoreInputDeviceTypes.Touch);
        }

        private void NoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.PenButton.IsChecked = false;
            this.MouseButton.IsChecked = false;
            this.TouchButton.IsChecked = false;
        }

        private void PenButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes | CoreInputDeviceTypes.Pen;
        }

        private void MouseButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes | CoreInputDeviceTypes.Mouse;
        }

        private void TouchButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes | CoreInputDeviceTypes.Touch;
        }

        private void PenButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes ^ CoreInputDeviceTypes.Pen;
        }

        private void MouseButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes ^ CoreInputDeviceTypes.Mouse;
        }

        private void TouchButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TargetInkCanvas.InkPresenter.InputDeviceTypes = this.TargetInkCanvas.InkPresenter.InputDeviceTypes ^ CoreInputDeviceTypes.Touch;
        }
    }
}