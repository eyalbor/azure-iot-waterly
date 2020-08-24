using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Configuration;

namespace Bill
{
    public static class Function1
    {
        [FunctionName("pay_bill")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bill")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("pay the bill for user ");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<WaterlyBillReq>(requestBody);

                //send the email

                // requires using System.Net.Http;
                var client = new HttpClient();
                log.LogInformation(requestBody);

                HttpResponseMessage result = await client.PostAsync(
                    // requires using System.Configuration;
                    "https://prod-26.eastus.logic.azure.com:443/workflows/06a66aa325a84a29b64f788ff1537d50/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-f_3sTNheCjl7dSq3dZzCuqkYChEXDcweiK92DVv_KU",
                    new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json"));

                var statusCode = result.StatusCode.ToString();

                if (statusCode != "200")
                {
                    return new BadRequestObjectResult(statusCode);
                } else {
                    return new OkObjectResult(statusCode);
                }    
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }

        }
    }
}

public class WaterlyBillReq
{
    public string email { get; set; }
    public string invoice { get; set; }
    public float amount { get; set; }
    public string task { get; set; }
}


