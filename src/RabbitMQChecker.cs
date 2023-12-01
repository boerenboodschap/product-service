using RabbitMQ.Client;

public class RabbitMQChecker
{
    public static bool IsRabbitMQAvailable(string hostName, string userName, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        try
        {
            using (var connection = factory.CreateConnection())
            {
                // Connection established successfully
                return true;
            }
        }
        catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
        {
            // Unable to connect to RabbitMQ broker
            return false;
        }
    }
}
