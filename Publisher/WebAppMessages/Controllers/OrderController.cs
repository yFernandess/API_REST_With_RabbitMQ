﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using WebAppMessages.Domain;

namespace WebAppMessages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        public IActionResult InsertOrder(Order order)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "OrderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "OrderQueue",
                                         basicProperties: null,
                                         body: body);
                }
                return Accepted(order);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar criar um novo pedido", ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
