using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Test.API.Controllers
{
    public class Request
    {
        public string Token { get; set; }
        public string AnyData { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(Request request)
        {
            //check auth token
            if (string.IsNullOrWhiteSpace(request?.Token) || request?.Token != "a72ae55b-7b31-4393-9764-a4ea5501290c") return Unauthorized();

            //check data
            if (string.IsNullOrWhiteSpace(request?.AnyData)) return BadRequest();

            //produce message into rabbitmq
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

            var body = Encoding.UTF8.GetBytes(request.AnyData);
            channel.BasicPublish(exchange: "logs",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);

            return Accepted();
        }
    }

}
