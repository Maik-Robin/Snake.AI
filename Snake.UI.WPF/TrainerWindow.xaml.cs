using Game.Core;
using Microsoft.Win32;
using SnakeEngine;
using SnakeEngine.AI;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Snake.AI;

public partial class TrainerWindow : Window
{
    private string? _selectedImagePath;
    private bool _isUpdatingFromSlider = false;
    private bool _isUpdatingFromTextBox = false;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isTraining = false;
    private string? _loadedModelPath;
    private string? _loadedModelName;
    string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
    string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
    private IGameEnvironment _evn;

    public TrainerWindow(IGameEnvironment env)
    {
        _evn = env;
        InitializeComponent();
        LoadDefaultImage();
        LoadDefaultValues();
        RandomizeRobotName();
    }

    private void LoadDefaultValues()
    {
        // Basic parameters
        EpisodesSlider.Value = 100000;
        MaxStepsSlider.Value = 5000;

        // Advanced parameters
        AlphaSlider.Value = 0.1;
        GammaSlider.Value = 0.95;
        EpsilonStartSlider.Value = 1.0;
        EpsilonMinSlider.Value = 0.05;
        EpsilonDecaySlider.Value = 0.00001;
        ReportIntervalSlider.Value = 100;

        StatusText.Text = "Default values loaded. Ready to train.";
    }

    public void SetRobotImageByName(string name)
    {
        var robotFiles = Directory.GetFiles(assetsPath, "robot_*.png");
        foreach (var robotFile in robotFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(robotFile);
            string robotName = fileName.Replace("robot_", "");
            if (name.Contains(robotName))
            {
                SetImage(robotFile);
                break;
            }
        }
    }

    public void RandomizeRobotName()
    {
        try
        {
            var robotFiles = Directory.GetFiles(assetsPath, "robot_*.png");

            if (robotFiles.Length == 0)
            {
                StatusText.Text = "No robot images found in Assets folder.";
                return;
            }

            var random = new Random();
            string selectedRobotFile = robotFiles[random.Next(robotFiles.Length)];

            string fileName = Path.GetFileNameWithoutExtension(selectedRobotFile);
            string robotName = fileName.Replace("robot_", "");

            int randomDigits = random.Next(100, 1000);

            string finalName = $"{robotName}{randomDigits}";

            ConfigNameTextBox.Text = finalName;

            SetImage(selectedRobotFile);

            StatusText.Text = $"Randomized robot name: {finalName}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error randomizing robot name: {ex.Message}";
        }
    }

    private void LoadDefaultImage()
    {
        // Try to load a default image from Assets folder if it exists
        string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "snake_logo.png");

