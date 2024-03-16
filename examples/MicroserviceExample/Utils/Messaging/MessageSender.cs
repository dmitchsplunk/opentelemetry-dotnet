// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Utils.Messaging;

public class MessageSender : IDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageSender));

    private readonly ILogger<MessageSender> logger;
    private readonly IConnection connection;
    private readonly IModel channel;

    public MessageSender(ILogger<MessageSender> logger)
    {
        this.logger = logger;
        this.connection = RabbitMqHelper.CreateConnection();
        this.channel = RabbitMqHelper.CreateModelAndDeclareTestQueue(this.connection);
    }

    public void Dispose()
    {
        this.channel.Dispose();
        this.connection.Dispose();
    }

    public string SendMessage()
    {
        try
        {
            var props = this.channel.CreateBasicProperties();
            var body = $"Published message: DateTime.Now = {DateTime.Now}.";

            this.channel.BasicPublish(
                exchange: RabbitMqHelper.DefaultExchangeName,
                routingKey: RabbitMqHelper.TestQueueName,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(body));

            this.logger.LogInformation($"Message sent: [{body}]");

            return body;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Message publishing failed.");
            throw;
        }
    }

    private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        try
        {
            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}
