These are message types published from command-handlers contained within the App project and handled by other application
service components.  Domain Events are published to notify other application components of an updated resource.

Messaging:      Messaging:  https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview#messaging---overview
Domain-Events:  https://github.com/grecosoft/NetFusion/wiki/core.messaging.domain-events#messaging---domain-events

A domain-vent can have multiple in-process message handlers and can be delivered out of process using the following 
infrastructure plug-ins:

NetFusion.RabbitMq (Direct, Topic, Fan-out)
    https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.direct#rabbitmq---direct-exchanges
    https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.topic#rabbitmq---topic-exchanges
    https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.fanout#rabbitmq---fan-out-exchanges

NetFusion.AMQP (Topics)               
    https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#publishing-domain-event-to-topic

NetFusion.Redis (Channels)                     
    https://github.com/grecosoft/NetFusion/wiki/integration.redis.overview#redis-pubsub
