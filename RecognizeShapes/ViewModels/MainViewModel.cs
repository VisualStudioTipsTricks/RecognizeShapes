﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using RecognizeShapes.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace RecognizeShapes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public event EventHandler GraphicElementDeleteRequested;

        public ObservableCollection<GraphicElement> Elements { get; set; }

        private GraphicElement selected;
        public GraphicElement Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                base.RaisePropertyChanged();
                this.DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ResetCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand CircleCommand { get; set; }
        public RelayCommand SquareCommand { get; set; }

        public MainViewModel()
        {
            this.ResetCommand = new RelayCommand(resetCommandExecute);
            this.DeleteCommand = new RelayCommand(deleteCommandExecute, deleteCommandCanExecute);
            this.CircleCommand = new RelayCommand(circleCommandExecute);
            this.SquareCommand = new RelayCommand(squareCommandExecute);
            this.Elements = new ObservableCollection<GraphicElement>();
            AutoRecognizeShapes.RecognitionOccured += AutoRecognizeShapes_RecognitionOccured;
        }

        private async void squareCommandExecute()
        {
            MessageDialog dlg = new MessageDialog("You've drawn a square!");
            await dlg.ShowAsync();
        }

        private async void circleCommandExecute()
        {
            MessageDialog dlg = new MessageDialog("You've drawn a circle!");

            await dlg.ShowAsync();
        }

        private bool deleteCommandCanExecute()
        {
            return this.Selected != null;
        }

        private void deleteCommandExecute()
        {
            if (this.Elements.Contains(this.Selected))
            {
                if (this.GraphicElementDeleteRequested != null)
                {
                    this.GraphicElementDeleteRequested(this, EventArgs.Empty);
                }

                var ge = this.Elements.Remove(this.Selected);
                this.Selected = this.Elements.FirstOrDefault();
                this.DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private void resetCommandExecute()
        {
            this.Elements.Clear();
        }

        private void AutoRecognizeShapes_RecognitionOccured(object sender, RecognitionEventArgs e)
        {
            GraphicElement element = new GraphicElement(e.Element);
            element.Description = e.Description;

            this.Elements.Insert(0, element);
            this.Selected = element;
        }
    }
}