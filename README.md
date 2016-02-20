# NetFusion
NetFusion is a design for composing an application from components that remains constant as an application grows with newly added features and technologies.

##Goals
* Allow an application to be easily composed from loosely coupled reusable components.
* Provide a well-defined application bootstrap process.
* Provide a pluggable architecture that can be easily configured.
* Provide a design that is non-obstructive and does not dictate how an application’s specific business logic is implemented or structured.  This allows each plugin to be designed and structured independently.
* Provided detailed log of how the application was composed..



##Core Infrastructure
The core infrastructure was implemented using the following open-source technologies:

* MEF (Microsoft Extensibility Framework)
* Autofac
* Serilog
* XUnit
* FluentAssertions


**[Bootstrap](./docs/infrastructure/bootstrap.md)**

Provides an overview of the AppContainer bootstrap process and explains how to create a basic configuration.  

**[Logging](./docs/infrastructure/logging.md)**

Provides an overview of how to configure logging for the AppContainer and application components.

**[Configuration](./docs/infrastructure/configuration.md)**

Provides and overview of how to specify and declare container configurations.

**[Settings](./docs/infrastructure/settings.md)**

Provides and overview of how to define and configure application settings.


**[Eventing](./docs/infrastructure/eventing.md)**

Provides an overview of how to create, publish and consume application domain events.

##Infrastructure Plugins
Built on top of the bootstrap process are plug-ins for the following open-source technologies:

* [MongoDB](./docs/plugins/mongodb.md)
* [EntityFramework](./docs/plugins/entityframework.md)
* [RabbitMQ](./docs/plugins/rabbitmq)
* [WebApi](./docs/plugins/webapi.md)
* [JWT (Java Web Token)](./docs/plugins/jwt.md)
