using Newtonsoft.Json.Linq; // Used for JSON object manipulation.
using OW; // Internal library for working with OpenWhisk.
using OWMicroservices; // Internal library for working with microservices.


public static class Program
{
    // The entry point of the application.
    public static void Main(string[] args)
    {
        // Check if command line arguments are provided.
        if (args.Length > 0)
        {
            // Create a generator instance to produce manifest files.
            var generator = new ManifestGenerator("template.yaml", "manifest.yml", "invoke.ps1");

            // Process command based on the first argument, doing case-insensitive comparison.
            switch (args[0].ToLowerInvariant())
            {
                case "generate":
                    // Call the generate method to create manifest file from template.
                    generator.Generate();
                    break;
                case "test":
                    // Execute test commands using the provided arguments. Debug configuration is considered.
                    RunTests(args);
                    break;
                default:
                    // Inform the user about unrecognized commands and suggest available options.
                    Console.WriteLine("Unrecognized argument. Use 'generate' to generate the file or 'test' to execute tests.");
                    break;
            }
        }
        else
        {
            // Notify the user when no arguments are passed to the program.
            Console.WriteLine("No arguments provided. Use 'generate' to generate the file or 'test' to execute tests.");
        }
    }

    // Method to encapsulate test execution logic.
    private static void RunTests(string[] args)
    {
        // Triggers a breakpoint in the attached debugger for manual inspection.
        System.Diagnostics.Debugger.Break();

        // Convert command line arguments into a dictionary for further processing.
        var dictionary = ParseArgumentsToDictionary(args);

        try
        {
            // Attempt to execute the microservice action using parsed arguments as input.
            _ = MyMicroservicesOpenWhiskExecutor.Execute(JObject.FromObject(dictionary));
        }
        catch (Exception ex)
        {
            // Catch any exceptions and output the error message to the console's standard error stream.
            Console.Error.WriteLine($"An error occurred while executing tests: {ex.Message}");
        }
    }

    // Converts an array of command line arguments into a dictionary.
    private static Dictionary<string, string> ParseArgumentsToDictionary(string[] args)
    {
        // Initialize a case-insensitive dictionary to store argument key-value pairs.
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Iterate through each argument and split it into a pair.
        foreach (var arg in args)
        {
            var splitArg = arg.Split(new char[] { '=' }, 2); // Split the string into exactly two substrings.
            if (splitArg.Length == 2)
            {
                // Add the split key and value to the dictionary if we have exactly two elements.
                dictionary[splitArg[0]] = splitArg[1];
            }
        }

        // Return the populated dictionary back to the caller.
        return dictionary;
    }
}
