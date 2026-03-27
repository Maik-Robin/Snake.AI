using Game.Core;
using Microsoft.Win32;
using Snake.AI.QLearning;
using Snake.AI.SnakeNEAT;
using Snake.Core;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Snake.AI;

/// <summary>
/// Training window that supports both Q-Learning and NEAT based Snake AI training.
/// </summary>
public partial class TrainerWindowV2 : Window
{
    /// <summary>
    /// Defines the available training algorithms.
    /// </summary>
    private enum AlgorithmMode { QLearning, NEAT }
    private AlgorithmMode _mode = AlgorithmMode.NEAT;

    private bool _isTraining = false;
    private CancellationTokenSource? _cts;

    private bool _syncingSlider = false;
    private bool _syncingTextBox = false;

    private string? _loadedModelPath;
    private string? _loadedModelName;

    private readonly ISnakeGameEnvironment _env;

    private readonly string _assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");

    /// <summary>
    /// Initializes a new trainer window instance.
    /// </summary>
    /// <param name="env">Game environment used during training episodes.</param>
    public TrainerWindowV2(ISnakeGameEnvironment env)
    {
        _env = env;
        InitializeComponent();
        LoadDefaultImage();
        RandomizeModelName();
        ApplyModeDefaults();
    }

    /// <summary>
    /// Handles algorithm selection and ensures only one toggle is active.
    /// </summary>
    /// <param name="sender">Toggle button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void AlgoToggle_Checked(object sender, RoutedEventArgs e)
    {
        if (sender == QLearningToggle)
        {
            if (NEATToggle != null) NEATToggle.IsChecked = false;
            _mode = AlgorithmMode.QLearning;
        }
        else
        {
            if (QLearningToggle != null) QLearningToggle.IsChecked = false;
            _mode = AlgorithmMode.NEAT;
        }
        ApplyModeDefaults();
    }

