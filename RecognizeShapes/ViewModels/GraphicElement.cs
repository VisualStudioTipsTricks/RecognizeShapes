using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace RecognizeShapes.ViewModels
{
    public class GraphicElement : ObservableObject
    {
        public UIElement Element { get; private set; }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value;
                base.RaisePropertyChanged();
            }
        }

        public GraphicElement(UIElement element)
        {
            this.Element = element;
        }
    }
}