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
using Azure;
using Azure.AI.TextAnalytics;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Npgsql;

namespace Beatbot
{
    public static class Function1
    {

        private static readonly Uri endpoint =
            new Uri("https://sentibot.cognitiveservices.azure.com/");

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
            };
            var client = new SecretClient(new Uri("https://brandkeys.vault.azure.net/"), new DefaultAzureCredential(), options);

            KeyVaultSecret pgHostSecret = client.GetSecret("PGHOST");
            string pgHost = pgHostSecret.Value;
            KeyVaultSecret pgUserSecret = client.GetSecret("PGUSER");
            string pgUser = pgUserSecret.Value;
            KeyVaultSecret pgPasswordSecret = client.GetSecret("PGPASSWORD");
            string pgPassword = pgPasswordSecret.Value;
            string pgDatabase = pgUser;
            KeyVaultSecret sentimentSecret = client.GetSecret("SENTIMENTKEY");

            AzureKeyCredential credentials = new AzureKeyCredential(sentimentSecret.Value);

            await using var db = new NpgsqlConnection($"Host={pgHost};Username={pgUser};Password={pgPassword};Database={pgDatabase}");

            var INSERT_items = $"insert into sentiment_data (user_id, score) values (@userId, @score);";
            int duplicates = 0;

            await db.OpenAsync();
            await using (var cmd = new NpgsqlCommand(INSERT_items, db))
            {
                cmd.Parameters.AddWithValue("@userId", "greg");
                cmd.Parameters.AddWithValue("@score", 1);
                await cmd.ExecuteNonQueryAsync();
            }

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
                var textAnalyticsClient = new TextAnalyticsClient(endpoint, credentials);
                DocumentSentiment documentSentiment = textAnalyticsClient.AnalyzeSentiment(text);
                var sentiment = documentSentiment.Sentences;
                var score = 0;
                foreach (var sentence in sentiment)
                {
                   if (sentence.Sentiment.ToString() == "Positive") 
                   {
                        score++;
                   } 
                   else if (sentence.Sentiment.ToString() == "Negative")
                   {
                        score--;
                   } 
                   else
                   {
                       continue;
                   }
                }



                //SentimentAnalysisExample(client, text);


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

        //static void SentimentAnalysisExample(TextAnalyticsClient client, string inputText)
       // {
            // Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");
           // System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", documentSentiment.Sentiment.ToString());

           // foreach (var sentence in documentSentiment.Sentences)
           // {
                //System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", $"\tText: {sentence.Text}");
                //System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", $"\tSentence Sentiment: {sentence.Sentiment.ToString()}");
                //System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", $"\tPositive Score: {sentence.ConfidenceScores.Positive.ToString(CultureInfo.InvariantCulture)}");
                //System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", $"\tNegative Score: {sentence.ConfidenceScores.Negative.ToString(CultureInfo.InvariantCulture)}");
                //System.IO.File.AppendAllText("/Users/stansbury/Desktop/sentiment.txt", $"\tNeutral Score: {sentence.ConfidenceScores.Positive.ToString(CultureInfo.InvariantCulture)}\n");

                // Console.WriteLine($"\tText: \"{sentence.Text}\"");
                // Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
                // Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
                // Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
                // Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
            //}
        //}
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
