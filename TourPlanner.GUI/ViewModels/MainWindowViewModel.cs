using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;
using log4net;

namespace TourPlanner.GUI.ViewModels
{
    public abstract class MainWindowViewModel : ViewModelBase
    {
        private bool _darkMode = false, _busy = false, _searchBarVisible = false, _includeDescChecked = false;
        private string _searchText = String.Empty;
        private Tour _selectedTour;
        private IDataManager _dm;
        private readonly ILog _log;

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
            set => SetProperty(ref _selectedTour, value);
        }

        public ObservableCollection<Tour> ShownTours { get; protected set; }

        public ICommand ResetConnectionCommand { get; }

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

        protected MainWindowViewModel()
        {
            IsBusy = true;
            _log = LogManager.GetLogger(typeof(MainWindowViewModel));
            ResetConnectionCommand = new RelayCommand(ResetConnection);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SwitchSearchBarVisibilityCommand = new RelayCommand(SwitchSearchBarVisibility);
            ExitApplicationCommand = new AsyncCommand(ExitApplication);
            AddTourCommand = new AsyncCommand(AddTour);
            DeleteTourCommand = new AsyncCommand(DeleteTour);
            AddTourLogCommand = new RelayCommand(AddTourLog);
            DeleteTourLogCommand = new RelayCommand(DeleteTourLog);
            ImportCommand = new AsyncCommand(ImportData);
            ExportCommand = new AsyncCommand(ExportData);
            GenerateSummaryReportCommand = new AsyncCommand(GenerateSummaryReport);
            GenerateTourReportCommand = new AsyncCommand(GenerateTourReport);
            DelayCommand = new AsyncCommand(Delay);
            ClearSearchTextCommand = new RelayCommand(ClearSearchText);
        }

        protected void FinishInitialization(IDataManager dataManager)
        {
            if (_dm is not null)
                throw new InvalidOperationException("Data manager has already been set.");

            _dm = dataManager;
            IsBusy = false;
        }

        protected void UpdateLoadedTours(IList<Tour> tours)
        {
            _dm.AllTours.Clear();

            if (_dm.AllTours is List<Tour> allTours)
            {
                allTours.AddRange(tours);
                return;
            }

            foreach (var tour in tours)
                _dm.AllTours.Add(tour);
        }

        private void ResetConnection()
            => _dm.Reinitialize();

        private void SwitchTheme()
        {
            IsDarkMode = !IsDarkMode;
            AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources,
                IsDarkMode ? AdonisUI.ResourceLocator.DarkColorScheme : AdonisUI.ResourceLocator.LightColorScheme);
        }

        private void SwitchSearchBarVisibility()
            => IsSearchBarVisible = !IsSearchBarVisible;

        private Task ExitApplication()
        {
            Application.Current.Shutdown();
            return Task.CompletedTask;
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
                var route = await _dm.CurrentDirectionsProvider.GetRoute(tourVM.StartLocation, tourVM.EndLocation);

                if (route is null) // not found
                {
                    _log.Error("No possible route was found for the given inputs.");
                    MessageBox.Show("No possible route could be found.", "Cannot add tour", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var imagePath = await _dm.CurrentMapImageProvider.GetImage(route);

                if (imagePath is null)
                {
                    _log.Warn("No image was found for the given route.");
                    MessageBox.Show("No image for the route could be loaded.", "Cannot load tour image", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                await _dm.SynchronizeTours();
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

        private async Task DeleteTour()
        {
            if (IsBusy)
                return;

            var selectedTour = SelectedTour;

            if (selectedTour is null)
                return;

            await BusySection(async () =>
            {
                _log.Info("Deleting tour with TourId=\"" + selectedTour.TourId + "\".");
                SelectedTour = null;
                _dm.AllTours.Remove(selectedTour);
                ShownTours.Remove(selectedTour);
                await _dm.SynchronizeTours();
            });
        }

        private void AddTourLog()
        {
            if (IsBusy)
                return;
        }

        private void DeleteTourLog()
        {
            if (IsBusy)
                return;
        }

        public async Task ImportData()
        {
            if (IsBusy)
                return;

            var path = OSInteraction.GetOpenFilePath("json", "JSON document (*.json)|*.json");

            if (String.IsNullOrEmpty(path))
                return;

            await BusySection(async () =>
            {
                _log.Info("Importing data from file \"" + path + "\".");
                var tours = await OSInteraction.ImportToursFromFile(path);
                UpdateLoadedTours(tours);
                UpdateShownTours();
                await _dm.SynchronizeTours();
            });
        }

        public async Task ExportData()
        {
            if (IsBusy)
                return;

            var path = OSInteraction.GetSaveFilePath(null, "json", "JSON document (*.json)|*.json");

            if (String.IsNullOrEmpty(path))
                return;

            await BusySection(async () =>
            {
                _log.Info("Exporting data to file \"" + path + "\".");
                await OSInteraction.ExportToursToFile(path, _dm.AllTours);
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
            if (IsBusy)
                return;

            if (SelectedTour is not Tour selectedTour)
                return;

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
            Func<Tour, bool> isContained = IsIncludeDescriptionChecked ? IsInNameOrDesc : IsInName;
            ShownTours.Clear();

            foreach (var tour in _dm.AllTours)
            {
                if (isContained(tour))
                    ShownTours.Add(tour);
            }

            bool IsInName(Tour tour) => tour.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            bool IsInNameOrDesc(Tour tour) => IsInName(tour) || tour.CustomDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private async Task BusySection(Func<Task> section)
        {
            IsBusy = true;
            await section();
            IsBusy = false;
        }
    }
}
