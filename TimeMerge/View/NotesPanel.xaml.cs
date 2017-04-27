using System.Windows;
using TimeMerge.ViewModel;

namespace TimeMerge.View
{
    /// <summary>
    /// Interaction logic for NotesPanel.xaml
    /// </summary>
    public partial class NotesPanel : Window
    {
        public NotesPanel()
        {
            InitializeComponent();

            this.DataContextChanged += NotesPanel_DataContextChanged;
        }

        private void NotesPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as TimeMerge.ViewModel.NotesPanelViewModel;
            vm.CloseCommand = new RelayCommand(arg => this.Hide());
        }
    }
}
