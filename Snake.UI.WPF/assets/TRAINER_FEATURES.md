# TrainerWindow Features

## Overview
The TrainerWindow has been updated with a modern, professional UI that includes:

### 1. **Custom Image/Logo Display**
- **Location**: Left panel of the window
- **Default Image**: Automatically loads `Assets/snake_logo.png` if it exists
- **Custom Selection**: Click "Choose Custom Image" to select any image from your computer
- **Supported Formats**: PNG, JPG, JPEG, BMP
- **Purpose**: Add visual branding or motivation to your training sessions

### 2. **Training Parameters**
The right panel includes comprehensive training configuration options:

#### Core Training Settings
- **Number of Episodes**: Total training iterations (default: 100,000)
- **Max Steps Per Episode**: Maximum steps before episode terminates (default: 5,000)

#### Q-Learning Hyperparameters
- **Learning Rate (Alpha)**: How quickly the agent learns (0.0 - 1.0, default: 0.1)
- **Discount Factor (Gamma)**: Importance of future rewards (0.0 - 1.0, default: 0.99)
- **Initial Exploration Rate (Epsilon)**: Starting exploration probability (0.0 - 1.0, default: 1.0)
- **Exploration Decay Rate**: How fast exploration decreases (0.0 - 1.0, default: 0.995)

#### Monitoring
- **Progress Report Interval**: Show progress every N episodes (default: 100)

### 3. **Action Buttons**
- **Start Training**: Validates parameters and starts the training process
- **Save Configuration**: Exports current parameters to a JSON file for reuse

### 4. **Status Panel**
- Real-time status updates
- Shows current training state
- Displays loaded image information

## Design Features
- Modern card-based layout with shadows
- Responsive design with scroll support
- Hover effects on buttons
- Input validation with helpful error messages
- Color-coded UI elements (blue for primary actions, green for file operations, orange for save)

## Usage Tips
1. **First Time Setup**: Add a `snake_logo.png` file to the `Assets` folder for automatic loading
2. **Parameter Tuning**: Adjust learning rate and exploration parameters based on training performance
3. **Save Configurations**: Use "Save Configuration" to preserve successful parameter combinations
4. **Validation**: All inputs are validated before training starts to prevent errors

## Next Steps
To integrate actual training functionality:
1. Wire up the QLearningSnakeAgent with the configured parameters
2. Implement background threading for non-blocking training
3. Add progress bars and real-time metrics display
4. Implement training pause/resume functionality
