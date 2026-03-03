using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Snake.Controls
{
    /// <summary>
    /// Control for visualizing AI neural network inputs and outputs.
    /// Displays up to 30 inputs and 10 outputs with visual bars.
    /// </summary>
    public partial class AIVisionControl : UserControl
    {
        public ObservableCollection<NeuronData> Inputs { get; }
        public ObservableCollection<NeuronData> Outputs { get; }

        public AIVisionControl()
        {
            InitializeComponent();
            Inputs = new ObservableCollection<NeuronData>();
            Outputs = new ObservableCollection<NeuronData>();
            
            InputsItemsControl.ItemsSource = Inputs;
            OutputsItemsControl.ItemsSource = Outputs;
        }

        /// <summary>
        /// Updates the input values with labels.
        /// </summary>
        /// <param name="inputs">Array of input values (max 30).</param>
        /// <param name="labels">Array of labels for each input (optional).</param>
        public void UpdateInputs(double[] inputs, string[]? labels = null)
        {
            if (inputs == null || inputs.Length == 0)
                return;

            int maxInputs = Math.Min(inputs.Length, 30);

            // Update existing or add new
            for (int i = 0; i < maxInputs; i++)
            {
                string label = labels != null && i < labels.Length ? labels[i] : $"Input {i}";
                double value = inputs[i];

                if (i < Inputs.Count)
                {
                    Inputs[i].Value = value;
                    Inputs[i].Label = label;
                }
                else
                {
                    Inputs.Add(new NeuronData
                    {
                        Label = label,
                        Value = value,
                        MaxWidth = 150,
                        BarColor = GetInputColor(i)
                    });
                }
            }

            // Remove excess items
            while (Inputs.Count > maxInputs)
            {
                Inputs.RemoveAt(Inputs.Count - 1);
            }
        }

        /// <summary>
        /// Updates the output values with labels and highlights the selected action.
        /// </summary>
        /// <param name="outputs">Array of output values (max 10).</param>
        /// <param name="selectedIndex">Index of the selected output (-1 for none).</param>
        /// <param name="labels">Array of labels for each output (optional).</param>
        public void UpdateOutputs(double[] outputs, int selectedIndex = -1, string[]? labels = null)
        {
            if (outputs == null || outputs.Length == 0)
                return;

            int maxOutputs = Math.Min(outputs.Length, 10);

            // Update existing or add new
            for (int i = 0; i < maxOutputs; i++)
            {
                string label = labels != null && i < labels.Length ? labels[i] : $"Output {i}";
                double value = outputs[i];
                bool isSelected = i == selectedIndex;

                if (i < Outputs.Count)
                {
                    Outputs[i].Value = value;
                    Outputs[i].Label = label;
                    Outputs[i].IsSelected = isSelected;
                }
                else
                {
                    Outputs.Add(new NeuronData
                    {
                        Label = label,
                        Value = value,
                        MaxWidth = 150,
                        IsSelected = isSelected,
                        BarColor = isSelected ? Brushes.LimeGreen : Brushes.SteelBlue
                    });
                }
            }

            // Remove excess items
            while (Outputs.Count > maxOutputs)
            {
                Outputs.RemoveAt(Outputs.Count - 1);
            }
        }

        /// <summary>
        /// Clears all inputs and outputs.
        /// </summary>
        public void Clear()
        {
            Inputs.Clear();
            Outputs.Clear();
        }

        private Brush GetInputColor(int index)
        {
            // Color code different input types
            if (index < 3) return Brushes.Crimson;      // Danger indicators
            if (index < 6) return Brushes.Orange;       // Distance to tail
            if (index < 8) return Brushes.Gold;         // Food position
            return Brushes.CornflowerBlue;              // Direction encoding
        }
    }

    /// <summary>
    /// Data model for a single neuron (input or output).
    /// </summary>
    public class NeuronData : DependencyObject
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(NeuronData), 
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NeuronData), 
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.Register(nameof(MaxWidth), typeof(double), typeof(NeuronData), 
                new PropertyMetadata(150.0));

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register(nameof(BarWidth), typeof(double), typeof(NeuronData), 
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty BarColorProperty =
            DependencyProperty.Register(nameof(BarColor), typeof(Brush), typeof(NeuronData), 
                new PropertyMetadata(Brushes.SteelBlue));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(NeuronData), 
                new PropertyMetadata(false, OnIsSelectedChanged));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double MaxWidth
        {
            get => (double)GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public double BarWidth
        {
            get => (double)GetValue(BarWidthProperty);
            set => SetValue(BarWidthProperty, value);
        }

        public Brush BarColor
        {
            get => (Brush)GetValue(BarColorProperty);
            set => SetValue(BarColorProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NeuronData neuron)
            {
                double value = (double)e.NewValue;
                // Clamp value between 0 and 1
                double normalizedValue = Math.Max(0, Math.Min(1, value));
                neuron.BarWidth = normalizedValue * neuron.MaxWidth;
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NeuronData neuron)
            {
                bool isSelected = (bool)e.NewValue;
                neuron.BarColor = isSelected ? Brushes.LimeGreen : Brushes.SteelBlue;
            }
        }
    }

    /// <summary>
    /// Converter for making selected outputs bold.
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
