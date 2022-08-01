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
            Event event = data?.event;
            Response response = new Response();
            response.event = event;

            return new OkObjectResult(response);
        }
    }

    public class Response
    {
        public Event event { get; set; }
}

    public class Event {
        public Message message { get; set; }
    

    }

    public class Message {
        public string text { get; set; }

    }
}
