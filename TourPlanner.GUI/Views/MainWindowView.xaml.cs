using AdonisUI.Controls;
using System;
using System.Threading.Tasks;
using TourPlanner.GUI.ViewModels;

namespace TourPlanner.GUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : AdonisWindow
    {
        private readonly Func<Task<bool>> _vmCloseHandler;

        public MainWindowView()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel(() => DependencyInitializer.InitializeRealDataManager());
            DataContext = vm;
            _vmCloseHandler = vm.ExitApplicationWrapper;
        }

        private async void ClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
            => e.Cancel = await _vmCloseHandler();
    }
}
