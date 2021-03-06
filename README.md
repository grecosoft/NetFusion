![IMAGE](./img/project_logo.jpg)

![Master Build & Publish](https://github.com/grecosoft/NetFusion/workflows/Master%20Build%20&%20Publish/badge.svg?branch=master)
![Microservice Template](https://github.com/grecosoft/NetFusion/workflows/Microservice%20Template/badge.svg)

![IMAGE](./img/lightbulb-icon.png)[**Quick Start**](./quick.start)

![IMAGE](./img/lightbulb-icon.png)[**Microservice Solution Template**](./microservice.solution.template)

![IMAGE](./img/lightbulb-icon.png)[**References**](./references)

## Problem

As more developers move towards implementing Microservice based solutions, it is important for these services to be quickly created and consistently configured.  Also, it is important that the overall structure be consistent between all Microservices that are part of a solution.  If not, each Microservice will vary overtime and become difficult to maintain and extend.

## Solution

**NetFusion** is a library enabling consistently between Microservices by providing the following:

* A well defined plug-in bootstrap process.
* Provides an implementation of the CQRS pattern that can be extended.
* Designed around Microservice architecture best practices.
* Suggests a solution structure based on Domain-Driven-Design (DDD) best practices.
* Has a core implementation not dependent on any open-source technologies.
* Introduces new technologies and open-source libraries by extending the core with plug-ins.
* Very loosely coupled. Only those plug-ins that a given host application utilizes are referenced.
* Provides a consistent methodology for unit-testing.
* Allows a complete Microservice to be build in seconds by providing a custom .NET Core CLI project template.

## Outcome

* Allows developers to focus on the domain of an application since technology concerns are encapsulated within reusable plugins shared between applications and developers.
* While the use of the CQRS implementation is not required, it provides a common message pipeline that can be extended by technology specific plug-ins such as RabbitMQ.
* Easy to use from the .NET Core Generic Host since the outcome of the bootstrap process is the population of the *IServiceCollection* with services provided by plug-ins.

## Lets Get Started

The Wiki documentation is ordered so each topic builds on the prior.  Each section discusses a specific topic with step-by-step code examples.  The topics use the solution generated by the **nf.micro-srv** template for discussion and as a starting point for implementing examples.   Before starting any of the topics, it is suggested that the section titled **[Microservice Solution Template](./microservice.solution.template)** is followed to create a starting Microservice solution using the **dotnet CLI**.  This solution can then be opened in the development IDE of your choice.
