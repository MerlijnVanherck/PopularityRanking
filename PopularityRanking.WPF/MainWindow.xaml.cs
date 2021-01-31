using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PopularityRanking.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MatchupWindow matchupWindow;

        public MainWindow()
        {
            DataContext = ViewModel.ranking;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ranking is null)
                OpenRanking();
            else
                CloseRanking();
        }

        private void CloseRanking()
        {
            var result = MessageBox.Show(
                "If you don't save the ranking before closing it, any changes you made will be lost.",
                "Save ranking before closing?",
                MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
                XmlFile.Export(PathName(), ViewModel.ranking);

            if (matchupWindow != null)
            {
                matchupWindow.Close();
                matchupWindow = null;
            }

            ViewModel.ranking = null;
            rankingFileButton.Content = "Open ranking";
            rankingFileName.IsEnabled = true;
            matchupButton.IsEnabled = false;
            addParticipantButton.IsEnabled = false;
            UpdateRankingGrid();
        }

        private void OpenRanking()
        {
            try
            {
                ViewModel.ranking = XmlFile.Import<Ranking>(PathName());
                rankingFileButton.Content = "Close ranking";
                rankingFileName.IsEnabled = false;
                matchupButton.IsEnabled = true;
                addParticipantButton.IsEnabled = true;
                UpdateRankingGrid();
            }
            catch (Exception ex)
            {
                DisplayError("Failed to import ranking", ex.ToString());
            }
        }

        private void UpdateRankingGrid()
        {
            rankingGrid.ItemsSource = null;
            rankingGrid.ItemsSource = ViewModel.ranking?.Participants;
        }

        private string PathName()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());
            var path = files.FirstOrDefault(f => f.EndsWith(rankingFileName.Text + ".xml"));
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Directory.GetCurrentDirectory() + "\\rankings\\" + rankingFileName.Text + ".xml";
            }

            return path;
        }

        private void rankingName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rankingFileName.Text))
                rankingFileButton.IsEnabled = false;
            else
                rankingFileButton.IsEnabled = true;
        }

        private void DisplayError(string title, string error)
        {
            MessageBox.Show(error, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void matchupButton_Click(object sender, RoutedEventArgs e)
        {
            matchupButton.IsEnabled = false;
            matchupWindow = new MatchupWindow();
            matchupWindow.Closing += MatchupWindow_Closing;
            matchupWindow.Show();
        }
        private void MatchupWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            matchupButton.IsEnabled = true;
            matchupWindow = null;
        }

        private void addParticipantButton_Click(object sender, RoutedEventArgs e)
        {
            if ((!int.TryParse(idBox.Text, out int id)) || string.IsNullOrWhiteSpace(nameBox.Text))
            {
                DisplayError("An id and name are required.", "Please enter a valid id and name in the textboxes.");
                return;
            }

            ViewModel.ranking.Participants.Add(new Participant
            {
                Id = id,
                Name = nameBox.Text
            });

            UpdateRankingGrid();

            if (matchupWindow != null)
                matchupWindow.UpdateComboBoxes();

            idBox.Text = "";
            nameBox.Text = "";
        }
    }
}
