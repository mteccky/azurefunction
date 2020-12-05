#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static async Task<HttpResponseMessage> Run(HttpRequest req, ILogger log)
{
   log.LogInformation("User - HTTP trigger function processed a request.");

    await new StreamReader(req.Body).ReadToEndAsync();

    dynamic responseObj = new JObject();
    responseObj.name = "Mona Khimani";
    responseObj.token = "1234-455662-22233333-3333";
    
    var response = new HttpResponseMessage()
    {
        Content = new StringContent(JsonConvert.SerializeObject(responseObj), System.Text.Encoding.UTF8, "application/json"),
        StatusCode = HttpStatusCode.OK,
    };

    return response;

}
