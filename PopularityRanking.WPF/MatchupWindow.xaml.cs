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
        private MainWindow mainWindow;
        public MatchupWindow(MainWindow window)
        {
            mainWindow = window;
            DataContext = ViewModel.ranking;
            InitializeComponent();
            participantSlider.Value = 2;
        }
        public void UpdateComboBoxes()
        {
            var list = ViewModel.ranking.Participants.OrderBy(p => p.Id);
            foreach (Grid g in matchupPanel.Children)
                ((ComboBox)g.Children[2]).ItemsSource = list;
        }

        public void ChooseRandomParticipants()
        {
            var random = new Random();
            int number = matchupPanel.Children.Count;
            var list = new List<int>(number);

            while (list.Count != number)
            {
                var temp = random.Next() % ViewModel.ranking.Participants.Count;
                if (!list.Contains(temp))
                    list.Add(temp);
            }

            for (int i = 0; i < number; i++)
                ((ComboBox)((Grid)(matchupPanel.Children[i])).Children[2]).SelectedIndex = list[i];
        }

        private void ParticipantSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
                matchupPanel.Children.Add(CreateParticipantPanel());

            ProcessMatchupButton.IsEnabled = false;
        }

        private Grid CreateParticipantPanel()
        {
            var comboBox = new ComboBox
            {
                ItemsSource = ViewModel.ranking.Participants.OrderBy(p => p.Id),
                Margin = new Thickness(0, 3, 0, 3),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            comboBox.SelectionChanged += this.Participant_SelectionChanged;

            Grid.SetColumn(comboBox, 1);
            Grid.SetRow(comboBox, 0);
            Grid.SetRowSpan(comboBox, 2);

            var upArrow = new Button
            {
                Content = "▲",
                FontSize = 6,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = 25,
                Height = 12,
                Padding = new Thickness(0)
            };

            upArrow.Click += this.UpArrow_Click;

            Grid.SetColumn(upArrow, 0);
            Grid.SetRow(upArrow, 0);

            var downArrow = new Button
            {
                Content = "▼",
                FontSize = 6,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 25,
                Height = 12,
                Padding = new Thickness(0)
            };

            downArrow.Click += this.DownArrow_Click;

            Grid.SetColumn(downArrow, 0);
            Grid.SetRow(downArrow, 1);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });

            grid.Children.Add(upArrow);
            grid.Children.Add(downArrow);
            grid.Children.Add(comboBox);

            return grid;
        }

        private void DownArrow_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = (Grid)button.Parent;

            var index = matchupPanel.Children.IndexOf(grid);

            if (index == matchupPanel.Children.Count - 1)
                return;

            matchupPanel.Children.RemoveAt(index);
            matchupPanel.Children.Insert(index + 1, grid);
        }

        private void UpArrow_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = (Grid)button.Parent;

            var index = matchupPanel.Children.IndexOf(grid);

            if (index == 0)
                return;

            matchupPanel.Children.RemoveAt(index);
            matchupPanel.Children.Insert(index - 1, grid);
        }

        private void Participant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessMatchupButton.IsEnabled = false;

            var selections = new List<int>();
            foreach (Grid c in matchupPanel.Children)
                selections.Add(((ComboBox)c.Children[2]).SelectedIndex);

            if (selections.Distinct().Count() == selections.Count
                && !selections.Contains(-1))
                ProcessMatchupButton.IsEnabled = true;
        }

        private void ProcessMatchupButton_Click(object sender, RoutedEventArgs e)
        {
            var matchup = new List<Participant>();

            foreach (Grid c in matchupPanel.Children)
                matchup.Add((Participant)((ComboBox)c.Children[2]).SelectedValue);

            Matchup.RunAnyPlayersMatchup(matchup.ToArray());
            ViewModel.ranking.AssignScores();
            mainWindow.UpdateRankingGrid();

            ChooseRandomParticipants();
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseRandomParticipants();
        }
    }
}
