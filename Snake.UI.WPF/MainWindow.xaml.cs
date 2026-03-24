using Game.Core;
using Microsoft.Win32;
using Snake.AI;
using Snake.AI.SnakeNEAT;
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
        //private const int TickIntervalMs = 150; //normal
        //private const int TickIntervalMs = 120; //faster
        private const int TickIntervalMs = 10; // Ultra fast for testing AI
        //private const int TickIntervalMs = 5; // Ultra fast for performance testing 

        private const bool debug = true;

        private SnakeGameUI? _game;
        private bool _gameStarted;
        private string aiModelName = "";
        private ISnakeGameController aiController;
        string assetsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        
        // Cache for rendering elements
        private bool _gridDrawn = false;
        private Ellipse? _foodElement;
        private List<Rectangle> _snakeElements = new List<Rectangle>();
        private double _cellWidth;
        private double _cellHeight;

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
            _game = new SnakeGameUI(10, 10, TickIntervalMs);
            _game.aiEnabled = enableAI;
            if(_game.aiEnabled)
            {
                _game.aiController = aiController;
                ToggleAIVisionOn();
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
            
            // Reset rendering state for new game
            _gridDrawn = false;
            _foodElement = null;
            _snakeElements.Clear();
            GameCanvas.Children.Clear();
            
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

            _cellWidth = GameCanvas.ActualWidth / (_game.World.PositionMax.X+1);
            _cellHeight = GameCanvas.ActualHeight / (_game.World.PositionMax.Y+1);

            // Draw grid lines only once
            if (!_gridDrawn)
            {
                DrawGrid();
                _gridDrawn = true;
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

            // Update food position
            UpdateFood(foodPos);

            // Update snake
            UpdateSnake(bodySnapshot);

            // Update score
            ScoreText.Text = $"Score: {score}";

            //Update AI vision if enabled
            var outputs = new double[3];
            UpdateAIVision(_game.GetGameState(), outputs);
        }

        private void DrawGrid()
        {
            // Draw horizontal grid lines
            for (int r = Convert.ToInt32(_game.World.PositionMin.Y); r <= _game.World.PositionMax.Y; r++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = r * _cellHeight,
                    X2 = GameCanvas.ActualWidth,
                    Y2 = r * _cellHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }
            // Draw vertical grid lines
            for (int c = Convert.ToInt32(_game.World.PositionMin.X); c <= _game.World.PositionMax.X; c++)
            {
                var line = new Line
                {
                    X1 = c * _cellWidth,
                    Y1 = 0,
                    X2 = c * _cellWidth,
                    Y2 = GameCanvas.ActualHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }
        }

        private void UpdateFood(Vector2? foodPos)
        {
            if (foodPos.HasValue)
            {
                var food = foodPos.Value;
                
                if (_foodElement == null)
                {
                    // Create food element if it doesn't exist
                    _foodElement = new Ellipse
                    {
                        Width = _cellWidth - 2,
                        Height = _cellHeight - 2,
                        Fill = new SolidColorBrush(Color.FromRgb(233, 69, 96)) // #e94560
                    };
                    GameCanvas.Children.Add(_foodElement);
                }
                
                // Update food position
                Canvas.SetLeft(_foodElement, food.X * _cellWidth + 1);
                Canvas.SetTop(_foodElement, food.Y * _cellHeight + 1);
            }
            else if (_foodElement != null)
            {
                // Remove food if it no longer exists
                GameCanvas.Children.Remove(_foodElement);
                _foodElement = null;
            }
        }

        private void UpdateSnake(Vector2[] bodySnapshot)
        {
            // Adjust snake elements pool size
            while (_snakeElements.Count < bodySnapshot.Length)
            {
                var rect = new Rectangle
                {
                    Width = _cellWidth - 1,
                    Height = _cellHeight - 1,
                    RadiusX = 3,
                    RadiusY = 3
                };
                _snakeElements.Add(rect);
                GameCanvas.Children.Add(rect);
            }

            while (_snakeElements.Count > bodySnapshot.Length)
            {
                var lastElement = _snakeElements[_snakeElements.Count - 1];
                GameCanvas.Children.Remove(lastElement);
                _snakeElements.RemoveAt(_snakeElements.Count - 1);
            }

            // Update each snake segment
            for (int i = 0; i < bodySnapshot.Length; i++)
            {
                var segment = bodySnapshot[i];
                var rect = _snakeElements[i];
                
                // Update position
                Canvas.SetLeft(rect, segment.X * _cellWidth + 0.5);
                Canvas.SetTop(rect, segment.Y * _cellHeight + 0.5);
                
                // Update color (head vs body)
                rect.Fill = i == 0
                    ? new SolidColorBrush(Color.FromRgb(0, 230, 118))   // bright green head
                    : new SolidColorBrush(Color.FromRgb(76, 175, 80));   // green body
            }
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
                if (aiController == null)
                {
                    MessageBox.Show("Please select an AI model first (Press 'C')", "No AI Model",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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
                        var controller = new AI.SnakeNEAT.SnakeController();
                        controller.LoadBestGenome(_loadedModelPath);


                        aiController = controller;
                        //aiController = new SnakeCheater1000();

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
                var trainerWindow = new TrainerWindow(_game);
                trainerWindow.ShowDialog();
                //var res = MessageBox.Show("Training an AI can take a long time. Click OK to start training.", "AI Training", MessageBoxButton.OK, MessageBoxImage.Information);
                //if (res == MessageBoxResult.OK)
                //{
                //    var t = new SnakeTrainer();
                //    t.Train();
                //}
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

        private AIVisionWindow? _aiVisionWindow;
        private void ToggleAIVisionOn()
        {
            if (_aiVisionWindow == null || !_aiVisionWindow.IsVisible)
            {
                _aiVisionWindow = new AIVisionWindow();
                _aiVisionWindow.Closed += (s, e) => _aiVisionWindow = null;
                _aiVisionWindow.Show();
            }
        }

        private void UpdateAIVision(SnakeGameState gameState, double[] neuralOutputs)
        {
            if (_aiVisionWindow == null || !_aiVisionWindow.IsVisible)
                return;

            try
            {
                // Encode the game state to neural network inputs
                var inputs = StateEncoder.Encode(gameState);

                // Find the selected action (max output)
                //int selectedAction = neuralOutputs.GetMaxIndex();

                // Update the vision window
                _aiVisionWindow.UpdateVision(inputs, neuralOutputs);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the game
                Console.WriteLine($"Error updating AI vision: {ex.Message}");
            }
        }




    }


}