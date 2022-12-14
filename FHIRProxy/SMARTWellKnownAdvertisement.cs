using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FHIRProxy
{
    public static class SMARTWellKnownAdvertisement
    {
        [FunctionName("KnownAdvertisementSMART")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fhir/.well-known/smart-configuration")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string aauth = req.Scheme + "://" + req.Host.Value + "/oauth2/authorize";
            string atoken = req.Scheme + "://" + req.Host.Value + "/oauth2/token";
            JObject config = await ADUtils.LoadOIDCConfiguration(ADUtils.GetIssuer(), log);
            if (config == null)
            {
                return new ContentResult() { Content = $"Error retrieving open-id configuration from {ADUtils.GetIssuer()}", StatusCode = 500, ContentType = "text/plain" };

            }
            //Override for Proxy Intercept of authorization/token issue and add capabilities for SMART
            config["token_endpoint"] = atoken;
            config["authorization_endpoint"] = aauth;
            JArray arr3 = new JArray();
			
			arr3.Add("launch-ehr");
            arr3.Add("launch-standalone");
            arr3.Add("client-public");
            arr3.Add("Patient Access for Standalone Apps");
            arr3.Add("Patient Access for EHR Launch");
            arr3.Add("Clinician Access for Standalone Apps");
            arr3.Add("Clinician Access for EHR Launch");
            arr3.Add("sso-openid-connect");
            arr3.Add("context-standalone-patient");
            arr3.Add("permission-offline");
            arr3.Add("permission-patient");
            arr3.Add("client-confidential-symmetric");

            config["capabilities"] = arr3;
            return new JsonResult(config);
        }
    }
}
