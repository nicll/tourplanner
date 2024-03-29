﻿using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;
using log4net;
using TourPlanner.Core.Exceptions;
using AdonisUI.Controls;

namespace TourPlanner.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILog _log;
        private bool _darkMode = false, _busy = false, _searchBarVisible = false, _includeDescChecked = false;
        private string _searchText = String.Empty;
        private IDataManager _dm;
        private Tour _selectedTour;
        private ListCollectionView _selectedLog;
        private LogEntry _selectedLogEntry;

        public bool IsDarkMode
        {
            get => _darkMode;
            set => SetProperty(ref _darkMode, value);
        }

        public bool IsBusy
        {
            get => _busy;
            private set => SetProperty(ref _busy, value);
        }

        public bool IsSearchBarVisible
        {
            get => _searchBarVisible;
            set => SetProperty(ref _searchBarVisible, value);
        }

        public bool IsIncludeDescriptionChecked
        {
            get => _includeDescChecked;
            set
            {
                SetProperty(ref _includeDescChecked, value);
                UpdateShownTours();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                UpdateShownTours();
            }
        }

        public Tour SelectedTour
        {
            get => _selectedTour;
            set
            {
                SetProperty(ref _selectedTour, value);
                SelectedTourLog = new ListCollectionView(value?.Log
                    ?? (System.Collections.IList)Array.Empty<LogEntry>());
                SelectedTourLogEntry = null;
            }
        }

        public ListCollectionView SelectedTourLog
        {
            get => _selectedLog;
            set => SetProperty(ref _selectedLog, value);
        }

        public LogEntry SelectedTourLogEntry
        {
            get => _selectedLogEntry;
            set => SetProperty(ref _selectedLogEntry, value);
        }

        public ObservableCollection<Tour> ShownTours { get; protected set; } = new();

        public ICommand ResetConnectionCommand { get; }

        public ICommand SynchronizeCommand { get; }

        public ICommand CleanupCommand { get; }

        public ICommand SwitchThemeCommand { get; }

        public ICommand SwitchSearchBarVisibilityCommand { get; }

        public ICommand ExitApplicationCommand { get; }

        public ICommand AddTourCommand { get; }

        public ICommand DeleteTourCommand { get; }

        public ICommand AddTourLogCommand { get; }

        public ICommand DeleteTourLogCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand GenerateSummaryReportCommand { get; }

        public ICommand GenerateTourReportCommand { get; }

        public ICommand DelayCommand { get; }

        public ICommand ClearSearchTextCommand { get; }

        public MainWindowViewModel(Func<Task<IDataManager>> dataManagerInitializer) : this(default(IDataManager))
            => Task.Run(async () => FinishInitialization(await dataManagerInitializer()));

        public MainWindowViewModel(IDataManager dataManager)
        {
            IsBusy = true;
            _log = LogManager.GetLogger(typeof(MainWindowViewModel));
            ResetConnectionCommand = new AsyncCommand(ResetConnection);
            SynchronizeCommand = new AsyncCommand(Synchronize);
            CleanupCommand = new AsyncCommand(Cleanup);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SwitchSearchBarVisibilityCommand = new RelayCommand(SwitchSearchBarVisibility);
            ExitApplicationCommand = new AsyncCommand(ExitApplication);
            AddTourCommand = new AsyncCommand(AddTour);
            DeleteTourCommand = new RelayCommand(DeleteTour);
            AddTourLogCommand = new RelayCommand(AddTourLog);
            DeleteTourLogCommand = new RelayCommand(DeleteTourLog);
            ImportCommand = new AsyncCommand(ImportData);
            ExportCommand = new AsyncCommand(ExportData);
            GenerateSummaryReportCommand = new AsyncCommand(GenerateSummaryReport);
            GenerateTourReportCommand = new AsyncCommand(GenerateTourReport);
            DelayCommand = new AsyncCommand(Delay);
            ClearSearchTextCommand = new RelayCommand(ClearSearchText);

            if (dataManager is not null)
                FinishInitialization(dataManager);
        }

        protected void FinishInitialization(IDataManager dataManager)
        {
            if (_dm is not null)
                throw new InvalidOperationException("Data manager has already been set.");

            _dm = dataManager;
            UpdateShownTours();
            IsBusy = false;
        }

        protected void OverwriteTours(ICollection<Tour> tours)
        {
            _dm.AllTours.Clear();

            foreach (var tour in tours)
                _dm.AllTours.Add(tour);
        }

        private async Task ResetConnection()
        {
            if (IsBusy)
                return;

            try
            {
                await BusySection(_dm.Reinitialize);
                UpdateShownTours();
            }
            catch (TourPlannerException ex)
            {
                _log.Error("Could not reset connection due to error.", ex);
                MessageBox.Show(App.Current.MainWindow, "An error occured while resetting the connection.", "Connection reset failed", MessageBoxButton.OK, MessageBoxImage.Error);
                IsBusy = false;
            }
        }

        private async Task Synchronize()
        {
            if (IsBusy)
                return;

            try { await BusySection(_dm.SynchronizeTours); }
            catch (DatabaseException ex)
            {
                _log.Error("Could not synchronize changes with the database.", ex);
                MessageBox.Show(App.Current.MainWindow, "Could not synchronize changes with the database.", "Synchronization failed", MessageBoxButton.OK, MessageBoxImage.Error);
                IsBusy = false;
            }
        }

        private async Task Cleanup()
        {
            if (IsBusy)
                return;

            try
            {
                await BusySection(_dm.CleanCache);
            }
            catch (DataProviderExcpetion ex)
            {
                _log.Error("Cache cleanup finished with error.", ex);
                MessageBox.Show(App.Current.MainWindow, "An error occured while cleaning the cache.", "Error during cleanup", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsBusy = false;
            }
        }

        private void SwitchTheme()
        {
            IsDarkMode = !IsDarkMode;
            AdonisUI.ResourceLocator.SetColorScheme(App.Current.Resources,
                IsDarkMode ? AdonisUI.ResourceLocator.DarkColorScheme : AdonisUI.ResourceLocator.LightColorScheme);
        }

        private void SwitchSearchBarVisibility()
            => IsSearchBarVisible = !IsSearchBarVisible;

        public async Task<bool> ExitApplicationWrapper()
            => await ExitApplication();

        private async Task<bool> ExitApplication()
        {
            if (IsBusy)
                return true;

            if (_dm.AllTours.IsChanged)
            {
                var msgBoxModel = new MessageBoxModel()
                {
                    Caption = "Save changes?",
                    Icon = MessageBoxImage.Question,
                    IsSoundEnabled = true,
                    Text = "Do you want to synchronize any unsaved changes?",
                    Buttons = new[]
                    {
                        new MessageBoxButtonModel("Yes", MessageBoxResult.Yes),
                        new MessageBoxButtonModel("No", MessageBoxResult.No),
                        new MessageBoxButtonModel("Cancel", MessageBoxResult.Cancel)
                    }
                };

                MessageBox.Show(App.Current.MainWindow, msgBoxModel);

                switch (msgBoxModel.Result)
                {
                    case MessageBoxResult.Yes:
                        try { await BusySection(_dm.SynchronizeTours); }
                        catch (DatabaseException ex) { _log.Error("Could not synchronize changes with the database.", ex); }
                        break;
                    case MessageBoxResult.Cancel:
                        return true;
                }
            }

            App.Current.Shutdown();
            return false;
        }

        private async Task AddTour()
        {
            if (IsBusy)
                return;

            var window = new Views.AddTourView();
            var tourVM = new AddTourViewModel(window)
            {
                CancelCommand = new RelayCommand(Cancel),
                FinishCommand = new RelayCommand(Finish)
            };

            if (window.ShowDialog() is not true) // false or null
                return;

            await BusySection(async () =>
            {
                Route route = null;
                try { route = await _dm.CurrentDirectionsProvider.GetRoute(tourVM.StartLocation, tourVM.EndLocation); }
                catch (DataProviderExcpetion ex)
                {
                    _log.Error("Route could not be loaded from data provider.", ex);
                    MessageBox.Show(App.Current.MainWindow, "Route could not be loaded from data provider.", "Cannot add tour", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (route is null) // no error but no result
                {
                    MessageBox.Show("No possible route could be found.", "No route", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                string imagePath = null;
                try { imagePath = await _dm.CurrentMapImageProvider.GetImage(route); }
                catch (DataProviderExcpetion ex) { _log.Error("An error occured while download the image for route: " + route.RouteId, ex); }

                if (imagePath is null)
                {
                    imagePath = String.Empty;
                    _log.Warn("No image was found for the given route.");
                    MessageBox.Show(App.Current.MainWindow, "No image for the route could be loaded.", "Cannot load tour image", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var newTour = new Tour
                {
                    TourId = Guid.NewGuid(),
                    Name = tourVM.Name,
                    CustomDescription = String.Empty,
                    ImagePath = imagePath,
                    Route = route
                };

                _log.Info("Adding new tour with TourId=\"" + newTour.TourId + "\".");
                _dm.AllTours.Add(newTour);
                UpdateShownTours();
            });

            void Cancel()
            {
                window.DialogResult = false;
                window.Close();
            }

            void Finish()
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void DeleteTour()
        {
            if (IsBusy)
                return;

            var selectedTour = SelectedTour;

            if (selectedTour is null)
                return;

            _log.Info("Deleting tour with TourId=\"" + selectedTour.TourId + "\".");
            SelectedTour = null;
            _dm.AllTours.Remove(selectedTour);
            ShownTours.Remove(selectedTour);
        }

        private void AddTourLog()
        {
            if (IsBusy || SelectedTour is null)
                return;

            var newEntry = new LogEntry { LogId = Guid.NewGuid(), Date = DateTime.Today, ParticipantCount = 1 };
            SelectedTourLog.AddNewItem(newEntry);
            SelectedTourLog.CommitNew();
            SelectedTourLogEntry = newEntry;
        }

        private void DeleteTourLog()
        {
            if (IsBusy || SelectedTour is null || SelectedTourLogEntry is null)
                return;

            var deletedEntry = SelectedTourLogEntry;
            SelectedTourLogEntry = null;
            SelectedTourLog.Remove(deletedEntry);
        }

        public async Task ImportData()
        {
            if (IsBusy)
                return;

            var path = OSInteraction.GetOpenFilePath(_dm.DataMigrator.PreferredFileExtension, _dm.DataMigrator.FileFilter);

            if (String.IsNullOrEmpty(path))
                return;

            await BusySection(async () =>
            {
                _log.Info("Importing data from file \"" + path + "\".");
                var tours = await OSInteraction.ImportToursFromFile(path, _dm.DataMigrator);

                if (tours is null)
                {
                    _log.Error("Importing data failed.");
                    MessageBox.Show(App.Current.MainWindow, "Could not import data.", "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OverwriteTours(tours);
                UpdateShownTours();
            });
        }

        public async Task ExportData()
        {
            if (IsBusy)
                return;

            var path = OSInteraction.GetSaveFilePath(null, _dm.DataMigrator.PreferredFileExtension, _dm.DataMigrator.FileFilter);

            if (String.IsNullOrEmpty(path))
                return;

            await BusySection(async () =>
            {
                _log.Info("Exporting data to file \"" + path + "\".");
                await OSInteraction.ExportToursToFile(path, _dm.AllTours, _dm.DataMigrator);
            });
        }

        private async Task GenerateSummaryReport()
        {
            if (IsBusy)
                return;

            _log.Debug("Generating summary report.");
            var savePath = GetReportSavePath("summary");

            if (String.IsNullOrEmpty(savePath))
                return;

            await BusySection(async () =>
            {
                _log.Debug("User selected \"" + savePath + "\" for summary report.");
                await _dm.ReportGenerator.GenerateSummaryReport(_dm.AllTours, savePath);
                OSInteraction.ShowFile(savePath);
                _log.Info("Generated summary report: " + savePath);
            });
        }

        private async Task GenerateTourReport()
        {
            if (IsBusy || SelectedTour is null)
                return;

            var selectedTour = SelectedTour;
            _log.Debug("Generating tour report for tour \"" + selectedTour.TourId + "\".");
            var savePath = GetReportSavePath(selectedTour.Name);

            if (String.IsNullOrEmpty(savePath))
                return;

            await BusySection(async () =>
            {
                _log.Debug("User selected \"" + savePath + "\" for tour report for tour \"" + selectedTour.TourId + "\".");
                await _dm.ReportGenerator.GenerateTourReport(selectedTour, savePath);
                OSInteraction.ShowFile(savePath);
                _log.Info("Generated tour report \"" + savePath + "\" for tour \"" + selectedTour.TourId + "\".");
            });
        }

        private async Task Delay()
            => await BusySection(async () => await Task.Delay(5000)); // simulate 5s wait time

        private static string GetReportSavePath(string suggestedName)
            => OSInteraction.GetSaveFilePath(suggestedName, "pdf", "Portable document files (*.pdf)|*.pdf");

        private void ClearSearchText()
            => SearchText = String.Empty;

        private void UpdateShownTours()
        {
            if (_dm?.AllTours is null)
                return;

            Func<Tour, bool> isContained = IsIncludeDescriptionChecked ? IsInNameOrDescOrNotes : IsInName;
            App.Current.Dispatcher.Invoke(() => ShownTours.Clear());

            foreach (var tour in _dm.AllTours)
            {
                if (isContained(tour))
                    App.Current.Dispatcher.Invoke(() => ShownTours.Add(tour));
            }

            bool IsInName(Tour tour) => tour.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            bool IsInNameOrDescOrNotes(Tour tour)
                => IsInName(tour)
                || tour.CustomDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || tour.Log.Any(l => l.Notes.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        private async Task BusySection(Func<Task> section)
        {
            IsBusy = true;
            await section();
            IsBusy = false;
        }
    }
}
