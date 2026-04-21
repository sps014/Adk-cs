# Computer Use Tool

The `ComputerUseTool` allows LLMs (like `gemini-2.5-flash` or Anthropic's Computer Use models) to perform automated, screen-based interactions. It bridges the gap between text-based AI models and graphical user interfaces by enabling the agent to "see" the screen, move the mouse, click elements, and type text.

## Overview

The `ComputerUseTool` relies on an `IComputerDriver` interface that abstracts the underlying OS or Browser automation system. The ADK provides built-in drivers:
- **`PlaywrightComputerDriver`**: Drives an actual Chromium browser, captures real viewport screenshots, and executes actual mouse/keyboard events.
- **`ConsoleComputerDriver`**: A lightweight, text-only driver for HTTP scraping without a GUI.

## Usage

First, instantiate your chosen driver and initialize it. Then, wrap it inside a `ComputerUseToolset` (which exposes the specific commands to the LLM) and assign it to the agent.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;
using GoogleAdk.Samples.ComputerUse.Drivers; // Depending on your driver location

// 1. Initialize the driver
var driver = new PlaywrightComputerDriver();
await driver.InitializeAsync();

// 2. Create the toolset
var toolset = new ComputerUseToolset(driver);

// 3. Attach to the agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "automation_agent",
    Model = "gemini-2.5-flash",
    Instruction = "You are a web automation bot. Use the computer_use tool to navigate to websites, read the screen, and interact with elements.",
    Tools = [ toolset ]
});
```

## How It Works

When the LLM decides to interact with the computer, it issues a function call to the `ComputerUseToolset`. The toolset translates LLM actions (like `left_click`, `type`, `mouse_move`) into corresponding driver commands.

The agent uses virtual coordinates (often scaled) to determine where to click based on its visual understanding of the screenshots (if you are utilizing vision capabilities) or the accessibility tree. The driver translates these to the physical dimensions of the screen or viewport.

### Example LLM Commands:
The LLM might issue JSON commands such as:
- `{"action": "left_click", "coordinate": [450, 300]}`
- `{"action": "type", "text": "Hello world"}`
- `{"action": "key", "text": "Return"}`

The `ComputerUseToolset` automatically maps these to the driver's `ClickAsync`, `TypeAsync`, and `PressKeyAsync` methods.

## Example

For a complete working example, including a full Playwright implementation, see the [Computer Use Sample](../samples/GoogleAdk.Samples.ComputerUse) in the repository.

*Important:* Make sure to call `await driver.CloseAsync();` when your application shuts down to clean up browser processes or OS hooks.