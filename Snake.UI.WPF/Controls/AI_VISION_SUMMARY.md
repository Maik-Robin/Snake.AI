# AI Vision System - Implementation Summary

## What Was Created

A complete WPF control system for visualizing neural network inputs and outputs in real-time for the Snake AI.

## Files Created

### 1. Core Control
- **`Snake\Controls\AIVisionControl.xaml`** - Visual layout for the control
- **`Snake\Controls\AIVisionControl.xaml.cs`** - Code-behind with full functionality

### 2. Helper Classes
- **`SnakeEngine\AI\AIVisionLabels.cs`** - Provides human-readable labels for Snake AI
- **`SnakeEngine\AI\AIVisionExtensions.cs`** - Extension methods for easy integration

### 3. Example Window
- **`Snake\AIVisionWindow.xaml`** - Standalone demo window
- **`Snake\AIVisionWindow.xaml.cs`** - Example implementation with test button

### 4. Documentation
- **`Snake\Controls\AIVisionControl_README.md`** - Complete usage guide

## Features

### Visual Display
? **Up to 30 inputs** - Scrollable list with labels, values, and visual bars
? **Up to 10 outputs** - Scrollable list with action highlighting
? **Color coding** - Different colors for different input types:
   - ?? Red: Danger indicators
   - ?? Orange: Distance to tail
   - ?? Gold: Food position
   - ?? Blue: Direction encoding
? **Selected action highlighting** - Green bar for chosen action
? **Real-time updates** - Uses ObservableCollection for efficient updates

### Snake AI Integration
? **11 labeled inputs**:
   - Danger Straight, Left, Right (3)
   - Distance to Tail Front, Left, Right (3)
   - Food Right, Food Down (2)
   - Direction: Up, Right, Down (3)

? **3 labeled outputs**:
   - Turn Left
   - Go Straight
   - Turn Right

## Quick Start

### Option 1: Standalone Demo Window
```csharp
// Open the demo window
var visionWindow = new AIVisionWindow();
visionWindow.Show();

// Click "Test Random Values" to see it in action
```

### Option 2: Integrate with Existing Game
```csharp
// Add to your MainWindow or game window
using SnakeEngine.AI;

// In XAML:
// <controls:AIVisionControl x:Name="VisionControl"/>

// In code (during game update):
var inputs = StateEncoder.Encode(gameState);
var outputs = neuralNetwork.FeedForward(inputs);
VisionControl.UpdateSnakeVision(inputs, outputs);
```

### Option 3: Add to Game Window as Panel
```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Game Canvas -->
    <Canvas Grid.Column="0" x:Name="GameCanvas"/>
    
    <!-- AI Vision Panel -->
    <controls:AIVisionControl Grid.Column="1" x:Name="VisionControl"/>
</Grid>
```

## API Reference

### AIVisionControl Methods

```csharp
// Update inputs with optional labels
void UpdateInputs(double[] inputs, string[]? labels = null)

// Update outputs with selection and optional labels
void UpdateOutputs(double[] outputs, int selectedIndex = -1, string[]? labels = null)

// Clear all data
void Clear()
```

### Extension Methods

```csharp
// Easy integration
control.UpdateSnakeVision(inputs, outputs, autoSelectMax: true)

// Get max output index
int selectedAction = outputs.GetMaxIndex()

// Convert action to index
int index = RelativeAction.TurnLeft.ToOutputIndex()

// Get action name
string name = RelativeAction.GoStraight.GetActionName()
```

## Integration Examples

### With SnakeNEATEvaluator
```csharp
public class SnakeNEATEvaluator : IFitnessFunction
{
    public AIVisionControl? VisionControl { get; set; }
    
    public double Evaluate(Genome genome)
    {
        // ... existing code ...
        
        if (VisionControl != null)
        {
            var inputs = StateEncoder.Encode(gameState);
            var outputs = genome.FeedForward(inputs);
            VisionControl.UpdateSnakeVision(inputs, outputs);
        }
        
        // ... continue evaluation ...
    }
}
```

### With Game Controller
```csharp
public class SnakeController
{
    private AIVisionWindow? _visionWindow;
    
    public void ShowVision()
    {
        _visionWindow = new AIVisionWindow();
        _visionWindow.Show();
    }
    
    public void UpdateStep()
    {
        var inputs = GetInputs();
        var outputs = GetOutputs();
        
        _visionWindow?.UpdateVision(inputs, outputs, selectedAction);
    }
}
```

## Customization

### Change Input Colors
Edit `GetInputColor()` method in `AIVisionControl.xaml.cs`:
```csharp
private Brush GetInputColor(int index)
{
    // Customize for your needs
    return index < 5 ? Brushes.Red : Brushes.Blue;
}
```

### Add More Inputs/Outputs
The control automatically handles up to 30 inputs and 10 outputs. Just pass larger arrays:
```csharp
double[] manyInputs = new double[30];
double[] manyOutputs = new double[10];
control.UpdateInputs(manyInputs, customLabels);
control.UpdateOutputs(manyOutputs, selectedIndex, customLabels);
```

### Custom Labels
```csharp
string[] myLabels = { "Custom 1", "Custom 2", ... };
control.UpdateInputs(inputs, myLabels);
```

## Testing

Run the demo:
```csharp
var demo = new AIVisionWindow();
demo.Show();
// Click "Test Random Values" button
```

## Next Steps

1. ? Control created and tested
2. ?? Integrate with MainWindow game loop
3. ?? Add toggle button to show/hide vision window
4. ?? Add to SnakeNEATEvaluator for training visualization
5. ?? Optional: Add recording/replay functionality

## Performance Notes

- Update frequency: Recommended 10-30 FPS (every 33-100ms)
- Uses WPF data binding for efficient updates
- Only update when window is visible for best performance

## Troubleshooting

**Issue**: Control not showing data
- Check that arrays are not null
- Verify labels array matches input/output count
- Ensure values are between 0 and 1 for proper bar display

**Issue**: Selection not highlighting
- Ensure selectedIndex is valid (0 to outputs.Length-1)
- Use -1 for no selection

**Issue**: Performance lag
- Reduce update frequency
- Only update when vision window is visible
- Consider updating every N frames instead of every frame
