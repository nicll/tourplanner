﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TourPlanner.GUI.Views;

namespace TourPlanner.GUI.ViewModels
{
    public class AddTourViewModel : ViewModelBase, IDataErrorInfo
    {
        private string _name = String.Empty, _startLocation = String.Empty, _endLocation = String.Empty;

        /// <summary>
        /// A user-decided name for the tour.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetPropertyAndError(ref _name, value);
        }

        /// <summary>
        /// The address of the starting point.
        /// </summary>
        public string StartLocation
        {
            get => _startLocation;
            set => SetPropertyAndError(ref _startLocation, value);
        }

        /// <summary>
        /// The address of the ending point.
        /// </summary>
        public string EndLocation
        {
            get => _endLocation;
            set => SetPropertyAndError(ref _endLocation, value);
        }

        public bool HasNoError => String.IsNullOrEmpty(Error);

        public string Error => this[nameof(Name)] ?? this[nameof(StartLocation)] ?? this[nameof(EndLocation)];

        public string this[string columnName] => columnName switch
            {
                nameof(Name) => String.IsNullOrEmpty(Name) || Name.Length < 4 ? "Enter the name of the new tour." : null,
                nameof(StartLocation) => String.IsNullOrEmpty(StartLocation) || StartLocation.Length < 3 ? "Enter the address of the starting location." : null,
                nameof(EndLocation) => String.IsNullOrEmpty(EndLocation) || EndLocation.Length < 3 ? "Enter the address of the destination location." : null,
                _ => String.Empty
            };

        public ICommand FinishCommand { get; init; }

        public ICommand CancelCommand { get; init; }

        public AddTourViewModel()
        { }

        public AddTourViewModel(AddTourView view)
            => view.DataContext = this;

        private void SetPropertyAndError<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            SetProperty(ref storage, value, propertyName);
            OnPropertyChanged(nameof(HasNoError));
        }
    }
}
