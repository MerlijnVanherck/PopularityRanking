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
        private readonly MainWindow mainWindow;
        private bool isRankedMode = true;

        public MatchupWindow(MainWindow window)
        {
            mainWindow = window;
            DataContext = ViewModel.ranking;
            InitializeComponent();
            participantSlider.Value = 2;
        }
        public void UpdateComboBoxes()
        {
            var list = ViewModel.ranking.Participants.Values.OrderBy(p => p.Id);
            foreach (Grid g in matchupPanel.Children)
                ((ComboBox)g.Children[^1]).ItemsSource = list;
        }

        public void ChooseRandomParticipants()
        {
            var comboboxes = new List<ComboBox>();
            foreach (Grid c in matchupPanel.Children)
                comboboxes.Add((ComboBox)c.Children[^1]);

            var participants = ViewModel.ranking.RandomMatchup(comboboxes.Count);

            for (int i = 0; i < comboboxes.Count; i++)
                comboboxes[i].SelectedItem = participants[i];
        }

        public void ChooseRivalParticipants()
        {
            var comboboxes = new List<ComboBox>();
            foreach (Grid c in matchupPanel.Children)
                comboboxes.Add((ComboBox)c.Children[^1]);

            var participants = ViewModel.ranking.RivalMatchup(comboboxes.Count);

            for (int i = 0; i < comboboxes.Count; i++)
                comboboxes[i].SelectedItem = participants[i];
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
                ItemsSource = ViewModel.ranking.Participants.Values.OrderBy(p => p.Id),
                Margin = new Thickness(0, 3, 0, 3),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            comboBox.SelectionChanged += this.Participant_SelectionChanged;

            Grid.SetColumn(comboBox, 1);
            Grid.SetRow(comboBox, 0);
            Grid.SetRowSpan(comboBox, 2);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(isRankedMode ? 30 : 50)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });

            if (isRankedMode)
            {
                CreateUpArrow(grid);
                CreateDownArrow(grid);
            }
            else
                CreateScoreBox(grid);
            
            grid.Children.Add(comboBox);

            return grid;
        }

        private void CreateScoreBox(Grid grid)
        {
            var textBox = new TextBox
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 2, 0),
                Padding = new Thickness(0),
                Height = 24
            };

            textBox.TextChanged += this.ScoreBox_TextChanged;

            Grid.SetColumn(textBox, 0);
            Grid.SetRow(textBox, 0);
            Grid.SetRowSpan(textBox, 2);

            grid.Children.Add(textBox);
        }

        private void ScoreBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProcessMatchupButton.IsEnabled = VerifyTextBoxes() && VerifySelectionBoxes();
        }

        private bool VerifyTextBoxes()
        {
            if (isRankedMode)
                return true;

            var textList = new List<string>();
            foreach (Grid c in matchupPanel.Children)
                textList.Add(((TextBox)c.Children[0]).Text);

            foreach (var t in textList)
                if (!int.TryParse(t, out _))
                    return false;

            return true;
        }

        private void CreateUpArrow(Grid grid)
        {
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

            grid.Children.Add(upArrow);
        }

        private void CreateDownArrow(Grid grid)
        {
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

            grid.Children.Add(downArrow);
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
            ProcessMatchupButton.IsEnabled = VerifySelectionBoxes()
                && VerifyTextBoxes();
        }

        private bool VerifySelectionBoxes()
        {
            var selections = new List<int>();
            foreach (Grid c in matchupPanel.Children)
                selections.Add(((ComboBox)c.Children[^1]).SelectedIndex);

            if (selections.Distinct().Count() == selections.Count
                && !selections.Contains(-1))
                return true;

            return false;
        }

        private void ProcessMatchupButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRankedMode)
                RunMatchupOrder();
            else
                RunMatchupScored();

            ViewModel.ranking.AssignScores();
            mainWindow.UpdateRankingGrid();

            ChooseRandomParticipants();
        }

        private void RunMatchupOrder()
        {
            var matchup = new List<Participant>();

            foreach (Grid c in matchupPanel.Children)
                matchup.Add(((Participant)((ComboBox)c.Children[^1]).SelectedValue));

            ViewModel.ranking.RunAnyPlayersMatchup(matchup.ToArray());
        }

        private void RunMatchupScored()
        {
            var matchup = new List<(Participant, int)>();

            foreach (Grid c in matchupPanel.Children)
                matchup.Add(((Participant)((ComboBox)c.Children[^1]).SelectedValue,
                    int.Parse(((TextBox)c.Children[0]).Text)));

            matchup = matchup.OrderByDescending(p => p.Item2).ToList();

            ViewModel.ranking.RunAnyPlayersMatchupScored(
                matchup.Select(p => p.Item1).ToArray(),
                matchup.Select(p => p.Item2).ToArray());
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseRandomParticipants();
        }

        private void RivalsButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseRivalParticipants();
        }

        private void ScoredRankedToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isRankedMode)
                SetScoredMode((Button) sender);
            else
                SetRankedMode((Button) sender);
        }

        private void SetRankedMode(Button b)
        {
            isRankedMode = true;
            WinnerLabel.Visibility = Visibility.Visible;
            LoserLabel.Visibility = Visibility.Visible;
            InstructionsLabel.Content = "Rank participants in order";
            b.Content = "Score mode";
            var participantNumber = (int)participantSlider.Value;
            DecreaseParticipants(participantNumber);
            IncreaseParticipants(participantNumber);
        }

        private void SetScoredMode(Button b)
        {
            isRankedMode = false;
            WinnerLabel.Visibility = Visibility.Hidden;
            LoserLabel.Visibility = Visibility.Hidden;
            InstructionsLabel.Content = "Assign each participant a score";
            b.Content = "Ranked mode";
            var participantNumber = (int)participantSlider.Value;
            DecreaseParticipants(participantNumber);
            IncreaseParticipants(participantNumber);
        }
    }
}
