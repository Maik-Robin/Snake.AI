# AI Vision Control Usage Guide

## Overview
The `AIVisionControl` is a WPF UserControl that visualizes neural network inputs and outputs in real-time. It supports up to 30 inputs and 10 outputs with visual progress bars and color coding.

## Files Created
1. **Snake\Controls\AIVisionControl.xaml** - XAML layout
2. **Snake\Controls\AIVisionControl.xaml.cs** - Code-behind with data binding
3. **SnakeEngine\AI\AIVisionLabels.cs** - Helper for Snake AI labels
4. **Snake\AIVisionWindow.xaml** - Example window using the control
5. **Snake\AIVisionWindow.xaml.cs** - Example implementation

## Basic Usage

### 1. Add the control to your XAML
```xaml
<Window ...
        xmlns:controls="clr-namespace:Snake.Controls">
    
    <controls:AIVisionControl x:Name="VisionControl"/>
</Window>
```

### 2. Update the control from code
```csharp
using SnakeEngine.AI;

// Update inputs
double[] inputs = GetNeuralNetworkInputs();
VisionControl.UpdateInputs(inputs, AIVisionLabels.GetSnakeInputLabels());

// Update outputs (with selected action highlighted)
double[] outputs = GetNeuralNetworkOutputs();
int selectedAction = GetMaxOutputIndex(outputs);
VisionControl.UpdateOutputs(outputs, selectedAction, AIVisionLabels.GetSnakeOutputLabels());
```

### 3. Integrate with Snake AI Controller

Add to your game loop or AI evaluation:

```csharp
// In your game update loop
if (aiVisionWindow != null && aiVisionWindow.IsVisible)
{
    var gameState = GetCurrentGameState();
    var inputs = StateEncoder.Encode(gameState);
    var outputs = genome.FeedForward(inputs);
    int action = GetMaxIndex(outputs);
    
    aiVisionWindow.UpdateVision(inputs, outputs, action);
}
```

## Features

### Color Coding
- **Inputs:**
  - Red (Crimson): Danger indicators (indices 0-2)
  - Orange: Distance to tail (indices 3-5)
  - Gold: Food position (indices 6-7)
  - Blue: Direction encoding (indices 8-10)

- **Outputs:**
  - Green: Selected action
  - Blue: Other actions

### Visual Bars
- Bar width represents the normalized value (0-1)
- Values are displayed numerically with 3 decimal places
- Automatically scales to fit values

### Scrolling
- Both input and output sections have vertical scroll bars
- Supports up to 30 inputs and 10 outputs

## Example: Opening the Vision Window

```csharp
// In MainWindow.xaml.cs or similar
private AIVisionWindow? _aiVisionWindow;

private void ShowAIVision()
{
    if (_aiVisionWindow == null || !_aiVisionWindow.IsVisible)
    {
        _aiVisionWindow = new AIVisionWindow();
        _aiVisionWindow.Show();
    }
    else
    {
        _aiVisionWindow.Activate();
    }
}

// Add menu item or button to call ShowAIVision()
```

## Custom Labels

For non-Snake applications, provide your own labels:

```csharp
string[] customInputLabels = { "Input 1", "Input 2", ... };
string[] customOutputLabels = { "Action A", "Action B", ... };

VisionControl.UpdateInputs(inputs, customInputLabels);
VisionControl.UpdateOutputs(outputs, selectedIndex, customOutputLabels);
```

## Performance Tips

1. Update the control only when visible
2. Use reasonable update intervals (e.g., every 100ms instead of every frame)
3. The control uses ObservableCollection for efficient updates

## Styling

The control uses dependency properties and can be styled/customized:
- Change colors in `GetInputColor()` method
- Modify bar heights in XAML (Height property)
- Adjust font sizes in XAML templates
- Customize the border colors and thickness
