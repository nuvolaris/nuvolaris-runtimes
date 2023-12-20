namespace OW
{
    /// <summary>
    /// Represents a descriptor for an OpenWhisk function, detailing how it can be accessed and invoked.
    /// </summary>
    public class OpenWhiskFunctionDescriptor
    {
        // An array of HTTP methods supported by the OpenWhisk action.
        public string[] HttpMethods { get; set; }

        // The route pattern that this OpenWhisk action responds to.
        public string Route { get; set; }

        // The name identifier of the OpenWhisk action.
        public string Name { get; set; }

        // The package where the OpenWhisk action is located.
        public string Package { get; set; }

        // The specific action to execute within the OpenWhisk framework.
        public string Action { get; set; }

        // A flag indicating whether only the result of the action should be returned without the full OpenWhisk metadata.
        public bool Result { get; set; }

        // A flag indicating whether SSL verification should be performed on requests.
        public bool SslVerify { get; set; }

        // The timeout for the OpenWhisk action invocation in milliseconds.
        public int Timeout { get; set; }

        // The API key to authenticate against the OpenWhisk API.
        public string ApiKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the OpenWhiskFunctionDescriptor class with specified details.
        /// </summary>
        /// <param name="httpMethods">An array of supported HTTP methods.</param>
        /// <param name="route">The route pattern to match.</param>
        /// <param name="name">The action's name.</param>
        /// <param name="package">The package containing the action.</param>
        /// <param name="action">The specific action to call.</param>
        /// <param name="result">Flag for returning only action results.</param>
        /// <param name="sslVerify">Flag for SSL verification requirement.</param>
        /// <param name="timeout">Timeout duration for the action.</param>
        /// <param name="apiKey">API key for authentication.</param>
        public OpenWhiskFunctionDescriptor(
            string[] httpMethods,
            string route,
            string name,
            string package,
            string action,
            bool result,
            bool sslVerify,
            int timeout,
            string apiKey)
        {
            HttpMethods = httpMethods ?? throw new ArgumentNullException(nameof(httpMethods));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Package = package ?? throw new ArgumentNullException(nameof(package));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Result = result;
            SslVerify = sslVerify;
            Timeout = timeout;
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }
    }
}
