﻿using Common.Consts;
using Common.Results;
using DataGenerator.Container;
using MQReceiver.Logger;
using MQReceiver.Matrix;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

namespace MQReceiver.MQ
{
    public class MQClient : IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private MatrixAssembler assembler;
        private WorkerTimeLogger logger;

        public MQClient(MatrixAccessor container, WorkerTimeLogger logger)
        {
            this.logger = logger;
            assembler = new MatrixAssembler(container);

            var factory = new ConnectionFactory()
            {
                HostName = MQServerConfig.IP,
                Port = MQServerConfig.PORT,
                UserName = MQServerConfig.USER,
                Password = MQServerConfig.USER,
                VirtualHost = MQServerConfig.VHOST
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: Queues.ReponseQueue, exclusive: false);
            channel.QueueDeclare(queue: Queues.NotificationQueue,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: true,
                                             arguments: null);
            consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: Queues.ReponseQueue, noAck: false, consumer: consumer);

        }

        public void Run()
        {
            bool theEnd = false;
            Console.WriteLine("Receiver running...");

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    CalculationResult result = CalculationResult.GetFromBytes(ea.Body);
                    var props = ea.BasicProperties;
                    var id = props.CorrelationId;
                    if (result.Row % 32 == 0)
                    {
                        Console.WriteLine(string.Format("Received result, adding to assembly. Row: {0}", result.Row));
                    }
                    logger.LogWorkerTime(result);
                    if (assembler.AddResult(result))
                    {
                        channel.BasicPublish(exchange: "", routingKey: Queues.NotificationQueue, body: new byte[] { 1 });
                        theEnd = assembler.FinalAssembly;
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
            while (!theEnd)
            {

            }
            channel.BasicPublish(exchange: "", routingKey: Queues.NotificationQueue, body: new byte[] { 1 });
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
            if (channel != null)
            {
                channel.Dispose();
            }
        }
    }
}
