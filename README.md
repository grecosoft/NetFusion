
# <img src="https://raw.githubusercontent.com/wiki/grecosoft/NetFusion/img/NetFusion_Plug.jpg" width="54" height="40" /> NetFusion-Core
## Composite Plugin Architecture for .NET Full and Core

[![Build status](https://ci.appveyor.com/api/projects/status/8k6l6lvmuulk2y94?svg=true)](https://ci.appveyor.com/project/grecosoft/netfusion)

![image](https://raw.githubusercontent.com/wiki/grecosoft/NetFusion/img/DotNetCore.png)

### "Great things are done by a series of small things that are brought together" </br>
    Vincent Van Gogh

The NetFusion open-source project consists of three repositories:

* [NetFusion-Core](https://github.com/grecosoft/NetFusion) - Composite-Application Bootstrap
    * The contents of this repository contain the core implementation of the application bootstrap process.  
    * Builds a composite-application from a set of referenced plug-ins.
    * This repository does not contain any reference to technology specific implementations.
    * Technology specific implementations are added by creating specific plug-ins.

* [NetFusion-Plugins](https://github.com/grecosoft/NetFusion-Plugins) - Technology Specific Plug-ins (Optional)
    * Contains technology dependent plug-ins that can be added to applications based on NetFusion-Core.
    * All of the plug-in implementations contained within this repository are 100% optional.

* [NetFusion-Tools](https://github.com/grecosoft/NetFusion-Tools) - Web Clients, Templates, and Examples
    * UtilityViewer - Angular5 based client consisting of modules providing developer utilities:
        * REST/HAL Viewer: Allowing developers to easy test REST/HAL based Web APIs.
        * Composite-App Viewer: Allows developers to view how a NetFusion based application is composted from plug-ins.  
        * Mircoservice Template: A "dotnet new" based template creating a solution based on microservice best-pratices.

## Objectives

* Provides a well defined plug-in bootstrap process.
* Core bootstrap process is not dependent on any open-source technologies. This allows the core implementation to remain unchanged since new technologies and open-source libraries are introduced via plug-ins.
* Very loosely coupled. Only those plug-ins that a given host application needs are referenced. 
* Provides a consistent methodology for unit-testing the application container and the interactions between its composite parts.
* Based mostly on convention and not configuration.
* Detailed logging of an application host’s composition.
* Not a heavyweight framework.
* Once the bootstrap process is understood for one plug-in, the same process applies to all plug-ins.
* The only requirement placed on a plug-in is the implementation of modules to integrate with the bootstrap process.
* While a plug-in design is ideal for implementing an “onion architecture” this is not a requirement.

## Getting Started

* [Project Wiki](https://github.com/grecosoft/NetFusion/wiki) - Documentation and Tutorials