        if (File.Exists(defaultImagePath))
        {
            SetImage(defaultImagePath);
        }
        else
        {
            // Create a placeholder if no default image exists
            StatusText.Text = "No default image found. Click 'Choose Custom Image' to add one.";
        }
    }

    private void SelectImageButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Training Logo Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SetImage(openFileDialog.FileName);
            StatusText.Text = $"Image loaded: {Path.GetFileName(openFileDialog.FileName)}";
        }
    }

    private void SetImage(string imagePath)
    {
        try
        {
            _selectedImagePath = imagePath;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            TrainingImage.Source = bitmap;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading image: {ex.Message}", "Image Load Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void StartTrainingButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate inputs
        if (!ValidateInputs(out var validationMessage))
        {
            MessageBox.Show(validationMessage, "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Get parameters
        int episodes = int.Parse(EpisodesTextBox.Text);
        int maxSteps = int.Parse(MaxStepsTextBox.Text);
        double alpha = double.Parse(AlphaTextBox.Text, CultureInfo.InvariantCulture);
        double gamma = double.Parse(GammaTextBox.Text, CultureInfo.InvariantCulture);
        double epsilonStart = double.Parse(EpsilonStartTextBox.Text, CultureInfo.InvariantCulture);
        double epsilonMin = double.Parse(EpsilonMinTextBox.Text, CultureInfo.InvariantCulture);
        double epsilonDecay = double.Parse(EpsilonDecayTextBox.Text, CultureInfo.InvariantCulture);
        int reportInterval = int.Parse(ReportIntervalTextBox.Text);
        string robotName = ConfigNameTextBox.Text;

        // Update UI for training state
        _isTraining = true;
        StartTrainingButton.IsEnabled = false;
        StatusText.Text = $"Starting training with {episodes:N0} episodes...";

        // Create cancellation token
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        //var sizeX = _evn.BoardSizeX;
        //var sizeY = _evn.BoardSizeY;

        IGameTrainer trainer = new SnakeNEAT.SnakeTrainer();
        try
        {
            await Task.Run(() =>
            {

                //var game = new SnakeGameForAI(sizeY, sizeX);

                trainer.Train();


                /*
                // Subscribe to events
                trainer.ProgressUpdated += OnTrainingProgressUpdated;
                trainer.EpisodeCompleted += OnEpisodeCompleted;
                trainer.TrainingCompleted += OnTrainingCompleted;

                try
                {
                    trainer.TrainModel(episodes, maxSteps, reportInterval);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        trainer.SaveTraining(robotName);
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = "Training completed successfully! Data saved.";
                        });
                    }
                }
                finally
                {
                    // Unsubscribe from events
                    trainer.ProgressUpdated -= OnTrainingProgressUpdated;
                    trainer.EpisodeCompleted -= OnEpisodeCompleted;
                    trainer.TrainingCompleted -= OnTrainingCompleted;
                }
                */

            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Training cancelled by user.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Training error: {ex.Message}", "Training Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Training failed.";
        }
        finally
        {
            _isTraining = false;
            StartTrainingButton.IsEnabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /*
    private void OnTrainingProgressUpdated(object? sender, SnakeTrainerMain.TrainingProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text =
                $"Episode {e.CurrentEpisode:N0}/{e.TotalEpisodes:N0} ({e.ProgressPercentage:F1}%)\n" +
                $"Last: {e.LastSteps} steps, Reward: {e.LastReward:F2}\n" +
                $"Elapsed: {e.Elapsed:hh\\:mm\\:ss} | Remaining: {e.EstimatedRemaining:hh\\:mm\\:ss}";
        });
    }


    private void OnEpisodeCompleted(object? sender, SnakeTrainerMain.EpisodeCompletedEventArgs e)
    {
        // This fires every episode - you can use this for detailed logging or charting
        // For now, we'll let the ProgressUpdated event handle UI updates
    }

    private void OnTrainingCompleted(object? sender, SnakeTrainerMain.TrainingCompletedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                $"Training Completed!\n\n" +
                $"Total Episodes: {e.TotalEpisodes:N0}\n" +
                $"Total Time: {e.TotalTime:hh\\:mm\\:ss}\n\n" +
                $"Training data has been saved.",
                "Training Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        });
    }

        */

    private bool ValidateInputs(out string message)
    {
        message = string.Empty;

        if (!int.TryParse(EpisodesTextBox.Text, out int episodes) || episodes <= 0)
        {
            message = "Episodes must be a positive integer.";
            return false;
        }

        if (!int.TryParse(MaxStepsTextBox.Text, out int maxSteps) || maxSteps <= 0)
        {
            message = "Max Steps must be a positive integer.";
            return false;
        }

        if (!double.TryParse(AlphaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double lr) || lr <= 0 || lr > 1)
        {
            message = "Learning Rate must be between 0 and 1.";
            return false;
        }

        if (!double.TryParse(GammaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double df) || df < 0 || df > 1)
        {
            message = "Discount Factor must be between 0 and 1.";
            return false;
        }

        if (!double.TryParse(EpsilonStartTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double er) || er < 0 || er > 1)
        {
            message = "Exploration Rate must be between 0 and 1.";
            return false;
        }

        if (!double.TryParse(EpsilonDecayTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double ed) || ed <= 0 || ed > 1)
        {
            message = "Epsilon Decay must be between 0 and 1.";
            return false;
        }

        if (!double.TryParse(EpsilonMinTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double emin) || emin < 0 || emin > 1)
        {
            message = "Minimum Exploration Rate must be between 0 and 1.";
            return false;
        }

        if (!int.TryParse(ReportIntervalTextBox.Text, out int ri) || ri <= 0)
        {
            message = "Report Interval must be a positive integer.";
            return false;
        }

        return true;
    }

    private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate inputs first
        if (!ValidateInputs(out var validationMessage))
        {
            MessageBox.Show(validationMessage, "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Get the configuration name
        string configName = string.IsNullOrWhiteSpace(ConfigNameTextBox.Text)
            ? "training_config"
            : ConfigNameTextBox.Text.Trim();

        // Sanitize filename
        string safeFileName = string.Join("_", configName.Split(Path.GetInvalidFileNameChars()));

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save Training Configuration",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = "json",
            FileName = $"{safeFileName}.json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var config = new
                {
                    ConfigurationName = configName,
                    SavedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Episodes = int.Parse(EpisodesTextBox.Text),
                    MaxStepsPerEpisode = int.Parse(MaxStepsTextBox.Text),
                    LearningRate = double.Parse(AlphaTextBox.Text, CultureInfo.InvariantCulture),
                    DiscountFactor = double.Parse(GammaTextBox.Text, CultureInfo.InvariantCulture),
                    ExplorationRate = double.Parse(EpsilonStartTextBox.Text, CultureInfo.InvariantCulture),
                    EpsilonDecay = double.Parse(EpsilonDecayTextBox.Text, CultureInfo.InvariantCulture),
                    EpsilonMin = double.Parse(EpsilonMinTextBox.Text, CultureInfo.InvariantCulture),
                    ReportInterval = int.Parse(ReportIntervalTextBox.Text),
                    ImagePath = _selectedImagePath
                };

                string json = System.Text.Json.JsonSerializer.Serialize(config,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(saveFileDialog.FileName, json);

                StatusText.Text = $"Configuration '{configName}' saved to {Path.GetFileName(saveFileDialog.FileName)}";
                MessageBox.Show($"Configuration '{configName}' saved successfully!\n\nLocation: {saveFileDialog.FileName}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to save configuration.";
            }
        }
    }

    private void LoadConfigButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Load Training Configuration",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Load configuration name
                if (root.TryGetProperty("ConfigurationName", out var configName))
                {
                    ConfigNameTextBox.Text = configName.GetString() ?? "Training_Config";
                }

                // Load basic parameters
                if (root.TryGetProperty("Episodes", out var episodes))
                {
                    EpisodesTextBox.Text = episodes.GetInt32().ToString();
                }
                if (root.TryGetProperty("MaxStepsPerEpisode", out var maxSteps))
                {
                    MaxStepsTextBox.Text = maxSteps.GetInt32().ToString();
                }

                // Load advanced parameters
                if (root.TryGetProperty("LearningRate", out var alpha))
                {
                    AlphaTextBox.Text = alpha.GetDouble().ToString("F3", CultureInfo.InvariantCulture);
                }
                if (root.TryGetProperty("DiscountFactor", out var gamma))
                {
                    GammaTextBox.Text = gamma.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                }
                if (root.TryGetProperty("ExplorationRate", out var epsilonStart))
                {
                    EpsilonStartTextBox.Text = epsilonStart.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                }
                if (root.TryGetProperty("EpsilonDecay", out var epsilonDecay))
                {
                    EpsilonDecayTextBox.Text = epsilonDecay.GetDouble().ToString("F5", CultureInfo.InvariantCulture);
                }
                if (root.TryGetProperty("EpsilonMin", out var epsilonMin))
                {
                    EpsilonMinTextBox.Text = epsilonMin.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                }
                if (root.TryGetProperty("ReportInterval", out var reportInterval))
                {
                    ReportIntervalTextBox.Text = reportInterval.GetInt32().ToString();
                }

                // Load image if path is available
                if (root.TryGetProperty("ImagePath", out var imagePath))
                {
                    string? imgPath = imagePath.GetString();
                    if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                    {
                        SetImage(imgPath);
                    }
                }

                string loadedConfigName = ConfigNameTextBox.Text;
                StatusText.Text = $"Configuration '{loadedConfigName}' loaded from {Path.GetFileName(openFileDialog.FileName)}";
                MessageBox.Show($"Configuration '{loadedConfigName}' loaded successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to load configuration.";
            }
        }
    }

    private void LoadModelButton_Click(object sender, RoutedEventArgs e)
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

                _loadedModelPath = openFileDialog.FileName;
                _loadedModelName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                // Display model information
                LoadedModelText.Text = $"Model: {_loadedModelName}";

                // Load Parameters from the model
                if (root.TryGetProperty("Parameters", out var parameters))
                {
                    if (parameters.TryGetProperty("Alpha", out var alpha))
                    {
                        AlphaTextBox.Text = alpha.GetDouble().ToString("F3", CultureInfo.InvariantCulture);
                    }
                    if (parameters.TryGetProperty("Gamma", out var gamma))
                    {
                        GammaTextBox.Text = gamma.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                    }
                    if (parameters.TryGetProperty("EpsilonStart", out var epsilonStart))
                    {
                        EpsilonStartTextBox.Text = epsilonStart.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                    }
                    if (parameters.TryGetProperty("EpsilonMin", out var epsilonMin))
                    {
                        EpsilonMinTextBox.Text = epsilonMin.GetDouble().ToString("F2", CultureInfo.InvariantCulture);
                    }
                    if (parameters.TryGetProperty("EpsilonDecay", out var epsilonDecay))
                    {
                        EpsilonDecayTextBox.Text = epsilonDecay.GetDouble().ToString("F5", CultureInfo.InvariantCulture);
                    }
                }

                // Load Metadata
                string metadataInfo = "";
                if (root.TryGetProperty("Metadata", out var metadata))
                {
                    if (metadata.TryGetProperty("Name", out var modelName))
                    {
                        ConfigNameTextBox.Text = modelName.GetString() ?? _loadedModelName;
                    }
                    if (metadata.TryGetProperty("Version", out var version))
                    {
                        metadataInfo += $"\nVersion: {version.GetString()}";
                    }
                    if (metadata.TryGetProperty("StateCount", out var stateCount))
                    {
                        metadataInfo += $"\nStates: {stateCount.GetInt32():N0}";
                    }
                    if (metadata.TryGetProperty("SaveDate", out var saveDate))
                    {
                        metadataInfo += $"\nSaved: {saveDate.GetString()}";
                    }
                }

                SetRobotImageByName(_loadedModelName ?? "");

                // Check for report files
                string directory = Path.GetDirectoryName(openFileDialog.FileName) ?? "";
                string htmlReportPath = Path.Combine(directory, $"{_loadedModelName}_report.html");
                string csvReportPath = Path.Combine(directory, $"{_loadedModelName}_report.csv");

                bool hasHtmlReport = File.Exists(htmlReportPath);
                bool hasCsvReport = File.Exists(csvReportPath);

                if (hasHtmlReport || hasCsvReport)
                {
                    ReportButtonsPanel.Visibility = Visibility.Visible;
                    OpenHtmlReportButton.IsEnabled = hasHtmlReport;
                    OpenCsvReportButton.IsEnabled = hasCsvReport;

                    string reportInfo = "";
                    if (hasHtmlReport && hasCsvReport)
                        reportInfo = "\nHTML and CSV reports available.";
                    else if (hasHtmlReport)
                        reportInfo = "\nHTML report available.";
                    else if (hasCsvReport)
                        reportInfo = "\nCSV report available.";

                    LoadedModelText.Text += metadataInfo + reportInfo;
                }
                else
                {
                    ReportButtonsPanel.Visibility = Visibility.Collapsed;
                    LoadedModelText.Text += metadataInfo;
                }

                StatusText.Text = $"Model loaded: {_loadedModelName} - Parameters and metadata loaded into UI";
                MessageBox.Show($"Model '{_loadedModelName}' loaded successfully!\n\n" +
                    $"Parameters loaded:\n" +
                    $"- Alpha (Learning Rate): {AlphaTextBox.Text}\n" +
                    $"- Gamma (Discount Factor): {GammaTextBox.Text}\n" +
                    $"- Epsilon Start: {EpsilonStartTextBox.Text}\n" +
                    $"- Epsilon Min: {EpsilonMinTextBox.Text}\n" +
                    $"- Epsilon Decay: {EpsilonDecayTextBox.Text}\n\n" +
                    $"HTML Report: {(hasHtmlReport ? "Available" : "Not found")}\n" +
                    $"CSV Report: {(hasCsvReport ? "Available" : "Not found")}",
                    "Model Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading model: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to load model.";
            }
        }
    }

    private void OpenHtmlReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_loadedModelName))
        {
            MessageBox.Show("No model loaded. Please load a model first.", "No Model",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(_loadedModelPath) ?? "";
            string htmlReportPath = Path.Combine(directory, $"{_loadedModelName}_report.html");

            if (File.Exists(htmlReportPath))
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = htmlReportPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                StatusText.Text = $"Opened HTML report for {_loadedModelName}";
            }
            else
            {
                MessageBox.Show($"HTML report not found:\n{htmlReportPath}", "File Not Found",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening HTML report: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenCsvReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_loadedModelName))
        {
            MessageBox.Show("No model loaded. Please load a model first.", "No Model",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(_loadedModelPath) ?? "";
            string csvReportPath = Path.Combine(directory, $"{_loadedModelName}_report.csv");

            if (File.Exists(csvReportPath))
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = csvReportPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                StatusText.Text = $"Opened CSV report for {_loadedModelName}";
            }
            else
            {
                MessageBox.Show($"CSV report not found:\n{csvReportPath}", "File Not Found",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening CSV report: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region UI-Sliders

    // Slider ValueChanged handlers
    private void EpisodesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || EpisodesTextBox == null) return;
        _isUpdatingFromSlider = true;
        EpisodesTextBox.Text = ((int)e.NewValue).ToString();
        _isUpdatingFromSlider = false;
    }

    private void MaxStepsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || MaxStepsTextBox == null) return;
        _isUpdatingFromSlider = true;
        MaxStepsTextBox.Text = ((int)e.NewValue).ToString();
        _isUpdatingFromSlider = false;
    }

    private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || AlphaTextBox == null) return;
        _isUpdatingFromSlider = true;
        AlphaTextBox.Text = e.NewValue.ToString("F3", CultureInfo.InvariantCulture);
        _isUpdatingFromSlider = false;
    }

    private void GammaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || GammaTextBox == null) return;
        _isUpdatingFromSlider = true;
        GammaTextBox.Text = e.NewValue.ToString("F2", CultureInfo.InvariantCulture);
        _isUpdatingFromSlider = false;
    }

    private void EpsilonStartSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || EpsilonStartTextBox == null) return;
        _isUpdatingFromSlider = true;
        EpsilonStartTextBox.Text = e.NewValue.ToString("F2", CultureInfo.InvariantCulture);
        _isUpdatingFromSlider = false;
    }

    private void EpsilonDecaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || EpsilonDecayTextBox == null) return;
        _isUpdatingFromSlider = true;
        EpsilonDecayTextBox.Text = e.NewValue.ToString("F5", CultureInfo.InvariantCulture);
        _isUpdatingFromSlider = false;
    }

    private void EpsilonMinSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || EpsilonMinTextBox == null) return;
        _isUpdatingFromSlider = true;
        EpsilonMinTextBox.Text = e.NewValue.ToString("F2", CultureInfo.InvariantCulture);
        _isUpdatingFromSlider = false;
    }

    private void ReportIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromTextBox || ReportIntervalTextBox == null) return;
        _isUpdatingFromSlider = true;
        ReportIntervalTextBox.Text = ((int)e.NewValue).ToString();
        _isUpdatingFromSlider = false;
    }

    // TextBox TextChanged handlers
    private void EpisodesTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || EpisodesSlider == null) return;
        if (int.TryParse(EpisodesTextBox.Text, out int value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= EpisodesSlider.Minimum && value <= EpisodesSlider.Maximum)
            {
                EpisodesSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void MaxStepsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || MaxStepsSlider == null) return;
        if (int.TryParse(MaxStepsTextBox.Text, out int value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= MaxStepsSlider.Minimum && value <= MaxStepsSlider.Maximum)
            {
                MaxStepsSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void AlphaTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || AlphaSlider == null) return;
        if (double.TryParse(AlphaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= AlphaSlider.Minimum && value <= AlphaSlider.Maximum)
            {
                AlphaSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void GammaTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || GammaSlider == null) return;
        if (double.TryParse(GammaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= GammaSlider.Minimum && value <= GammaSlider.Maximum)
            {
                GammaSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void EpsilonStartTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || EpsilonStartSlider == null) return;
        if (double.TryParse(EpsilonStartTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= EpsilonStartSlider.Minimum && value <= EpsilonStartSlider.Maximum)
            {
                EpsilonStartSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void EpsilonDecayTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || EpsilonDecaySlider == null) return;
        if (double.TryParse(EpsilonDecayTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= EpsilonDecaySlider.Minimum && value <= EpsilonDecaySlider.Maximum)
            {
                EpsilonDecaySlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void EpsilonMinTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || EpsilonMinSlider == null) return;
        if (double.TryParse(EpsilonMinTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= EpsilonMinSlider.Minimum && value <= EpsilonMinSlider.Maximum)
            {
                EpsilonMinSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    private void ReportIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromSlider || ReportIntervalSlider == null) return;
        if (int.TryParse(ReportIntervalTextBox.Text, out int value))
        {
            _isUpdatingFromTextBox = true;
            if (value >= ReportIntervalSlider.Minimum && value <= ReportIntervalSlider.Maximum)
            {
                ReportIntervalSlider.Value = value;
            }
            _isUpdatingFromTextBox = false;
        }
    }

    #endregion

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isTraining)
        {
            var result = MessageBox.Show("Training is in progress. Stop and go back?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            _cancellationTokenSource?.Cancel();
        }

        this.Close();
    }

}

