using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace paperSnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            {GridValue.Empty, Images.Empty },
            {GridValue.Snake, Images.Body },
            {GridValue.Food, Images.Food }
        };

        private readonly int rows = 15, cols = 15;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
            LoadLeaderboard(); 
        }

        private async Task RunGame()
        {
            nicknameTextBox.IsEnabled = false;
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
            nicknameTextBox.IsEnabled = true; 
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (nicknameTextBox.IsFocused)
            {
                return; 
            }

            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left); 
                    break;

                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;

                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;

                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty
                    };

                    images[r,c] = image;
                    GameGrid.Children.Add(image);   
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            ScoreText.Text = $"score {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    if (gameState.HeadPosition().Row == r && gameState.HeadPosition().Column == c)
                    {
                        // Rotate the head image based on the current direction
                        RotateTransform rotateTransform = new RotateTransform(DirectionToAngle(gameState.Dir));
                        gridImages[r, c].Source = Images.Head;
                        gridImages[r, c].RenderTransform = rotateTransform;
                        gridImages[r, c].RenderTransformOrigin = new Point(0.5, 0.5); // Set the rotation center to the middle of the image
                    }
                    else
                    {
                        gridImages[r, c].Source = gridValToImage[gridVal];
                    }
                }
            }
        }

        // Convert direction to angle for rotation
        private double DirectionToAngle(Direction dir)
        {
            if (dir == Direction.Up) return 0;
            if (dir == Direction.Right) return 90;
            if (dir == Direction.Down) return 180;
            if (dir == Direction.Left) return 270;
            return 0; // Default to 0 degrees if direction is unknown
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i>= 1; i--)
            {
                OverlayText.Text = i.ToString() ;
                await Task.Delay(300) ;
            }
        }

        private async Task ShowGameOver()
        {
            await Task.Delay(100);
            SaveScore(nicknameTextBox.Text, gameState.Score);
            LoadLeaderboard();
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "press any key to start";
        }

        public void SaveScore(string nickname, int score)
        {
            if (string.IsNullOrWhiteSpace(nickname)) return; 
            if (nickname == "Enter Nickname") nickname = "Anonymous";

            var scores = new Dictionary<string, int>();
            string[] lines = File.ReadAllLines("Assets/scores.txt");
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    scores[parts[0].Trim()] = int.Parse(parts[1].Trim());
                }
            }

            if (!scores.ContainsKey(nickname) || score > scores[nickname])
            {
                scores[nickname] = score;
                File.WriteAllLines("Assets/scores.txt", scores.Select(kv => $"{kv.Key}: {kv.Value}"));
            }
        }
        public void NicknameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Enter Nickname")
            {
                textBox.Text = string.Empty;
            }
        }

        public void NicknameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter Nickname";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            nicknameTextBox.IsEnabled = false;
        }

        public void LoadLeaderboard()
        {
            var leaderboard = new List<(string Player, int Score)>();
            string[] lines = File.ReadAllLines("Assets/scores.txt");
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    leaderboard.Add((parts[0].Trim(), int.Parse(parts[1].Trim())));
                }
            }
            leaderboard = leaderboard.OrderByDescending(x => x.Score).Take(10).ToList();
            LeaderboardPanel.Children.Clear();
            foreach (var entry in leaderboard)
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                stackPanel.Children.Add(new TextBlock { Text = entry.Player, Width = 140 });
                stackPanel.Children.Add(new TextBlock { Text = entry.Score.ToString(), Width = 60 });
                LeaderboardPanel.Children.Add(stackPanel);
            }
        }
    }
}

