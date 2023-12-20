using Newtonsoft.Json.Linq;
using OW;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace OWMicroservices
{
    // Class to perform OpenWhisk operations
    [Package("PrototipeMicroservices")]
    [Action("PrototipeFNC")]
    [Route("/prototipe/api/v1/")]
    public class MyMicroservicesOpenWhiskExecutor
    {
        // Static repository instance
        private static FncDtoRepository? _repository;

        // Main method to execute operations based on the HTTP method
        public static JObject Execute(JObject args)
        {
            try
            {
                _repository = new FncDtoRepository(args?.ToString() ?? string.Empty);
                FncDto? fncDto = null;
                // Configure APISIX if required
                var setupResult = PluginConfig.SetupApisixIfRequired(args!).GetAwaiter().GetResult();
                if (setupResult["error"] != null)
                {
                    return AddResultInBody(setupResult);
                }

                if (args != null)
                {
                    fncDto = args.ToObject<FncDto>();
                }



                string? httpMethod = args?["method"]?.ToString().ToUpper();

                JObject result = httpMethod switch
                {
                    // Handles different HTTP operations using a switch expression, awaited asynchronously
                    "GET" => MyMicroservicesVerbsModel.HandleGet(Guid.Parse(args!["id"]!.ToString()), _repository!).GetAwaiter().GetResult(),
                    "PUT" => MyMicroservicesVerbsModel.HandlePut(fncDto! , _repository!).GetAwaiter().GetResult(),
                    "DELETE" => MyMicroservicesVerbsModel.HandleDelete(Guid.Parse(args!["id"]!.ToString()) , _repository!).GetAwaiter().GetResult(),
                    "PATCH" => MyMicroservicesVerbsModel.HandlePatch(fncDto!, _repository!).GetAwaiter().GetResult(),
                    "POST" => HandlePost(args!).GetAwaiter().GetResult(),
                    // Default case for unsupported HTTP methods
                    _ => new JObject
                    {
                        ["error"] = "HTTP method not supported" + httpMethod,
                        ["args"] = args
                    },
                };

                // Wraps the result in a response body
                return AddResultInBody(result);
            }
            catch (Exception ex)
            {
                // Handles exceptions during execution and provides detailed error message
                return AddResultInBody(new JObject
                {
                    ["error"] = $"Error during action execution: {ex.Message}, line: {(ex.StackTrace != null ? ExtractLineNumber(ex.StackTrace) : "N/A")}"
                });
            }
        }

        // Handles the POST method by determining which action should be taken based on additional parameters.
        private static async Task<JObject> HandlePost(JObject args)
        {
            // Safe navigation used to prevent null reference issues
            string? createTable = args["createtable"]?.ToString().ToUpper();
            string? truncateTable = args["truncatetable"]?.ToString().ToUpper();
            string? dropTable = args["droptable"]?.ToString().ToUpper();
            string? query = args["query"]?.ToString();

            // Check if the 'createtable' command is present and equals 'YES'
            if (createTable == "YES")
            {
                // Handle table creation
                return await MyMicroservicesVerbsModel.HandleCreateTable(_repository!);
            }
            else  if (dropTable == "YES")
            {
                // Handle table deletion
                return await MyMicroservicesVerbsModel.HandleDropTable(_repository!);
            }
            else if (truncateTable == "YES")
            {
                // Handle table truncation
                return await MyMicroservicesVerbsModel.HandleTruncateTable(_repository!);
            }
            else if (query != null)
            {
                // Handle query execution if a query parameter is provided
                return await MyMicroservicesVerbsModel.HandleExecuteQuery(args, _repository!);
            }

            // If no known commands are found, return an error indicating that the POST method is not supported
            return new JObject
            {
                ["error"] = "POST method not supported",
                ["message"] = "Metodo POST non supportato",
                ["args"] = args
            };
        }

        // Extracts line number from stack trace string safely
        private static string ExtractLineNumber(string stackTrace)
        {
            return stackTrace?.Substring(stackTrace.LastIndexOf("line") + 4) ?? "N/A";
        }

        // Private method not exposed on OpenWhisk apisix to get the list of environment variables
        public static JObject GetEnvVars()
        {
            var envVars = Environment.GetEnvironmentVariables();
            JObject result = new JObject();

            // Iterating over each entry in environment variables dictionary
            foreach (DictionaryEntry entry in envVars)
            {
                // Casting key and value of DictionaryEntry to strings and adding them to the result object
                var key = entry.Key.ToString();
                var value = entry.Value?.ToString() ?? string.Empty;
                #pragma warning disable CS8604 // Possible null reference argument.
                    result[key] = value!;
                #pragma warning restore CS8604 // Possible null reference argument.
            }

            return result;
        }

        // Adds the provided JObject as the 'body' of the response payload
        private static JObject AddResultInBody(JObject body)
        {
            // Encapsulates the body within another JObject under the 'body' key
            return new JObject { ["body"] = body };
        }
    }
}
