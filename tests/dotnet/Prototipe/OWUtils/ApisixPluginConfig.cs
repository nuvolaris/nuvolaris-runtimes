using Newtonsoft.Json.Linq;
using OWMicroservices;
using System;
using System.Reflection;
using System.Threading.Tasks;

// Define the namespace for Organziation Works (OW)
namespace OW
{
    // PluginConfig class to handle Plugin Configuration and setup specific tasks.
    public class PluginConfig
    {
        // Properties are unchanged as their usage is not clear from the snippet.

        /// <summary>
        /// Asynchronously sets up the APIs in APISIX if required.
        /// </summary>
        /// <param name="args">The JObject containing the necessary arguments.</param>
        /// <returns>A JObject indicating the success or error state of the operation.</returns>
        public static async Task<JObject> SetupApisixIfRequired(JObject args)
        {
            // Retrieve all custom attributes associated with MyMicroservicesOpenWhiskExecutor class at once.
            var attributes = typeof(MyMicroservicesOpenWhiskExecutor).GetCustomAttributes();

            // Extracting specific attribute values for action name, package name, and route name
            string actionName = OpenWhiskUtilities.GetAttributeValue("Action");
            string packageName = OpenWhiskUtilities.GetAttributeValue("Package");
            string routeName = OpenWhiskUtilities.GetAttributeValue("Route");
            string authUri = OpenWhiskUtilities.GetAttributeValue("AuthUri");
            string server = OpenWhiskUtilities.GetAttributeValue("Server");

            try
            {
                // Check if 'setupapisix' argument is provided and equals "True"

                // Prepare descriptor based on extracted attributes and additional settings
                var descriptor = new OpenWhiskFunctionDescriptor
                (
                    httpMethods: new[] { "GET", "PUT", "DELETE", "PATCH", "POST" },
                    route: routeName,
                    name: actionName,
                    package: packageName,
                    action: actionName,
                    result: true,
                    sslVerify: false,
                    timeout: 60000,
                    apiKey: args["API_KEY"]?.ToString() ?? "ERROR_API_KEY_NOT_FOUND",
                    server: server,
                    authUri: authUri
                );

                // Execute the setup routine with the prepared descriptor
                if (args["generateopenapi"]?.ToString().ToUpper() == "TRUE")
                {
                    return OpenWhiskUtilities.GenerateOpenApiSpec(descriptor);
                }
                else
                if (args["setupapisix"]?.ToString().ToUpper() == "TRUE")
                {
                    return await OpenWhiskUtilities.SetupRouteInApisix(descriptor);
                }



                // If setup is not required, return a simple JObject indicating it.
                return JObject.FromObject(new { result = "Setup non richiesto" });
            }
            catch (Exception ex)
            {
                // In case of an exception, return error information including the message and line number from the stack trace.
                return JObject.FromObject(new
                {
                    error = $"Errore durante l'esecuzione del metodo SetupApisixIfRequired: {ex.Message}, riga: {OpenWhiskUtilities.GetLineNumberFromStackTrace(ex)}"
                });
            }
        }
    }
}
