using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using OWMicroservices;

namespace OW
{
    public class ManifestGenerator
    {
        private readonly string _templatePath;
        private readonly string _outputPath;

        // Constructor to initialize the paths needed for the generator.
        public ManifestGenerator(string templatePath, string outputPath)
        {
            _templatePath = templatePath;
            _outputPath = outputPath;
        }

        // Main method that generates the manifest based on the assembly information and template file.
        public void Generate()
        {
            Assembly entryAssembly = Assembly.GetExecutingAssembly();
            Type type = typeof(MyMicroservicesOpenWhiskExecutor);

            // Retrieve all custom attributes associated with MyMicroservicesOpenWhiskExecutor class at once.
            var attributes = typeof(MyMicroservicesOpenWhiskExecutor).GetCustomAttributes();

            // Fetch necessary custom attribute values from the assembly and provided type.
            // Extracting specific attribute values for action name, package name, and route name
            string actionName = OpenWhiskUtilities.GetAttributeValue("Action");
            string packageName = OpenWhiskUtilities.GetAttributeValue("Package");

            string runtimeVersion = entryAssembly?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName?.Split('=')[1] ?? string.Empty; 
            string dllName = entryAssembly?.GetName()?.Name ?? string.Empty;
            var publicClassType = entryAssembly?.GetTypes().FirstOrDefault(t => t.GetMethods().Any(method => method.Name == "Execute"));

            string template = File.ReadAllText(_templatePath);

            // Replace placeholders in the template if present.
            if (publicClassType != null)
            {
                ProcessTemplatePlaceholders(ref template, publicClassType, runtimeVersion, dllName, actionName, packageName);
            }
            // Write the modified content back to the output file.
            File.WriteAllText(_outputPath, template);

            // Generate the invoke.sh script with the appropriate package and action names.
            string invokeContent = $"nuv action invoke {packageName}/{actionName} --param setupapisix true -r ";
            File.WriteAllText("invoke.sh", invokeContent);
        }

        // Processes the template placeholders and replaces them with actual values.
        private void ProcessTemplatePlaceholders(ref string template, Type publicClassType, string runtimeVersion, 
                                            string dllName, string actionName, string packageName)
        {
            // Only retrieve process data if placeholders exist in the template.
            if (template.Contains("##db"))
            {
                string[] processOutput = RetrieveProcessOutputLines("/c nuv debug status | grep postgres");
                var dbInfo = ParseDatabaseInfo(processOutput);
                template = ReplaceDatabasePlaceholders(template, dbInfo);
            }

            if (template.Contains("##auth##"))
            {
                // Fetch authentication details from external command output.
                string auth = RetrieveProcessOutputLine("/c nuv -wsk -i property get --auth").Split('\t')[2];
                template = template.Replace("##auth##", auth.Trim());
            }

            // Replace placeholders related to the assembly and custom attributes.
            template = template.Replace("##runtimeversion##", runtimeVersion)
                               .Replace("##name##", actionName)
                               .Replace("##dllzip##", $"{dllName}.zip")
                               .Replace("##dllname##", dllName)
                               .Replace("##projectname##", packageName)
                               .Replace("##Programcsnamespace##", publicClassType?.Namespace ?? string.Empty)
                               .Replace("##publicclassname##", publicClassType?.Name ?? string.Empty);
        }

        // Replaces database related placeholders in the template string.
        private string ReplaceDatabasePlaceholders(string template, (string Password, string User, string Host, string Port, string Name) dbInfo)
        {
            return template.Replace("##dbpassword##", dbInfo.Password)
                           .Replace("##dbuser##", dbInfo.User)
                           .Replace("##dbhost##", dbInfo.Host.Split('.')[0])
                           .Replace("##dbport##", dbInfo.Port.Replace("\"", string.Empty).Trim())
                           .Replace("##dbname##", dbInfo.Name);
        }

        // Runs a command line process and returns the standard output as an array of lines.
        private string[] RetrieveProcessOutputLines(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return output.Split('\n');
        }

        // Gets a single line from the standard output of a command line process.
        private string RetrieveProcessOutputLine(string arguments)
        {
            return RetrieveProcessOutputLines(arguments).First();
        }

        // Parses out relevant database information from an array of strings containing command output.
        private (string Password, string User, string Host, string Port, string Name) ParseDatabaseInfo(string[] lines)
        {
            string FindValue(string key) => lines.FirstOrDefault(line => line.Contains(key))?.Split(':')[1].Trim() ?? string.Empty;
            return (
                Password: FindValue("postgres_password"),
                User: FindValue("postgres_username"),
                Host: FindValue("postgres_host"),
                Port: FindValue("postgres_port"),
                Name: FindValue("postgres_database")
            );
        }
    }
}
