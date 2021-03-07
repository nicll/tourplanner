using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TourPlanner.GUI.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _exampleCounter = 0;
        private string _measuredText = "...";

        public int ExampleCounter
        {
            get => _exampleCounter;
            set
            {
                _exampleCounter = value;
                OnPropertyChanged(nameof(ExampleCounter)); // this property changed
                OnPropertyChanged(nameof(ExampleText)); // dependent property also changed!
            }
        }

        public string ExampleText => "The button was pressed " + ExampleCounter + " time(s).";

        public string MeasuredText
        {
            get => _measuredText;
            set
            {
                _measuredText = value;
                OnPropertyChanged(nameof(MeasuredText)); // this property changed
                OnPropertyChanged(nameof(MeasuredTextLength)); // dependent property changed
            }
        }

        public int MeasuredTextLength => MeasuredText.Length;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ButtonCommand { get; }

        public MainWindowViewModel()
        {
            ButtonCommand = new RelayCommand(() => ++ExampleCounter);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
