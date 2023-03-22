Entities are simple C# classes modeling the business logic contained within the solution
being implemented.  Domain-entities should not have dependencies on application services.
Instead, Command and Domain-Event handlers, contained within the App project, should load
change, and save entities required to handle the received command or domain-event.  The
command and domain-event handlers will delegate to application services and repositories.