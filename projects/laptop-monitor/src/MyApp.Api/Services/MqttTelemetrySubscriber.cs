using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace MyApp.Api.Services;

public sealed class MqttTelemetrySubscriber(
    IConfiguration configuration,
    ILogger<MqttTelemetrySubscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var username = configuration["MQTT_USER"];
        var password = configuration["MQTT_PASSWORD"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogError(
                "MQTT_USER or MQTT_PASSWORD is missing. Subscriber not started.");

            return;
        }

        var factory = new MqttClientFactory();

        using var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithClientId("myapp-api-local")
            .WithTcpServer("127.0.0.1", 1883)
            .WithCredentials(username, password)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCleanSession()
            .Build();

        client.ApplicationMessageReceivedAsync += args =>
        {
            logger.LogInformation(
                "MQTT message received on {Topic}: {Payload}",
                args.ApplicationMessage.Topic,
                args.ApplicationMessage.ConvertPayloadToString());

            return Task.CompletedTask;
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await client.ConnectAsync(options, stoppingToken);

                var subscription = factory
                    .CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(filter => filter
                        .WithTopic("telemetry/+/samples")
                        .WithQualityOfServiceLevel(
                            MqttQualityOfServiceLevel.AtLeastOnce))
                    .Build();

                var result = await client.SubscribeAsync(
                    subscription, stoppingToken);

                foreach (var item in result.Items)
                {
                    if ((int)item.ResultCode >= 128)
                    {
                        throw new InvalidOperationException(
                            $"MQTT subscription rejected: {item.ResultCode}");
                    }
                }

                logger.LogInformation(
                    "MQTT connected. Subscribed to telemetry/+/samples.");

                while (client.IsConnected &&
                       !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "MQTT connection interrupted. Retrying in 5 seconds.");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
