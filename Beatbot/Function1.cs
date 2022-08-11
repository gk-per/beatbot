using System;
using System.Text.RegularExpressions;
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
           // log.LogInformation(requestBody);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Event @event = JsonConvert.DeserializeObject<Event>(JsonConvert.SerializeObject(data?.@event));
            Message message = new Message();
            @event.Message = message;
            string token = data?.token;
            string text = data?.@event?.text;
            Response response = new Response();
            response.Event = @event;
            response.Token = token;
            if (text != null)
            {
                log.LogInformation($"Found text: {text}");

                //if (text.StartsWith("<") && text.EndsWith(">"))
                //{
                //    text = text.Remove(0, 1);
                //    text = text.Remove(text.Length - 1, 1);
                //    log.LogInformation($"New text after remove <>: {text}");
                //}
                //@event.Message.Text = text.Remove(0,1);
                //if (Regex.Match(text, "^((?:https?:)?\\/\\/)?((?:www|m)\\.)?((?:youtube(-nocookie)?\\.com|youtu.be))(\\/(?:[\\w\\-]+\\?v=|embed\\/|v\\/)?)([\\w\\-]+)(\\S+)?$").Success)
                //{
                //    log.LogInformation($"Youtube link found: {text}");
                //}
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
