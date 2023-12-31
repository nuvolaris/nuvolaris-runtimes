using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OW;
using OWMicroservices;
using Newtonsoft.Json;

/// <summary>
/// Custom attribute for annotating OpenWhisk element parameters actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]

public class ServerAttribute : Attribute
{
    /// <summary>
    ///     Gets the name of the action.
    ///     The name is used to identify the where is located the api.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ActionAttribute class with the specified name.
    /// </summary>
    /// <param name="name"></param>
    public ServerAttribute(string name)
    {
        Name = name;
    }
}

public class AuthUriAttribute : Attribute
{
    /// <summary>
    ///     Gets the name of the AuthUriAttribute.
    ///     The name is used to identify the action for get jwt token.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the AuthUriAttribute class with the specified name.
    /// </summary>
    /// <param name="name"></param>
    public AuthUriAttribute(string name)
    {
        Name = name;
    }
}

public class ActionAttribute : Attribute
{
    /// <summary>
    ///     Gets the name of the action.
    ///     The name is used to identify the action in the OpenWhisk system.
    ///     The name is required and must be unique within the package that contains the action.
    ///     The name can be qualified using a namespace, such as /whisk.system/utils/echo.
    ///     The namespace must be unique within the system.
    ///     The name and namespace cannot be changed after an action is created.
    ///     The name must be at least 1 character and no more than 128 characters.
    ///     The name can contain letters (a-z), numbers (0-9), dashes (-), and underscores (_).
    ///     The name cannot begin with a dash.
    ///     The name cannot begin or end with an underscore.
    ///     The name cannot contain two or more consecutive dashes.
    ///     The name cannot contain two or more consecutive underscores.
    ///     The name cannot contain spaces.
    ///     The name cannot contain any other special characters.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ActionAttribute class with the specified name.
    /// </summary>
    /// <param name="name"></param>
    public ActionAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Custom attribute for annotating OpenWhisk element parameters packages.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PackageAttribute : Attribute
{
    /// <summary>
    ///    Gets the name of the package.
    ///    The name is used to identify the package in the OpenWhisk system.
    ///    The name is required and must be unique within the system.
    ///    The name cannot be changed after a package is created.
    ///    The name can be qualified using a namespace, such as /whisk.system/utils.
    ///    The namespace must be unique within the system.
    ///    The name and namespace cannot be changed after a package is created.
    ///    The name must be at least 1 character and no more than 128 characters.
    ///    The name can contain letters (a-z), numbers (0-9), dashes (-), and underscores (_).
    ///    The name cannot begin with a dash.
    ///    The name cannot begin or end with an underscore.
    ///    The name cannot contain two or more consecutive dashes.
    ///    The name cannot contain two or more consecutive underscores.
    ///    The name cannot contain spaces.
    ///    The name cannot contain any other special characters.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the PackageAttribute class with the specified name.
    /// </summary>
    public PackageAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Custom attribute for annotating APISIX element parameters routes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute : Attribute
{
    /// <summary>
    /// Gets the PATH of the route for APISIX system.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the RouteAttribute class with the specified name.
    /// </summary>
    public RouteAttribute(string name)
    {
        Name = name;
    }
}

namespace OW
{
    /// <summary>
    /// Provides utility functions to interact with OpenWhisk actions and Apisix routes.
    /// </summary>
    public static class OpenWhiskUtilities
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string contapiHost = "http://controller:3233";
        private static readonly string jwtApiKey = "edd1c9f034335f136f87ad84b625c8f1";
        private static readonly string apisixAdminPattern = "http://apisix-admin:9280/apisix/admin/routes/{0}";

        /// <summary>
        /// Helper method that extracts the value of a specified attribute property or returns a default error message.
        /// </summary>
        /// <typeparam name="T">The Attribute type to extract from.</typeparam>
        /// <param name="attributes">An IEnumerable of Attribute instances to search within.</param>
        /// <param name="attributeName">The name of the property to extract from the attribute.</param>
        /// <returns>The extracted value as a string or an error message.</returns>
        public static string GetAttributeValue(string attributeName)
        {
            Type type = typeof(MyMicroservicesOpenWhiskExecutor);

            switch (attributeName)
            {
                case "Action":
                    return type.GetCustomAttribute<ActionAttribute>()?.Name ?? $"ERRORE_{attributeName}_NON_TROVATO";
                case "Package":
                    return type.GetCustomAttribute<PackageAttribute>()?.Name ?? $"ERRORE_{attributeName}_NON_TROVATO";
                case "Route":
                    return type.GetCustomAttribute<RouteAttribute>()?.Name ?? $"ERRORE_{attributeName}_NON_TROVATO";
                case "AuthUri":
                    return type.GetCustomAttribute<AuthUriAttribute>()?.Name ?? $"ERRORE_{attributeName}_NON_TROVATO";
                case "Server":
                    return type.GetCustomAttribute<ServerAttribute>()?.Name ?? $"ERRORE_{attributeName}_NON_TROVATO";
                default:
                    return $"ERRORE_{attributeName}_NON_TROVATO";
            }

        }

        /// <summary>
        /// Extracts the line number from an exception's stack trace.
        /// </summary>
        /// <param name="ex">The Exception to analyze.</param>
        /// <returns>A nullable int representing the parsed line number, or null if it cannot be parsed.</returns>
        public static int? GetLineNumberFromStackTrace(Exception ex)
        {
            // Safely attempt to obtain the stack trace from the Exception object
            string? stackTrace = ex.StackTrace;
            if (!string.IsNullOrWhiteSpace(stackTrace))
            {
                // Attempt to locate and parse the line number from the stack trace
                int index = stackTrace.LastIndexOf("line ", StringComparison.OrdinalIgnoreCase);
                if (index != -1 && int.TryParse(stackTrace[(index + "line ".Length)..], out int lineNumber))
                {
                    return lineNumber;
                }
            }
            // Return null if the line number could not be determined
            return null;
        }

        /// <summary>
        /// Static constructor to initialize http client with default headers.
        /// </summary>
        static OpenWhiskUtilities()
        {
            client.DefaultRequestHeaders.Add("x-api-key", jwtApiKey);
        }

        /// <summary>
        /// Asynchronously sets up an Apisix route for an OpenWhisk function.
        /// </summary>
        /// <param name="descriptor">OpenWhiskFunctionDescriptor object containing details about the function.</param>
        /// <returns>A JObject containing the result of the setup operation.</returns>
        public static async Task<JObject> SetupRouteInApisix(OpenWhiskFunctionDescriptor descriptor)
        {
            var apisixAdminUrl = string.Format(apisixAdminPattern, descriptor.Name);

            // Read the template for the request body transformation from embedded resources.
            string embeddedtemplate = await ReadEmbeddedTemplate("OpenWhisk_MicroAPI.OWUtils");

            // Construct configuration object for the route setup.
            var routeConfig = new
            {
                plugins = new Dictionary<string, object>
                {
                    ["jwt-auth"] = new { _meta = new { disable = false } },
                    ["openwhisk"] = new
                    {
                        api_host = contapiHost,
                        service_token = descriptor.ApiKey,
                        @namespace = "nuvolaris",
                        action = descriptor.Action,
                        package = descriptor.Package,
                        result = descriptor.Result,
                        ssl_verify = descriptor.SslVerify,
                        timeout = descriptor.Timeout
                    },
                    ["body-transformer"] = new
                    {
                        request = new { template = embeddedtemplate }
                    }
                },
                methods = descriptor.HttpMethods,
                name = descriptor.Package + "-" + descriptor.Name,
                uri = $"{descriptor.Route}{descriptor.Action}"
            };

            // Serialize configuration object and send Put request to Apisix.
            var content = new StringContent(
                JObject.FromObject(routeConfig).ToString(),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PutAsync(apisixAdminUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Return an error object if response status is not successful.
                    return new JObject { ["error"] = await response.Content.ReadAsStringAsync(), ["sended_body"] = JObject.FromObject(routeConfig) };
                }
                var respcont = JObject.Parse(await response.Content.ReadAsStringAsync());
                var openapiSpec = GenerateOpenApiSpec(descriptor);
                // Return success object including the route configuration sent.
                return new JObject
                {
                    ["success"] = respcont,
                    ["openapi_spec"] = openapiSpec,
                    ["sended_body"] = JObject.FromObject(routeConfig)
                };
            }
            catch (HttpRequestException e)
            {
                // Catch and return HttpRequestException details as error.
                return new JObject { ["error"] = e.Message };
            }
        }

        /// <summary>
        /// Generates OpenAPI specification for the OpenWhisk function based on the setup result.
        /// </summary>
        /// <param name="setupResult">Result from setting up the Apisix route as JObject.</param>
        /// <param name="descriptor">Details of the OpenWhisk function.</param>
        /// <returns>OpenAPI Specification as JObject.</returns>
        public static JObject GenerateOpenApiSpec(OpenWhiskFunctionDescriptor descriptor)
        {
            string OpenApiPath = GeneratePathForMethod(descriptor);
            // Create the base structure for OpenAPI spec.
            var openApiSpec = new JObject
            {
                ["openapi"] = "3.0.0",
                ["info"] = new JObject { ["title"] = descriptor.Name, ["version"] = "1.0.0" },
                ["servers"] = new JArray(new JObject { ["url"] = descriptor.Server }),
                ["securitySchemes"] = new JObject // Aggiungi lo schema di sicurezza
                {
                    ["bearerAuth"] = new JObject
                    {
                        ["type"] = "http",
                        ["scheme"] = "bearer",
                        ["bearerFormat"] = "JWT",
                        ["description"] = $"JWT token can be obtained from {descriptor.Server}{descriptor.AuthUri}"
                    }
                }
            };
            openApiSpec["paths"] = new JObject{ [OpenApiPath] = new JObject()};

            // Generate path item object for each HTTP method supported.
            foreach (var method in descriptor.HttpMethods)
            {
                // Utilizzo di GeneratePathItem per ogni metodo
                JObject genpathitem = GeneratePathItem(method, descriptor);

                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    openApiSpec["paths"][OpenApiPath][method.ToLower()] = genpathitem;
                #pragma warning restore CS8602 // Dereference of a possibly null reference.

            }

            // Creare lo schema per FncDto
            var fncDtoSchema = new JObject();
            foreach (var prop in typeof(FncDto).GetProperties())
            {
                fncDtoSchema[prop.Name] = new JObject
                {
                    ["type"] = prop.PropertyType.Name.ToLower()
                };
            }


            openApiSpec["components"] = new JObject
            {
                ["schemas"] = new JObject
                {
                    ["FncDto"] = new JObject
                    {
                        ["schema"] = new JObject
                        {

                            ["type"] = "object",
                            ["properties"] = fncDtoSchema
                        }
                    }
                },
                ["security"] = new JArray(new JObject { ["bearerAuth"] = new JArray() }) // Aggiungi informazioni di sicurezza JWT
            };

            return openApiSpec;
        }

        /// <summary>
        /// Generates path item information for a given HTTP method.
        /// </summary>
        /// <param name="httpMethod">HTTP method for which to generate the path item.</param>
        /// <returns>JObject that represents the path item.</returns>
        private static JObject GeneratePathItem(string httpMethod, OpenWhiskFunctionDescriptor descriptor)
        {
            var requestbody = new JObject();
            var parameters = new JArray();
            if (httpMethod.ToLower() == "post")
            {
                parameters = null;
                requestbody = new JObject
                {
                    ["content"] = new JObject
                    {
                        ["application/json"] = new JObject
                        {
                            ["schema"] = new JObject{
                                ["$ref"] = "#/components/schemas/FncDto"
                            },
                            ["example"] = JObject.FromObject(new FncDto())
                        }
                    }
                };
            }
            if (httpMethod.ToLower() == "put")
            {
                parameters = null;
                requestbody = new JObject
                {
                    ["content"] = new JObject
                    {
                        ["application/json"] = new JObject
                        {
                            ["schema"] = new JObject{
                                ["$ref"] = "#/components/schemas/FncDto"
                            },
                            ["example"] = JObject.FromObject(new FncDto())
                        }

                    }
                };
            }
            if (httpMethod.ToLower() == "patch")
            {
                parameters = null;
                requestbody = new JObject
                {
                    ["content"] = new JObject
                    {
                        ["application/json"] = new JObject
                        {
                            ["schema"] = new JObject{
                                ["$ref"] = "#/components/schemas/FncDto"
                            },
                            ["example"] = JObject.FromObject(new FncDto())
                        }
                    }
                };
            }
            if (httpMethod.ToLower() == "delete")
            {
                requestbody = null;
                parameters = new JArray(new JObject
                {
                    ["in"] = "query",
                    ["name"] = typeof(FncDto).GetProperties()[0].Name,
                    ["schema"] = new JObject
                    {
                        ["type"] = typeof(FncDto).GetProperties()[0].PropertyType.Name.ToLower()
                    },
                    ["description"] = "Table ID"
                });

            }
            if (httpMethod.ToLower() == "get")
            {
                requestbody = null;
                parameters = new JArray(new JObject
                {
                    ["in"] = "query",
                    ["name"] = typeof(FncDto).GetProperties()[0].Name,
                    ["schema"] = new JObject
                    {
                        ["type"] = typeof(FncDto).GetProperties()[0].PropertyType.Name.ToLower()
                    },
                    ["description"] = "Table ID"
                });
            }
            // Costruisci l'oggetto path item con dettagli di riepilogo e risposta.
            var pathItem = new JObject
            {

                    ["tags"] = new JArray(descriptor.Package + "-" +  descriptor.Name),
                    ["operationId"] = $"{descriptor.Package}-{descriptor.Name}_{httpMethod.ToLower()}",
                    ["summary"] = $"API for {httpMethod.ToUpper()}",
                    ["description"] = $"API for {httpMethod.ToUpper()}",
                    ["parameters"] = new JArray(),
                    ["responses"] = new JObject
                    {
                        ["200"] = new JObject
                        {
                            ["description"] = "Successful response",
                            ["content"] = new JObject
                            {
                                ["application/json"] = new JObject
                                {
                                    ["schema"] = new JObject{
                                        ["$ref"] = "#/components/schemas/FncDto"
                                    },
                                    ["example"] = JObject.FromObject(new FncDto())
                                }
                            }
                        }
                    },
                    ["x-examples"] = new JObject()

            };

            if (requestbody is not null){
                    pathItem["requestBody"] = requestbody;
            }
            if (parameters is not null){
                    pathItem["parameters"] = parameters;
            }
            // Genera il comando cURL
            string curlExample = GenerateCurlExample(httpMethod, descriptor);
            if (pathItem["x-examples"] != null)
            {
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    pathItem["x-examples"]["curl"] = curlExample;
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            return pathItem;
        }

        private static string GenerateCurlExample(string httpMethod, OpenWhiskFunctionDescriptor descriptor)
        {
            // Genera l'URL completo per la chiamata API
            string apiUrl = $"{descriptor.Server}{descriptor.Route}{descriptor.Name}";

            // Costruisci il comando cURL
            var curlBuilder = new StringBuilder();
            curlBuilder.Append($"curl -X {httpMethod.ToUpper()} \"{apiUrl}\"");

            // Aggiungi headers
            curlBuilder.Append(" -H \"Content-Type: application/json\"");

            // Aggiungi autenticazione JWT o altri headers se necessario
            curlBuilder.Append(" -H \"Authorization: Bearer YOUR_JWT_TOKEN\"");

            // Determina e aggiunge il body della richiesta in base al metodo HTTP
            string requestBody;
            switch (httpMethod.ToLower())
            {
                case "put":
                case "patch":
                    // Serializza il DTO in formato JSON
                    requestBody = JsonConvert.SerializeObject(new FncDto()); // Usa il tuo DTO qui
                    break;
                case "post":
                    // Utilizza un body di esempio key-value per POST
                    requestBody = "{\"key1\":\"value1\", \"key2\":\"value2\"}";
                    break;
                default:
                    requestBody = "";
                    break;
            }

            // Aggiungi il body della richiesta se necessario
            if (!string.IsNullOrEmpty(requestBody))
            {
                curlBuilder.Append($" -d '{requestBody}'");
            }

            // Aggiungi autenticazione JWT o altri headers se necessario
            curlBuilder.Append(" -H \"Authorization: Bearer YOUR_JWT_TOKEN\"");

            return curlBuilder.ToString();
        }

        public static string GeneratePathForMethod( OpenWhiskFunctionDescriptor descriptor)
        {
            // Potresti voler modificare il percorso in base al metodo HTTP
            return $"{descriptor.Route}{descriptor.Name}";

        }


        /// <summary>
        /// Reads an embedded template file asynchronously.
        /// </summary>
        /// <param name="resourceFileName">The name of the resource file to be read.</param>
        /// <returns>The contents of the embedded resource file as a string.</returns>
        private static async Task<string> ReadEmbeddedTemplate(string resourceFileName)
        {

            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream($"{resourceFileName}.template.lua"))
            {
                if (stream == null)
                {
                    // Crea un StringBuilder per accumulare i nomi delle risorse
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("ERRORE_LETTURA_TEMPLATE: FILE NON TROVATO. " + $"{resourceFileName}.template.lua");

                    // Ottieni e aggiungi tutti i nomi delle risorse incorporate
                    string[] resourceNames = assembly.GetManifestResourceNames();
                    foreach (string resourceName in resourceNames)
                    {
                        sb.AppendLine(resourceName);
                    }

                    // Restituisce l'elenco delle risorse
                    return sb.ToString();
                }

                using (var reader = new StreamReader(stream))
                {
                    // Leggi e restituisci l'intero contenuto del file di risorsa incorporato
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