    /// <summary>
    /// Prevents both algorithm toggles from being unchecked at the same time.
    /// </summary>
    /// <param name="sender">Toggle button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void AlgoToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (QLearningToggle != null && NEATToggle != null &&
            QLearningToggle.IsChecked == false && NEATToggle.IsChecked == false)
            ((ToggleButton)sender).IsChecked = true;
    }

    /// <summary>
    /// Updates UI labels according to the currently selected algorithm mode.
    /// </summary>
    private void ApplyModeDefaults()
    {
        if (AlgorithmBadgeText == null) return;

        if (_mode == AlgorithmMode.QLearning)
        {
            AlgorithmBadgeText.Text = "Q-Learning";
            SubtitleText.Text = "Tabular Q-Learning — fast, interpretable, great for small state spaces.";
        }
        else
        {
            AlgorithmBadgeText.Text = "NEAT";
            SubtitleText.Text = "NeuroEvolution of Augmenting Topologies — evolves neural network structure and weights.";
        }
    }

    /// <summary>
    /// Starts model training using the currently selected algorithm.
    /// </summary>
    /// <param name="sender">Button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void StartTrainingButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(out string msg))
        {
            MessageBox.Show(msg, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetTrainingUI(true);
        Log($"[{DateTime.Now:HH:mm:ss}] Starting {_mode} training…");

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            if (_mode == AlgorithmMode.QLearning)
                await RunQLearningAsync(token);
            else
                await RunNEATAsync(token);
        }
        catch (OperationCanceledException)
        {
            Log("Training cancelled.");
            StatusText.Text = "Cancelled.";
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show($"Training error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Training failed.";
        }
        finally
        {
            SetTrainingUI(false);
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Requests cancellation of the current training run.
    /// </summary>
    /// <param name="sender">Button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void StopTrainingButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        Log($"[{DateTime.Now:HH:mm:ss}] Stop requested…");
        StatusText.Text = "Stopping…";
    }

    /// <summary>
    /// Executes Q-Learning training in a background task and updates progress in the UI.
    /// </summary>
    /// <param name="token">Cancellation token used to stop training.</param>
    private async Task RunQLearningAsync(CancellationToken token)
    {
        var config = BuildQLearningConfig();
        var modelName = ConfigNameTextBox.Text.Trim();
        var reportInterval = ParseInt(ReportIntervalTextBox.Text, 100);

        QLearningTrainer? trainer = null;

        await Task.Run(() =>
        {
            trainer = new QLearningTrainer(_env, config);
            var rng = new Random();
            double epsilon = config.EpsilonStart;

            for (int ep = 0; ep < config.Episodes; ep++)
            {
                token.ThrowIfCancellationRequested();

                var state = _env.Reset();
                string stateKey = QLearningTrainer.EncodeState(state);

                for (int step = 0; step < config.MaxStepsPerEpisode; step++)
                {
                    int action = rng.NextDouble() < epsilon
                        ? rng.Next(QTable.ActionCount)
                        : trainer.QTable.BestAction(stateKey);

                    var dir = Direction2DHelper.RelativeToDirection(state.Snake.CurrentDirection, (RelativeDirection2D)action);
                    var next = _env.Step(dir);
                    string nextKey = QLearningTrainer.EncodeState(next);

                    double reward = ComputeQLearningReward(state, next, config);
                    bool terminal = next.IsGameOver || next.IsGameVictory;

                    trainer.QTable.Update(stateKey, action, reward, nextKey, terminal, config);

                    state = next;
                    stateKey = nextKey;
                    if (terminal) break;
                }

                epsilon = Math.Max(config.EpsilonMin, epsilon * config.EpsilonDecay);

                if ((ep + 1) % reportInterval == 0 || ep == config.Episodes - 1)
                {
                    int ep1 = ep + 1;
                    double pct = (ep1 / (double)config.Episodes) * 100.0;
                    double eps = epsilon;
                    int states = trainer.QTable.StateCount;
                    Dispatcher.Invoke(() =>
                    {
                        TrainingProgress.Value = pct;
                        ProgressText.Text = $"{ep1:N0} / {config.Episodes:N0} episodes";
                        StatusText.Text = $"Episode {ep1:N0}  Epsilon={eps:F4}  States={states:N0}";
                        Log($"[{ep1:N0}] Epsilon={eps:F4}  States={states:N0}  ({pct:F1}%)");
                    });
                }
            }
        }, token);

        if (trainer != null && !token.IsCancellationRequested)
        {
            string savePath = Path.Combine(_assetsPath, $"{modelName}_qtable.json");
            trainer.SaveTrainingData(savePath);
            Log($"Q-table saved to {savePath}");
            StatusText.Text = $"Done! Saved to {Path.GetFileName(savePath)}";
            TrainingProgress.Value = 100;
            MessageBox.Show($"Training complete!\nStates learned: {trainer.QTable.StateCount:N0}\nSaved to:\n{savePath}",
                "Training Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Calculates reward for a Q-Learning step transition.
    /// </summary>
    /// <param name="current">Current game state.</param>
    /// <param name="next">Next game state after applying an action.</param>
    /// <param name="cfg">Active Q-Learning configuration.</param>
    /// <returns>Computed scalar reward value.</returns>
    private static double ComputeQLearningReward(SnakeGameState current, SnakeGameState next, QLearningConfig cfg)
    {
        if (next.IsGameVictory) return cfg.RewardVictory;
        if (next.IsGameOver)    return cfg.PenaltyDeath;
        if (current.EatsFood)   return cfg.RewardFood;
        return cfg.PenaltyStep;
    }

    /// <summary>
    /// Executes NEAT training in a background task and updates progress in the UI.
    /// </summary>
    /// <param name="token">Cancellation token used to stop training.</param>
    private async Task RunNEATAsync(CancellationToken token)
    {
        var modelName      = ConfigNameTextBox.Text.Trim();
        var config         = BuildNEATConfig();
        //var config = SnakeTrainer.GetConfig();
        //config.TargetFitness = ParseDouble(TargetFitnessTextBox.Text, 5000.0);
        //config.PopulationSize = ParseInt(PopSizeTextBox.Text, 200);
        //config.Generations = ParseInt(EpisodesTextBox.Text, 200);
        int reportInterval = ParseInt(ReportIntervalTextBox.Text, 10);

        await Task.Run(() =>
        {
            var algo = new MicroNEAT.Algorithm.NEATAlgorithm(config);

            int totalGen = config.Generations;
            int gen = 0;

            algo.OnGenerationComplete += (generation, best) =>
            {
                token.ThrowIfCancellationRequested();
                gen = generation;
                if (generation % reportInterval == 0 || generation == totalGen)
                {
                    double pct = (generation / (double)totalGen) * 100.0;
                    Dispatcher.Invoke(() =>
                    {
                        TrainingProgress.Value = pct;
                        ProgressText.Text = $"Gen {generation} / {totalGen}";
                        StatusText.Text = $"Generation {generation}  Best fitness={best:F1}";
                        Log($"[Gen {generation}] Best={best:F1}  ({pct:F1}%)");
                    });
                }
            };

            algo.RunAdvancedEvaluation();

            //var best = algo.GetBestGenomeFromPopulation();
            //var best = algo.GetBestGenomeFromRun();
            var best = algo.GetBestGenomeFromEvaluation();
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{modelName}_neat.json");
            MicroNEAT.Core.Genome.GenomeBuilder.SaveGenome(best, savePath);

            Dispatcher.Invoke(() =>
            {
                TrainingProgress.Value = 100;
                StatusText.Text = $"Done! Saved to {Path.GetFileName(savePath)}";
                Log($"NEAT complete. Best genome saved to {savePath}");
                MessageBox.Show($"NEAT training complete!\nBest genome saved to:\n{savePath}",
                    "Training Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }, token);
    }

    /// <summary>
    /// Builds a <see cref="QLearningConfig"/> from the current UI values.
    /// </summary>
    /// <returns>Configured Q-Learning settings.</returns>
    private QLearningConfig BuildQLearningConfig()
    {
        return new QLearningConfig
        {
            Episodes            = ParseInt(EpisodesTextBox.Text, 10000),
            MaxStepsPerEpisode  = ParseInt(MaxStepsTextBox.Text, 2000),
            LearningRate        = ParseDouble(AlphaTextBox.Text, 0.1),
            DiscountFactor      = ParseDouble(GammaTextBox.Text, 0.95),
            EpsilonStart        = ParseDouble(EpsilonStartTextBox.Text, 1.0),
            EpsilonMin          = ParseDouble(EpsilonMinTextBox.Text, 0.01),
            EpsilonDecay        = ParseDouble(EpsilonDecayTextBox.Text, 0.995),
            RewardFood          = ParseDouble(RewardFoodTextBox.Text, 10.0),
            PenaltyDeath        = ParseDouble(PenaltyDeathTextBox.Text, -10.0),
            PenaltyStep         = ParseDouble(PenaltyStepTextBox.Text, -0.01),
            RewardVictory       = ParseDouble(RewardVictoryTextBox.Text, 100.0),
        };
    }

    /// <summary>
    /// Builds a NEAT configuration from the current UI values.
    /// </summary>
    /// <returns>Configured NEAT settings.</returns>
    private MicroNEAT.Config.NEATConfig BuildNEATConfig()
    {
        var baseConfig = SnakeTrainer.GetConfig();

        baseConfig.Generations              = ParseInt(EpisodesTextBox.Text, 200);
        baseConfig.PopulationSize           = ParseInt(PopSizeTextBox.Text, 200);
        baseConfig.WeightMutationRate       = ParseDouble(WeightMutRateTextBox.Text, 0.8);
        baseConfig.AddConnectionMutationRate= ParseDouble(AddConnRateTextBox.Text, 0.1);
        baseConfig.AddNodeMutationRate      = ParseDouble(AddNodeRateTextBox.Text, 0.05);
        baseConfig.RecurrentConnectionRate  = ParseDouble(RecurrentRateTextBox.Text, 0.3);
        baseConfig.CompatibilityThreshold   = ParseDouble(CompatThresholdTextBox.Text, 3.0);
        baseConfig.SurvivalRate             = ParseDouble(SurvivalRateTextBox.Text, 0.2);
        baseConfig.PopulationStagnationLimit= ParseInt(StagnationLimitTextBox.Text, 30);
        baseConfig.NumOfElite               = ParseInt(EliteCountTextBox.Text, 15);
        baseConfig.TargetFitness            = ParseDouble(TargetFitnessTextBox.Text, 5000.0);

        return baseConfig;
    }

    /// <summary>
    /// Opens a file dialog and loads model metadata into the UI.
    /// </summary>
    /// <param name="sender">Button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void LoadModelButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Load Saved AI Model",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            InitialDirectory = Directory.Exists(_assetsPath) ? _assetsPath : AppDomain.CurrentDomain.BaseDirectory
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            _loadedModelPath = dlg.FileName;
            _loadedModelName = Path.GetFileNameWithoutExtension(dlg.FileName);
            ConfigNameTextBox.Text = _loadedModelName;
            LoadedModelText.Text = $"Loaded: {_loadedModelName}";
            StatusText.Text = $"Model loaded: {_loadedModelName}";
            SetRobotImageByName(_loadedModelName);
            Log($"Model loaded from: {dlg.FileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Toggles UI controls based on whether training is currently running.
    /// </summary>
    /// <param name="training">True when training is active; otherwise false.</param>
    private void SetTrainingUI(bool training)
    {
        _isTraining = training;
        StartTrainingButton.IsEnabled  = !training;
        StopTrainingButton.IsEnabled   = training;
        QLearningToggle.IsEnabled      = !training;
        NEATToggle.IsEnabled           = !training;
        if (!training) TrainingProgress.Value = 0;
    }

    /// <summary>
    /// Appends a message to the training log and scrolls to the newest entry.
    /// </summary>
    /// <param name="message">Message to log.</param>
    private void Log(string message)
    {
        LogTextBox.AppendText(message + Environment.NewLine);
        LogScrollViewer.ScrollToBottom();
    }

    /// <summary>
    /// Mirrors the configuration name into the model display label.
    /// </summary>
    /// <param name="sender">Text box that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void ConfigNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ModelNameText != null)
            ModelNameText.Text = ConfigNameTextBox.Text;
    }

    /// <summary>
    /// Closes the window and optionally confirms cancellation when training is active.
    /// </summary>
    /// <param name="sender">Button that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isTraining)
        {
            var result = MessageBox.Show("Training is in progress. Stop and close?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            _cts?.Cancel();
        }
        Close();
    }

    /// <summary>
    /// Loads default image assets for the header and training preview.
    /// </summary>
    private void LoadDefaultImage()
    {
        string defaultPath = Path.Combine(_assetsPath, "snake_logo.png");
        if (File.Exists(defaultPath))
            SetImage(defaultPath, TrainingImage);

        string headerPath = Path.Combine(_assetsPath, "snake_logo.png");
        if (File.Exists(headerPath))
            SetImage(headerPath, HeaderImage);
    }

    /// <summary>
    /// Picks a random robot image and generates a matching random model name.
    /// </summary>
    private void RandomizeModelName()
    {
        try
        {
            if (!Directory.Exists(_assetsPath)) return;
            var robots = Directory.GetFiles(_assetsPath, "robot_*.png");
            if (robots.Length == 0) return;

            var rng = new Random();
            string chosen = robots[rng.Next(robots.Length)];
            string name = Path.GetFileNameWithoutExtension(chosen).Replace("robot_", "");
            ConfigNameTextBox.Text = $"{name}{rng.Next(100, 1000)}";
            SetImage(chosen, TrainingImage);
        }
        catch
        {
            // Ignore image/name randomization issues; window can still be used.
        }
    }

    /// <summary>
    /// Selects a robot preview image that best matches the provided model name.
    /// </summary>
    /// <param name="name">Model name used to find a matching robot image.</param>
    private void SetRobotImageByName(string name)
    {
        if (!Directory.Exists(_assetsPath)) return;
        foreach (var f in Directory.GetFiles(_assetsPath, "robot_*.png"))
        {
            string robot = Path.GetFileNameWithoutExtension(f).Replace("robot_", "");
            if (name.Contains(robot, StringComparison.OrdinalIgnoreCase))
            {
                SetImage(f, TrainingImage);
                return;
            }
        }
    }

    /// <summary>
    /// Loads an image from disk and applies it to the specified image control.
    /// </summary>
    /// <param name="path">Absolute image file path.</param>
    /// <param name="target">Target image control.</param>
    private static void SetImage(string path, System.Windows.Controls.Image target)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            target.Source = bmp;
        }
        catch
        {
            // Ignore image loading failures to keep UI responsive.
        }
    }

    /// <summary>
    /// Validates user input for the currently selected algorithm.
    /// </summary>
    /// <param name="message">Validation message if input is invalid.</param>
    /// <returns>True when all required values are valid; otherwise false.</returns>
    private bool ValidateInputs(out string message)
    {
        message = string.Empty;

        if (!int.TryParse(EpisodesTextBox.Text, out int ep) || ep <= 0)
        { message = "Episodes must be a positive integer."; return false; }

        if (!int.TryParse(MaxStepsTextBox.Text, out int ms) || ms <= 0)
        { message = "Max Steps must be a positive integer."; return false; }

        if (_mode == AlgorithmMode.QLearning)
        {
            if (!double.TryParse(AlphaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double a) || a <= 0 || a > 1)
            { message = "Learning Rate (Alpha) must be in (0, 1]."; return false; }

            if (!double.TryParse(GammaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double g) || g < 0 || g > 1)
            { message = "Discount Factor (Gamma) must be in [0, 1]."; return false; }

            if (!double.TryParse(EpsilonDecayTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double ed) || ed <= 0 || ed >= 1)
            { message = "Epsilon Decay must be in (0, 1)."; return false; }
        }
        else
        {
            if (!int.TryParse(PopSizeTextBox.Text, out int pop) || pop < 2)
            { message = "Population size must be at least 2."; return false; }
        }

        return true;
    }

    /// <summary>
    /// Synchronizes a slider value to a text box representation.
    /// </summary>
    /// <param name="slider">Source slider.</param>
    /// <param name="box">Target text box.</param>
    /// <param name="format">Numeric format string.</param>
    private void SyncSliderToBox(Slider slider, TextBox box, string format = "G")
    {
        if (_syncingTextBox || box == null) return;
        _syncingSlider = true;
        box.Text = format == "int"
            ? ((int)slider.Value).ToString()
            : slider.Value.ToString(format, CultureInfo.InvariantCulture);
        _syncingSlider = false;
    }

    /// <summary>
    /// Synchronizes a text box value to a slider when parsing succeeds.
    /// </summary>
    /// <param name="box">Source text box.</param>
    /// <param name="slider">Target slider.</param>
    /// <param name="isInt">True for integer parsing; otherwise decimal parsing.</param>
    private void SyncBoxToSlider(TextBox box, Slider slider, bool isInt = false)
    {
        if (_syncingSlider || slider == null) return;
        if (isInt && int.TryParse(box.Text, out int iv))
        {
            _syncingTextBox = true;
            if (iv >= slider.Minimum && iv <= slider.Maximum) slider.Value = iv;
            _syncingTextBox = false;
        }
        else if (!isInt && double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double dv))
        {
            _syncingTextBox = true;
            if (dv >= slider.Minimum && dv <= slider.Maximum) slider.Value = dv;
            _syncingTextBox = false;
        }
    }

    /// <summary>
    /// Updates episode text when the episode slider changes.
    /// </summary>
    private void EpisodesSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(EpisodesSlider, EpisodesTextBox, "int");

    /// <summary>
    /// Updates max-steps text when the max-steps slider changes.
    /// </summary>
    private void MaxStepsSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(MaxStepsSlider, MaxStepsTextBox, "int");

    /// <summary>
    /// Updates report-interval text when the report-interval slider changes.
    /// </summary>
    private void ReportIntervalSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(ReportIntervalSlider, ReportIntervalTextBox, "int");

    /// <summary>
    /// Updates alpha text when the alpha slider changes.
    /// </summary>
    private void AlphaSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(AlphaSlider, AlphaTextBox, "F3");

    /// <summary>
    /// Updates gamma text when the gamma slider changes.
    /// </summary>
    private void GammaSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(GammaSlider, GammaTextBox, "F2");

    /// <summary>
    /// Updates epsilon-start text when the slider changes.
    /// </summary>
    private void EpsilonStartSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(EpsilonStartSlider, EpsilonStartTextBox, "F2");

    /// <summary>
    /// Updates epsilon-min text when the slider changes.
    /// </summary>
    private void EpsilonMinSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(EpsilonMinSlider, EpsilonMinTextBox, "F2");

    /// <summary>
    /// Updates epsilon-decay text when the slider changes.
    /// </summary>
    private void EpsilonDecaySlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(EpsilonDecaySlider, EpsilonDecayTextBox, "F4");

    /// <summary>
    /// Updates population-size text when the slider changes.
    /// </summary>
    private void PopSizeSlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
        => SyncSliderToBox(PopSizeSlider, PopSizeTextBox, "int");

    /// <summary>
    /// Updates the episode slider when episode text changes.
    /// </summary>
    private void EpisodesTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(EpisodesTextBox, EpisodesSlider, isInt: true);

    /// <summary>
    /// Updates the max-steps slider when max-steps text changes.
    /// </summary>
    private void MaxStepsTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(MaxStepsTextBox, MaxStepsSlider, isInt: true);

    /// <summary>
    /// Updates the report-interval slider when report-interval text changes.
    /// </summary>
    private void ReportIntervalTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(ReportIntervalTextBox, ReportIntervalSlider, isInt: true);

    /// <summary>
    /// Updates the alpha slider when alpha text changes.
    /// </summary>
    private void AlphaTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(AlphaTextBox, AlphaSlider);

    /// <summary>
    /// Updates the gamma slider when gamma text changes.
    /// </summary>
    private void GammaTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(GammaTextBox, GammaSlider);

    /// <summary>
    /// Updates the epsilon-start slider when text changes.
    /// </summary>
    private void EpsilonStartTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(EpsilonStartTextBox, EpsilonStartSlider);

    /// <summary>
    /// Updates the epsilon-min slider when text changes.
    /// </summary>
    private void EpsilonMinTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(EpsilonMinTextBox, EpsilonMinSlider);

    /// <summary>
    /// Updates the epsilon-decay slider when text changes.
    /// </summary>
    private void EpsilonDecayTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(EpsilonDecayTextBox, EpsilonDecaySlider);

    /// <summary>
    /// Updates the population-size slider when text changes.
    /// </summary>
    private void PopSizeTextBox_TextChanged(object s, TextChangedEventArgs e)
        => SyncBoxToSlider(PopSizeTextBox, PopSizeSlider, isInt: true);

    /// <summary>
    /// Parses an integer value or returns a fallback.
    /// </summary>
    /// <param name="text">Input text.</param>
    /// <param name="fallback">Fallback value.</param>
    /// <returns>Parsed integer value or fallback.</returns>
    private static int ParseInt(string text, int fallback)
        => int.TryParse(text, out int v) ? v : fallback;

    /// <summary>
    /// Parses a decimal value using invariant culture or returns a fallback.
    /// </summary>
    /// <param name="text">Input text.</param>
    /// <param name="fallback">Fallback value.</param>
    /// <returns>Parsed decimal value or fallback.</returns>
    private static double ParseDouble(string text, double fallback)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : fallback;
}
