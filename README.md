
# <img src="https://raw.githubusercontent.com/wiki/grecosoft/NetFusion/img/NetFusion_Plug.jpg" width="32" height="32" /> NetFusion
## Composite Plugin Architecture for .NET Full and Core

[![Build status](https://ci.appveyor.com/api/projects/status/8k6l6lvmuulk2y94?svg=true)](https://ci.appveyor.com/project/grecosoft/netfusion)

![image](https://raw.githubusercontent.com/wiki/grecosoft/NetFusion/img/DotNetCore.png)

# Better Architecture Starts with a Better Framework

* Provides a well defined plug-in bootstrap process.
* Core bootstrap process is not dependent on any open-source technologies. This allows the core implementation to remain unchanged since new technologies and open-source libraries are introduced via plug-ins.
* Very loosely coupled. Only those plug-ins that a given host application needs are referenced. Provides a consistent methodology for unit-testing the application container and the interactions between its composite parts.
* Based mostly on convention and not configuration.
* Detailed logging of an application host’s composition - “A Picture is Worth a Thousand Words...”
* Not a heavyweight framework - “The whole is greater than the sum of its parts...”
* Once the bootstrap process is understood for one plug-in, the same process applies to all plug-ins. This process is also well documented. This allows new developers to become efficient in a short period of time.
* The core implementation is not dependent on the executing host assemblies. This greatly simplifies unit-testing and allows for complete execution in a variety of application hosts - LinqPad, .NET Core, …
* The only requirement placed on a plug-in is the implementation of the protocol required to integrate with the bootstrap process.
* While a plug-in design is ideal for implementing an “onion architecture” this is not a requirement.

# Getting Started

### [Project Wiki](https://github.com/grecosoft/NetFusion/wiki) - Documentation and Tutorials

### [Source Documentation](https://grecosoft.github.io/docs/netfusion/source/api/NetFusion.Bootstrap.Container.html) - Source Reference
