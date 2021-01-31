using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PopularityRanking.WPF
{
    /// <summary>
    /// Interaction logic for MatchupWindow.xaml
    /// </summary>
    public partial class MatchupWindow : Window
    {
        public MatchupWindow()
        {
            DataContext = ViewModel.ranking;
            InitializeComponent();
            participantSlider.Value = 2;
        }
        public void UpdateComboBoxes()
        {
            foreach (ComboBox c in matchupPanel.Children)
                c.ItemsSource = ViewModel.ranking.Participants.OrderBy(p => p.Id);
        }

        private void participantSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsInitialized)
                return;

            if (participantSlider.Value > matchupPanel.Children.Count)
                IncreaseParticipants((int)participantSlider.Value - matchupPanel.Children.Count);
            else if (participantSlider.Value < matchupPanel.Children.Count)
                DecreaseParticipants(matchupPanel.Children.Count - (int)participantSlider.Value);
        }

        private void DecreaseParticipants(int v)
        {
            for (int i = 0; i < v; i++)
                matchupPanel.Children.RemoveAt(matchupPanel.Children.Count - 1);
        }

        private void IncreaseParticipants(int v)
        {
            for (int i = 0; i < v; i++)
                matchupPanel.Children.Add(CreateParticipantComboBox());
        }

        private ComboBox CreateParticipantComboBox()
        {
            var b = new ComboBox
            {
                ItemsSource = ViewModel.ranking.Participants.OrderBy(p => p.Id),
                Margin = new Thickness(0, 5, 0, 5),
            };

            b.SelectionChanged += this.Participant_SelectionChanged;

            return b;
        }

        private void Participant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessMatchupButton.IsEnabled = true;

            foreach (ComboBox c in matchupPanel.Children)
                if (c.SelectedIndex == -1)
                    ProcessMatchupButton.IsEnabled = false;
        }

        private void ProcessMatchupButton_Click(object sender, RoutedEventArgs e)
        {
            var matchup = new List<Participant>();

            foreach (ComboBox c in matchupPanel.Children)
                matchup.Add((Participant)c.SelectedValue);

            Matchup.RunAnyPlayersMatchup(matchup.ToArray());

            foreach (ComboBox c in matchupPanel.Children)
                c.SelectedIndex = -1;
        }
    }
}
