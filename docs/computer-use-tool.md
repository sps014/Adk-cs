# Computer Use Tool

The `ComputerUseTool` allows LLMs to perform automated, screen-based interactions. It bridges text-based AI models with graphical user interfaces, enabling the agent to "see" the screen, click elements, and type text.

## Overview

The `ComputerUseTool` relies on an `IComputerDriver` that abstracts browser automation.
- **`PlaywrightComputerDriver`**: Drives Chromium, captures viewports, and executes real mouse/keyboard events.
- **`ConsoleComputerDriver`**: A lightweight, text-only driver for HTTP scraping.

## Usage

Instantiate a driver, wrap it in a `ComputerUseToolset`, and assign it to the agent.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;
using GoogleAdk.Samples.ComputerUse.Drivers;

// 1. Initialize the driver
var driver = new PlaywrightComputerDriver();
await driver.InitializeAsync();

// 2. Attach to the agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "automation_agent",
    Model = "gemini-2.5-flash",
    Instruction = "You are a web automation bot. Use computer_use to interact with elements.",
    Tools = [ new ComputerUseToolset(driver) ]
});
```

### LLM Commands

The LLM issues JSON commands (e.g., `{"action": "left_click", "coordinate": [450, 300]}`) which the `ComputerUseToolset` automatically maps to the driver's `ClickAsync`, `TypeAsync`, and `PressKeyAsync` methods.

*Important:* Ensure you call `await driver.CloseAsync();` when shutting down to clean up processes.