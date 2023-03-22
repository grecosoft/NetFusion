These are message types sent from controllers contained within the WebApi project and handled within the App project.  
Commands are sent to alter the state of resources managed by the application and can have an optional return type.

Messaging:  https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview#messaging---overview
Commands:   https://github.com/grecosoft/NetFusion/wiki/core.messaging.commands#messaging---commands

A command can have only one in-process message handler and can be delivered out of process using the following 
infrastructure plug-ins:

NetFusion.RabbitMq  [https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.overview#rabbitmq-overview]
NetFusion.AMQP      [https://github.com/grecosoft/NetFusion/wiki/integration.amqp.overview#amqp-overview]   

