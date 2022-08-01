using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Beatbot
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Event @event = JsonConvert.DeserializeObject<Event>(JsonConvert.SerializeObject(data?.@event));
            string token = data?.token;
            string text = data?.@event?.message?.text;
            Response response = new Response();
            response.Event = @event;
            response.Token = token;
            if (text != null)
            {
                log.LogInformation($"Found text: {text}");
                response.Event.Message.Text = text;
            }

            return new OkObjectResult(response);

        }
    }

    public class Response
    {
        public string Token { get; set; }
        public Event Event { get; set; }
    }

    public class Event {
        public Message Message { get; set; }
    
    }

    public class Message {
        public string Text { get; set; }

    }
}
