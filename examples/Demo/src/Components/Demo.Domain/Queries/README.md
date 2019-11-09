Queries are simple C# classes used to express the retrieval of resources managed by an application service.
Queries are most often dispatched from controllers contained within the WebApi project.  Queries are handled
by components contained within the App project and return domain-entities or models constructed from domain-entities.
Often the handlers will not load complete domain-entities but rather query specific views for increased performance.

Messaging:  https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview#messaging---overview
Queries:    https://github.com/grecosoft/NetFusion/wiki/core.queries.overview#query---overview
