using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Multiplayer_Tic_Tac_Toe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        int turn = 0;
        string actualP1Name = "", actualP2Name = "";
        bool gameStarted = false;
        Button[,] gameMatrix;

        public string ActualP1Name {
            get { return actualP1Name; }
            set { actualP1Name = value; }
        }

        public string ActualP2Name {
            get { return actualP2Name; }
            set { actualP2Name = value; }
        }

        PlayersList players = new PlayersList();
        ApplicationDataContainer localStorage;

        public PlayersList Players {
            get { return players; }
            set { SetProperty<PlayersList>(ref players, value); }
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (object.Equals(storage, value)) {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;
            ClearGameGrid();
            playAgainButton.IsEnabled = false;

            gameMatrix = new Button[3, 3] {
                {but00, but01, but02},
                {but10, but11, but12},
                {but20, but21, but22}
            };

            localStorage = ApplicationData.Current.LocalSettings;

            LoadPlayers();

            Application.Current.Suspending += OnSuspending;
        }

        void SwitchToLandscape() {
            headerRow.Height = new GridLength(120);
            offsetColumn.Width = new GridLength(120);
            infoColumn.Width = new GridLength(1, GridUnitType.Star);
        }

        void SwitchToPortrait() {
            headerRow.Height = new GridLength(0);
            offsetColumn.Width = new GridLength(0);
            infoColumn.Width = new GridLength(0);
        }

        void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs args) {
            SavePlayers();
        }

        private void SavePlayers() {
            foreach (Player p in players) {
                string data = p.Won.ToString() + "," + p.Lost.ToString();
                localStorage.Values[p.PlayerName] = data;
            }            
        }

        void LoadPlayers() {
            players.Clear();
            //players = new List<Player>();
            foreach (string playerName in localStorage.Values.Keys) {
                string[] data = (localStorage.Values[playerName] as string).Split(new char[] { ',' });
                int won, lost;
                if (Int32.TryParse(data[0], out won) == true && Int32.TryParse(data[1], out lost) == true) {
                    players.Add(new Player(playerName, won, lost));
                    
                    Debug.WriteLine(players.ElementAt<Player>(players.Count - 1).ToString());
                }
            }
            players.Sort(new PlayerComparer());
            //Players = players;
            //scoreList.ItemsSource = Players;
            scoreList.UpdateLayout();
            scoreList.InvalidateArrange();
        }

        class PlayerComparer : IComparer<Player> {

            public int Compare(Player x, Player y) {
                int _x = x.Won - x.Lost;
                int _y = y.Won - y.Lost;
                if (_x > _y) return -1;
                if (_x < _y) return 1;
                return 0;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void OnNewGameButtonClicked(object sender, RoutedEventArgs e) {
            NewGameDialog dialog = new NewGameDialog();

            Popup popup = new Popup {
                Child = dialog,
                IsLightDismissEnabled = true
            };

            Binding binding1 = new Binding {
                Source = dialog.Player1TB,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.TwoWay
            };
            this.SetBinding(Player1Property, binding1);

            Binding binding2 = new Binding {
                Source = dialog.Player2TB,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.TwoWay
            };
            this.SetBinding(Player2Property, binding2);

            dialog.SizeChanged += (src, args) => {
                popup.VerticalOffset = this.ActualHeight - this.BottomAppBar.ActualHeight - dialog.ActualHeight
                    - 48;
                popup.HorizontalOffset = this.ActualWidth - dialog.ActualWidth - 48;
            };

            dialog.PlayButton.Click += (src, args) => {
                if (this.Player1 == "" || this.Player2 == "" || this.Player1.Equals(this.Player2)) {
                    dialog.ErrorText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    return;
                }
                popup.IsOpen = false;
                turn = 0;
                actualP1Name = this.Player1;
                actualP2Name = this.Player2;
                ClearGameGrid();
                gameStarted = true;
                this.BottomAppBar.IsOpen = false;
                playAgainButton.IsEnabled = true;
                ShowTurn();
            };

            popup.IsOpen = true;
        }

        private void ShowTurn() {
            if (gameStarted == false) return;
            scoreList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            infoGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            winText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            turnText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (turn == 0) {
                currentPlayerName.Text = ActualP1Name;
            }
            else {
                currentPlayerName.Text = ActualP2Name;
            }
        }

        private void ClearGameGrid() {
            foreach (UIElement child in gameGrid.Children) {
                if (child is Border) continue;
                (child as Button).Content = null;
            }
            gameMatrix = new Button[3, 3] {
                {but00, but01, but02},
                {but10, but11, but12},
                {but20, but21, but22}
            };
        }

        public static DependencyProperty Player1Property { private set; get; }
        public static DependencyProperty Player2Property { private set; get; }

        static MainPage() {
            Player1Property = DependencyProperty.Register("Player1", typeof(string), typeof(MainPage),
                                            new PropertyMetadata("", OnPlayerSet));
            Player2Property = DependencyProperty.Register("Player2", typeof(string), typeof(MainPage),
                                            new PropertyMetadata("", OnPlayerSet));
        }

        private static void OnPlayerSet(DependencyObject d, DependencyPropertyChangedEventArgs e) {            
            (d as MainPage).OnPlayerSet(e);            
        }

        private void OnPlayerSet(DependencyPropertyChangedEventArgs e) {
            //Debug.WriteLine(e.NewValue);
        }

        public string Player1 {
            set { SetValue(Player1Property, value); }
            get { return (string)GetValue(Player1Property); }
        }

        public string Player2 {
            set { SetValue(Player2Property, value); }
            get { return (string)GetValue(Player2Property); }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e) {
            if (!gameStarted) return;
            
            Button button = sender as Button;
            if (button.Content != null) return;
            
            if (turn==0) {
                Ellipse ellipse = new Ellipse {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    StrokeThickness = 3,
                    Stroke = new SolidColorBrush(Colors.Red),
                    Width = 3*button.ActualWidth/4 ,
                    Height = 3*button.ActualHeight/4 , 
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center
                };
                button.Content = ellipse;
            }
            else {
                Windows.UI.Xaml.Shapes.Path path = new Windows.UI.Xaml.Shapes.Path() {
                    Stroke = new SolidColorBrush(Colors.Blue),
                    StrokeThickness = 3,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center
                };
                PathGeometry geometry = new PathGeometry();

                PathFigure seg1 = new PathFigure();
                seg1.StartPoint = new Point(10, 10);
                seg1.Segments.Add(new LineSegment() {
                    Point = new Point(button.ActualWidth - 10, button.ActualHeight - 10)
                });
                PathFigure seg2 = new PathFigure();
                seg2.StartPoint = new Point(button.ActualWidth - 10, 10);
                seg2.Segments.Add(new LineSegment() {
                    Point = new Point(10, button.ActualHeight - 10)
                });

                geometry.Figures.Add(seg1);
                geometry.Figures.Add(seg2);

                path.Data = geometry;

                button.Content = path;
            }

            int x = (button.Tag as ButtonCoords).X;
            int y = (button.Tag as ButtonCoords).Y;
            
            int result = EvaluateGame(x, y);
            if (result != 0) { //there is one winner
                gameStarted = false;
                ShowWin(result);
                UpdatePlayers(result);
                SavePlayers();
            }
            else if (GridIsFull()) { //draw
                gameStarted = false;
                ShowWin(result);
            }

            turn++;
            turn %= 2;

            ShowTurn();
        }

        private void UpdatePlayers(int result) {
            if (result == 0) return;
            bool handled1 = false, handled2 = false;
            foreach (Player p in players) {
                if (p.PlayerName.Equals(ActualP1Name) && !handled1) {
                    if (result == 1) p.Won++;
                    else p.Lost++;
                    handled1 = true;
                }
                if (p.PlayerName.Equals(ActualP2Name) && !handled2) {
                    if (result == 2) p.Won++;
                    else p.Lost++;
                    handled2 = true;
                }
            }
            if (!handled1) {
                if (result == 1) players.Add(new Player(ActualP1Name, 1, 0));
                else players.Add(new Player(ActualP1Name, 0, 1));
            }
            if (!handled2) {
                if (result == 2) players.Add(new Player(ActualP2Name, 1, 0));
                else players.Add(new Player(ActualP2Name, 0, 1));
            }
        }

        void ShowWin(int result) {
            turnText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            winText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            switch (result) {
                case 0 :                    
                    winnerName.Text = "Draw !";
                    break;
                case 1 :
                    winnerName.Text = ActualP1Name + " wins !";
                    break;
                default :
                    winnerName.Text = ActualP2Name + " wins !";
                    break;
            }
        }

        int EvaluateGame(int x, int y) {
            Type t = turn == 0 ? typeof(Ellipse) : typeof(Windows.UI.Xaml.Shapes.Path);
            int ans = turn == 0 ? 1 : 2;
            
            //checking column
            int flag1 = 0;
            for (int i = 0; i < 3; i++) {
                object o = gameMatrix[i, y].Content;
                if (o==null || o.GetType() != t) { 
                    flag1 = 1; 
                    break; 
                }
            }
            if (flag1 == 0) return ans;

            //checking row
            int flag2 = 0;
            for (int i = 0; i < 3; i++) {
                object o = gameMatrix[x, i].Content;
                if (o==null || o.GetType() != t) { flag2 = 1; break; }
            }
            if (flag2 == 0) return ans;

            if (x == y) { //checking diagonal
                int flag3 = 0;
                for (int i = 0; i < 3; i++) {
                    object o = gameMatrix[i, i].Content;
                    if (o==null || o.GetType() != t) { flag3 = 1; break; }
                }
                if (flag3 == 0) return ans;
            }

            if (y == 2 - x) { //checking anti-diag
                int flag4 = 0;
                for (int i = 0; i < 3; i++) {
                    object o = gameMatrix[i, 2-i].Content;
                    if (o==null || o.GetType() != t) { flag4 = 1; break; }
                }
                if (flag4 == 0) return ans;
            }

            return 0;
        }

        bool GridIsFull() {
            foreach (UIElement elem in gameGrid.Children) {
                if (elem is Border) continue;
                Button b = elem as Button;
                if (b.Content == null) return false;
            }
            return true;
        }

        private void OnPlayAgainButtonClicked(object sender, RoutedEventArgs e) {
            ClearGameGrid();
            gameStarted = true;
            turn = 0;
            ShowTurn();
        }

        private void OnScoreboardButtonClicked(object sender, RoutedEventArgs e) {
            LoadPlayers();
            infoGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            scoreList.Visibility = Visibility.Visible;
            scoreList.UpdateLayout();
            scoreList.InvalidateArrange();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnClearScoreboardButtonClicked(object sender, RoutedEventArgs e) {
            ClearPlayers();
        }

        private void ClearPlayers() {
            foreach (Player p in players) {
                localStorage.Values.Remove(p.PlayerName);
            }
            players.Clear();
            //players = new List<Player>();
            //Players = players;
            //scoreList.ItemsSource = players;
            scoreList.UpdateLayout();
            scoreList.InvalidateArrange();
        }

        private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e) {
            if (e.NewSize.Width > e.NewSize.Height) {
                SwitchToLandscape();
            }
            else {
                SwitchToPortrait();
            }
        }
    }

    public class ButtonCoords : DependencyObject {
        public static DependencyProperty XProperty { protected set; get; }
        public static DependencyProperty YProperty { protected set; get; }

        static ButtonCoords() {
            XProperty = DependencyProperty.Register("X", typeof(int), typeof(ButtonCoords),
                                            new PropertyMetadata(0));
            YProperty = DependencyProperty.Register("Y", typeof(int), typeof(ButtonCoords),
                                            new PropertyMetadata(0));
        }

        public int X {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public int Y {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
    }

    public class PlayersList : List<Player>, INotifyCollectionChanged {
        public void Add(Player p) {
            base.Add(p);
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void Clear() {
            base.Clear();
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

}
