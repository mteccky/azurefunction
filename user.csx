#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
   log.LogInformation("User - HTTP trigger function processed a request.");

    await new StreamReader(req.Body).ReadToEndAsync();

    dynamic response = new JObject();
    response.name = "test";
    response.token = "1234-455662-22233333-3333";

    return new OkObjectResult(response.ToString());
}
