![image](./img/netfusionlogo.png)

# Composite Plugin Architecture for .NET Core

[![Build status](https://ci.appveyor.com/api/projects/status/8k6l6lvmuulk2y94?svg=true)](https://ci.appveyor.com/project/grecosoft/netfusion)

![image](https://raw.githubusercontent.com/wiki/grecosoft/NetFusion/img/DotNetCore.png)

## Objectives

*Great things are done by a series of small things that are brought together* - *Vincent Van Gogh*

* Provide a well defined plug-in bootstrap process.
* Design around microservice architecure best practices.
* Do not make the core dependent on any open-source technologies.
* Introduce new technologies and open-source libraries by extending the core using plugins.
* Must be very loosely coupled. Only those plug-ins that a given host application needs are referenced.
* Provide a consistent methodology for unit-testing.
* Should be based mostly on convention and not configuration. 

## Core Implementations

* The core bootstrap process is only dependent on Microsoft's set of abstractions:
  * Service Collection
  * Configuration Builder
  * LoggerFactory
* The core responsibility of the bootstrap process is to orchestrate the population of Microsoft's Service Collection from a set of discovered plugins.
* Provides detailed logging of the bootstrap process and allows the information to be queried using a REST API.
* Contains an implementation of the *CQRS* design pattern for in-process Commands, Queries and Domain Events.  This core implementation provides extension points that can be utilized to dispatch to other destinations such as RabbitMQ.
* Extends Microsoft's configuration abstractions by allowing options to be automatically discovered and registered with the service collection for associated section paths.  The application setting options are configured so they are validated and directly injected into dependent components.
* Provides structure for mapping one object representation to another but does not provided a specific implementation.  This allows manual mapping or the use of an open-source solution.  While the implementation might be different between microservices, the overall process remains the same.
* As with mapping, a structure is provided for object validation.  A simple interface used for specifying custom type validations and a default object validator, based on Microsoft's data annotations, is provided. An open-source specific implementation can be substituted but the overall validation process remains consistent.
* Provides a simple method for indicating that a typed object should also support dynamic properties at runtime.  Core classes such as the Command, Query, and Domain Event are based on this interface so they can be easily extended.  For example, a message enricher can tag a message with dynamic properties for all sent commands or published domain events.
* Provides a set of classes for defining scripts that can be evaluated against objects at runtime.  The implementation allows new calculated properties to be set and evaluated on any type supporting the dynamic property interface. 
