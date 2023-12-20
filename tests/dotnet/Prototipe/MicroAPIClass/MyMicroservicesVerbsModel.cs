using Newtonsoft.Json.Linq;
using OW;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OWMicroservices
{
    /// <summary>
    /// Class for handling various HTTP operations.
    /// </summary>
    public static class MyMicroservicesVerbsModel
    {
        /// <summary>
        /// Handles the creation of a table.
        /// </summary>
        public static async Task<JObject> HandleCreateTable(FncDtoRepository repository)
        {
            await repository.CreateTableAsync();
            return JObject.FromObject(new { message = "Tabella creata: " + repository.tableName });
        }

        /// <summary>
        /// Handles the truncate of a table.
        /// </summary>
        public static async Task<JObject> HandleTruncateTable(FncDtoRepository repository)
        {
            await repository.TruncateTableAsync();
            return JObject.FromObject(new { message = "Tabella troncata: " + repository.tableName });
        }

        /// <summary>
        /// Handles the drop of a table.
        /// </summary>
        public static async Task<JObject> HandleDropTable(FncDtoRepository repository)
        {
            await repository.DropTableAsync();
            return JObject.FromObject(new { message = "Tabella droppata: " + repository.tableName });
        }

        /// <summary>
        /// Executes a specified query.
        /// </summary>
        public static async Task<JObject> HandleExecuteQuery(JObject args, FncDtoRepository repository)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    var query = args["query"]?.ToString();
                    if (query != null)
                    {
                        // Check if the query is a SELECT statement
                        if (!IsSelectQuery(query))
                        {
                            throw new Exception("Only SELECT queries are allowed");
                        }

                        var queryResult = await repository.ExecuteQueryAsync(query);
                        if (queryResult != null)
                        {
                            return JToken.FromObject(queryResult);
                        }
                        else
                        {
                            return JObject.FromObject(new { message = "La query non ha generato risultati" });
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("query", "Query is null");
                    }
                },
                "Esecuzione della query completata",
                "Errore durante l'esecuzione della query: "
            );
        }

        private static bool IsSelectQuery(string query)
        {
            // Implement your logic to check if the query is a SELECT statement
            // You can use regular expressions or parsing libraries to perform this check
            // Here's a simple example using regular expressions:
            return Regex.IsMatch(query, @"^\s*SELECT", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Handles the GET operation.
        /// </summary>
        public static async Task<JObject> HandleGet(Guid? id, FncDtoRepository repository)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    if (id != null)
                    {
                        #pragma warning disable CS8602 // Dereference of a possibly null reference.
                            var result = await repository.GetByIdAsync((Guid)id);
                        #pragma warning restore CS8602 // Dereference of a possibly null reference.
                        return result != null ? JObject.FromObject(result) : throw new Exception("Id not found");
                    }
                    else
                    {
                        throw new ArgumentNullException("id", "Id is null");
                    }
                },
                "Operazione GET completata",
                "Errore durante l'operazione GET: "
            );
        }

        /// <summary>
        /// Handles the PUT operation.
        /// </summary>
        public static async Task<JObject> HandlePut(FncDto dto, FncDtoRepository repository)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await repository.InsertAsync(dto);
                    return JObject.FromObject(new { message = "Inserimento completato id: " + dto.Id.ToString() });
                },
                "Inserimento completato",
                "Errore durante l'inserimento: ",
                dto
            );
        }

        /// <summary>
        /// Handles the DELETE operation.
        /// </summary>
        public static async Task<JObject> HandleDelete(Guid id, FncDtoRepository repository)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await repository.DeleteAsync(id);
                    return JObject.FromObject(new { message = "Record eliminato id: " + id.ToString() });
                },
                "Eliminazione completata",
                "Errore durante l'eliminazione: "
            );
        }

        /// <summary>
        /// Handles the PATCH operation.
        /// </summary>
        public static async Task<JObject> HandlePatch(FncDto dto, FncDtoRepository repository)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    var existingRecord = await repository.GetByIdAsync(dto.Id);
                    if (existingRecord != null)
                    {
                        await repository.UpdateAsync(dto);
                    }
                    else
                    {
                        await repository.InsertAsync(dto);
                    }
                    return JObject.FromObject(new { message = "Upsert completata id: " + dto.Id.ToString() });
                },
                "Upsert completato id: " + dto.Id.ToString() + "",
                "Errore durante l'esecuzione dell'upsert: ",
                dto
            );
        }

        /// <summary>
        /// Attempts to execute an asynchronous action, returning a JSON result or error message.
        /// </summary>
        private static async Task<JObject> TryExecuteAsync(Func<Task<JToken>> action, string successMessage, string errorMessage, object? argument = null)
        {
            try
            {
                // Ensures that DTO is valid before executing the action.
                if (argument is null || argument as FncDto != null)
                {
                    var result = await action();
                    // If a result is returned from the action, use it; otherwise, return the success message.
                    return result != null ? new JObject { ["result"] = result } : new JObject { ["result"] = successMessage };
                }

                // Returns an error if the DTO is null.
                return new JObject { ["error"] = "Il DTO è nullo" };
            }
            catch (Exception ex)
            {
                // Captures and returns any exception messages encountered during execution.
                return new JObject { ["error"] = $"{errorMessage}{ex.Message}" };
            }
        }
    }
}
