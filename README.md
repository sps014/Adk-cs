# Agent Development Kit (ADK) for .NET (C#)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE.md)
[![NuGet](https://img.shields.io/badge/NuGet-Preview-informational)](#-installation)

<html>
    <h2 align="center">
      <img src="https://raw.githubusercontent.com/google/adk-python/main/assets/agent-development-kit.png" width="256"/>
    </h2>
    <h3 align="center">
      An open-source, code-first .NET toolkit for building, evaluating, and deploying sophisticated AI agents with flexibility and control.
    </h3>
  
</html>

Agent Development Kit (ADK) is designed for developers seeking fine-grained
control and flexibility when building advanced AI agents that are tightly
integrated with services in Google Cloud. It allows you to define agent
behavior, orchestration, and tool use directly in code, enabling robust
debugging, versioning, and deployment anywhere – from your laptop to the cloud.

---

## ✨ Key Features

- **Rich Tool Ecosystem**: Utilize pre-built tools, custom functions, OpenAPI
  specs, or integrate existing tools to give agents diverse capabilities, all
  for tight integration with the Google ecosystem.
- **Code-First Development**: Define agent logic, tools, and orchestration
  directly in C# for ultimate flexibility, testability, and versioning.
- **Modular Multi-Agent Systems**: Design scalable applications by composing
  multiple specialized agents into flexible hierarchies.

## 🚀 Installation

This repo is currently built from source. Package publishing is coming soon.

## 📚 Documentation

For building, evaluating, and deploying agents, follow the docs and samples:

- **[Documentation](https://google.github.io/adk-docs)**
- **[Samples](https://github.com/google/adk-samples)**

## 🏁 Feature Highlight

### Same Features & Familiar Interface As Other ADKs:

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var rootAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "search_assistant",
    Description = "An assistant that can search the web.",
    ModelName = "gemini-2.5-flash",
    Instruction = "You are a helpful assistant. Answer user questions using Google Search when needed.",
    Tools = [ToolRegistry.GOOGLE_SEARCH],
});
```

### Development UI

Same as the Python Development UI. A built-in development UI to help you test,
evaluate, debug, and showcase your agent(s).

<img  alt="Image" src="https://github.com/user-attachments/assets/2b37352d-3f84-4090-bdaf-231c1325aaf6" />

### Evaluate Agents

Coming soon...

## 🤖 A2A and ADK integration

For remote agent-to-agent communication, ADK integrates with the
[A2A protocol](https://github.com/google/A2A/). See the A2A client/server
components under `src/GoogleAdk.Core/A2a` and the E2E tests under
`tests/GoogleAdk.E2e.Tests`.

## 🏗️ Building the Project

To set up the project and build it from source, follow these steps:

1. **Install dependencies**:

   ```bash
   dotnet restore "GoogleAdk/GoogleAdk.slnx"
   ```

1. **Build the project**:

   ```bash
   dotnet build "GoogleAdk/GoogleAdk.slnx"
   ```

## 🤝 Contributing

We welcome contributions from the community! Whether it's bug reports, feature
requests, documentation improvements, or code contributions, please see our
guidelines:

- [General contribution guideline and flow](https://google.github.io/adk-docs/contributing-guide/).
- Then if you want to contribute code, please read
  [Code Contributing Guidelines](./CONTRIBUTING.md) to get started.

## 📄 License and intellectual property

Google’s **Agent Development Kit (ADK)**—the reference product family (for example, the Python ADK), its documentation, and related Google materials—is a **copyrighted product of Google LLC** and/or its affiliates. Google retains applicable rights in that product and in the **ADK** and **Google** trademarks.

**This repository** is a **.NET port**: an open-source implementation aligned with ADK-style APIs and patterns. It is **not** described here as Google’s own first-party, directly Google-copyrighted release of ADK for .NET; copyright in this port belongs to **its contributors**, who license the code under the **Apache License, Version 2.0**. See **[LICENSE.md](LICENSE.md)** for the full terms, including how upstream ADK, this port, and trademarks are distinguished.

Use, reproduction, and distribution of this repository are governed by the **Apache License, Version 2.0**; it does **not** transfer ownership of Google’s ADK, Google’s trademarks, or third-party materials to you. Third-party components may be subject to separate licenses as noted in the applicable files or notices.

If you contribute code, your contributions may be subject to the contributor license arrangements described in the contribution documentation. Nothing in this README is legal advice or a waiver of Google’s or any third party’s rights except as stated in **[LICENSE.md](LICENSE.md)** and applicable licenses.

## Preview

This feature is subject to the "Pre-GA Offerings Terms" in the General Service
Terms section of the
[Service Specific Terms](https://cloud.google.com/terms/service-terms#1).
Pre-GA features are available "as is" and might have limited support. For more
information, see the
[launch stage descriptions](https://cloud.google.com/products?hl=en#product-launch-stages).

---

_Happy Agent Building!_
