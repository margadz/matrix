﻿using Common.Consts;
using Common.UOW;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace MQProducer.MQ
{
    public class MQClient : IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        public MQClient()
        {
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
            channel.QueueDeclare(queue: Queues.MessageQueue, autoDelete:true, exclusive:false);
            channel.QueueDeclare(queue: Queues.NotificationQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: true,
                                 arguments: null);
            consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: Queues.NotificationQueue, noAck: false, consumer: consumer);
            
        }

        public void Call(string message)
        {
            var corelationId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = Queues.ReponseQueue;
            props.CorrelationId = corelationId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            Console.WriteLine(string.Format("sending {0}", message));
            channel.BasicPublish(exchange: "", routingKey: Queues.MessageQueue, basicProperties: props, body: messageBytes);

            //consumer.Received += (model, ea) =>
            //                {
            //                    var body = ea.Body;
            //                    var reply = Encoding.UTF8.GetString(body);
            //                    Console.WriteLine("reply " + reply);
            //                };
            //Console.ReadLine();
        }

        public void Call(byte[] message)
        {
            var corelationId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = Queues.ReponseQueue;
            props.CorrelationId = corelationId;

            Console.WriteLine("Sending raw bytes.");
            channel.BasicPublish(exchange: "", routingKey: Queues.MessageQueue, basicProperties: props, body: message);
        }

        public void Call(UnitOfWork uow)
        {
            var corelationId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = Queues.ReponseQueue;
            props.CorrelationId = corelationId;

            Console.WriteLine(string.Format("Sending UOW: {0}", uow.ToString()));
            channel.BasicPublish(exchange: "", routingKey: Queues.MessageQueue, basicProperties: props, body: UnitOfWork.ToBytes(uow));
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
