using Game.Core;
using Microsoft.Win32;
using Snake.UI;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Numerics;
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
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private const int TickIntervalMs = 5000; //very slow
        //private const int TickIntervalMs = 500; //slow
        //private const int TickIntervalMs = 120; //normal
        private const int TickIntervalMs = 10; // Ultra fast for testing AI
        private const bool debug = true;

        private SnakeGameUI? _game;
        private bool _gameStarted;
        private string aiModelName = "";
        //private Snake aiController;
        string assetsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

        public MainWindow()
        {
            InitializeComponent();
            GameCanvas.Loaded += GameCanvas_Loaded;
        }

        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Press <SPACE> to start (as Human)");
            sb.AppendLine("-- -- -- --");
            sb.AppendLine("Press <T> to start train an AI");
            sb.AppendLine("Press <C> to choose AI / Model");
            sb.AppendLine("Press <V> let AI play");
            ShowOverlay("SNAKE GAME - Main Menu", sb.ToString());
        }

        private void StartGame(bool enableAI)
        {
            _game?.Stop();

            //_game = new SnakeGameUI(Core.SnakeWorld.SnakeWorldSize.Small, TickIntervalMs);
            _game = new SnakeGameUI(40, 40, TickIntervalMs);
            _game.aiEnabled = enableAI;
            if(_game.aiEnabled)
            {
                //_game.aiController = new 
            }
            _game.GameUpdated += () => Dispatcher.InvokeAsync(DrawGame, DispatcherPriority.Render);
            _game.GameOver += () => Dispatcher.InvokeAsync(OnGameOver, DispatcherPriority.Render);

            _gameStarted = true;
            OverlayPanel.Visibility = Visibility.Collapsed;

            // Update player mode label
            if (enableAI){
                PlayerModeText.Text = String.Format("AI ({0})", aiModelName);
            }
            else
            {
                    PlayerModeText.Text = "HUMAN";
            }
            PlayerModeText.Foreground = enableAI 
                ? new SolidColorBrush(Color.FromRgb(0, 230, 118))  // #00e676 green for AI
                : new SolidColorBrush(Color.FromRgb(100, 181, 246)); // #64b5f6 blue for Human
            
            DrawGame();
            _game.Start();
        }

        private void OnGameOver()
        {
            _gameStarted = false;
            int score = _game!.Score;
            
            if (debug)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                if (_game.IsGameVictory)
                {
                    sb.AppendLine($" Your Score: {score} - You won the game");
                }
                else
                {
                    sb.AppendLine($" Your Score: {score} ");
                }
                sb.AppendLine("");
                sb.AppendLine("");
                sb.AppendLine("Press <SPACE> to start (as Human)");
                sb.AppendLine("-- -- -- --");
                sb.AppendLine("Press <T> to start train an AI");
                sb.AppendLine("Press <C> to choose AI / Model");
                sb.AppendLine("Press <V> let AI play");
                var headPos = _game.Snake.Head;
                sb.AppendLine("Snake.CurrentDirection: " + _game.Snake.CurrentDirection);
                sb.AppendLine("Snake.Head Position: " +_game.Snake.Head);
                sb.AppendLine("Snake.Head Position: " + headPos);
                ShowOverlay("GAME OVER",sb.ToString());
            }
            else
            {
                ShowOverlay("GAME OVER", $"Score: {score}\nPress SPACE to restart");
            }
        }

        private void ShowOverlay(string title, string message)
        {
            OverlayTitle.Text = title;
            OverlayMessage.Text = message;
            OverlayPanel.Visibility = Visibility.Visible;
        }

        private void DrawGame()
        {
            if (_game == null) return;

            GameCanvas.Children.Clear();

            double cellWidth = GameCanvas.ActualWidth / (_game.World.PositionMax.X+1);
            double cellHeight = GameCanvas.ActualHeight / (_game.World.PositionMax.Y+1);

            // Draw grid lines (subtle)
            for (int r = Convert.ToInt32(_game.World.PositionMin.X); r <= _game.World.PositionMax.X; r++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = r * cellHeight,
                    X2 = GameCanvas.ActualWidth,
                    Y2 = r * cellHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }
            for (int c = Convert.ToInt32(_game.World.PositionMin.Y); c <= _game.World.PositionMax.Y; c++)
            {
                var line = new Line
                {
                    X1 = c * cellWidth,
                    Y1 = 0,
                    X2 = c * cellWidth,
                    Y2 = GameCanvas.ActualHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }



            // Snapshot game state under lock to avoid tearing
            Vector2? foodPos;
            Vector2[] bodySnapshot;
            int score;
            lock (_game.SyncRoot)
            {
                foodPos = _game.World.FoodPosition;
                bodySnapshot = _game.Snake.Body;
                score = _game.Score;
            }

            // Draw food
            if (foodPos.HasValue)
            {
                var food = foodPos.Value;
                var foodRect = new Ellipse
                {
                    Width = cellWidth - 2,
                    Height = cellHeight - 2,
                    Fill = new SolidColorBrush(Color.FromRgb(233, 69, 96)) // #e94560
                };
                Canvas.SetLeft(foodRect, food.X * cellWidth + 1);
                Canvas.SetTop(foodRect, food.Y * cellHeight + 1);
                GameCanvas.Children.Add(foodRect);
            }

            // Draw snake
            bool isHead = true;
            foreach (var segment in bodySnapshot)
            {
                var rect = new Rectangle
                {
                    Width = cellWidth - 1,
                    Height = cellHeight - 1,
                    RadiusX = 3,
                    RadiusY = 3,
                    Fill = isHead
                        ? new SolidColorBrush(Color.FromRgb(0, 230, 118))   // bright green head
                        : new SolidColorBrush(Color.FromRgb(76, 175, 80))   // green body
                };
                Canvas.SetLeft(rect, (segment.X) * cellWidth + 0.5);
                Canvas.SetTop(rect, (segment.Y) * cellHeight + 0.5);
                GameCanvas.Children.Add(rect);
                isHead = false;
            }

            // Update score
            ScoreText.Text = $"Score: {score}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !_gameStarted)
            {
                StartGame(false);
                return;
            }
            if (e.Key == Key.V && !_gameStarted)
            {
                StartGame(true);
                return;
            }
            if (e.Key == Key.C && !_gameStarted)
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Load Saved AI Training Model",
                    Filter = "Training Model Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    InitialDirectory = assetsPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        
                        var _loadedModelPath = openFileDialog.FileName;
                        var _loadedModelName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                        aiModelName = _loadedModelName;

                        //aiController = controller;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading model: {ex.Message}", "Load Error",
                   MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
            }
            if (e.Key == Key.T && !_gameStarted)
            {
                //var trainerWindow = new TrainerWindow(this);
                //trainerWindow.ShowDialog();
                return;
            }
            if (e.Key == Key.S && !_gameStarted)
            {

            }

            if (_game == null || _game.IsGameOver) return;

            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    _game.ChangeDirection(Direction2D.Up);
                    break;
                case Key.Down:
                case Key.S:
                    _game.ChangeDirection(Direction2D.Down);
                    break;
                case Key.Left:
                case Key.A:
                    _game.ChangeDirection(Direction2D.Left);
                    break;
                case Key.Right:
                case Key.D:
                    _game.ChangeDirection(Direction2D.Right);
                    break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _game?.Stop();
            base.OnClosed(e);
        }







    }


}