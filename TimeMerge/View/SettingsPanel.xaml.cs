using System.Windows;
using TimeMerge.ViewModel;

namespace TimeMerge.View
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : Window
    {
        public SettingsPanel()
        {
            InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as TimeMerge.ViewModel.SettingsPanelViewModel;
            vm.CloseCommand = new RelayCommand(arg => this.Hide());
        }
    }
}
