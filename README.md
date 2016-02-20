# NetFusion
NetFusion is a design for composing an application from components that remains constant as an application grows with newly added features and technologies.

[![Build status](https://ci.appveyor.com/api/projects/status/8k6l6lvmuulk2y94?svg=true)](https://ci.appveyor.com/project/grecosoft/netfusion)

## Goals
* Allow an application to be easily composed from loosely coupled reusable components.
* Provide a well-defined application bootstrap process.
* Provide a pluggable architecture that can be easily configured.
* Provide a design that is non-obstructive and does not dictate how an application’s specific business logic is implemented or structured.  This allows each plugin to be designed and structured independently.
* Provided detailed log of how the application was composed..

## Core
The core infrastructure was implemented using the following open-source technologies:

* MEF (Microsoft Extensibility Framework)
* Autofac
* Serilog
* XUnit
* FluentAssertions

**[Bootstrap](./src/NetFusion/NetFusion.Bootstrap/README.md)**
Provides an overview of the AppContainer bootstrap process and explains how to create a basic configuration.  

**[Eventing](./src/NetFusion/NetFusion.Eventing/README.md)**
Provides an overview of how to create, publish and consume application domain events.

**[Settings](./src/NetFusion/NetFusion.Settings/README.md)**
Provides and overview of how to define and configure application settings.

## Infrastructure
Built on top of the bootstrap process are plug-ins for the following open-source technologies:

**[EntityFramework](./src/NetFusion/NetFusion.EntityFramework/README.md)**

**[MongoDB](./src/NetFusion/NetFusion.MongoDB/README.md)**

**[RabbitMQ]./src/NetFusion/NetFusion.RabbitMQ/README.md)**

**[WebApi](./src/NetFusion/NetFusion.WebApi/README.md)**
