using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace RecognizeShapes.Helpers
{
    public class RecognitionEventArgs : EventArgs
    {
        public UIElement Element { get; private set; }
        public string Description { get; set; }

        public RecognitionEventArgs(UIElement element)
        {
            this.Element = element;
        }
    }
}