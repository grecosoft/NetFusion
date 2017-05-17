namespace NetFusion.RabbitMQ
{
    public class RabbitMqLogEvents
    {
        public const int BROKER_EXCEPTION = 5000;

        public const int BROKER_CONFIGURATION = 5001;
        public const int PUBLISHER_CONFIGURATION = 5002;
        public const int CONSUMER_CONFIGURATION = 5003;

        public const int PUBLISHER_RPC_RESPONSE = 5004;
        public const int PUBLISHER_RPC_REQUEST = 5005;

        public const int MESSAGE_PUBLISHER = 5006;
        public const int MESSAGE_CONSUMER = 5007;
    }
}
