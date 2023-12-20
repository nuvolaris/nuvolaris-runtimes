using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OW
{
    // DTO class to represent functionalities.
    public class FncDto
    {
        // Default property initializers to assign default values.
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Descr { get; set; }
        public bool? Enabled { get; set; } = true;
        public DateTime? UpdDate { get; set; } = DateTime.Now;

        // Override ToString for a readable representation of the object.
        public override string ToString()
        {
            return $"Id: {Id}, Descr: {Descr}, Enabled: {Enabled}, UpdDate: {UpdDate}";
        }
    }

    // Repository class to manage database operations.
    public class FncDtoRepository
    {
        private string? actionName;
        public string? tableName { get; set; }
        private readonly string _connectionString;

        private IConfiguration config;

        // Costruttore del repository
        public FncDtoRepository(string argsJson)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            dynamic? args = argsJson != null ? JsonConvert.DeserializeObject(argsJson) : null;
            string server = config["DB_SERVER"] ?? "nuvolaris-postgres";
            string database = config["DB_NAME"] ?? "nuvolaris";
            string port = config["DB_PORT"] ?? "5432";
            string user = config["DB_USER"] ?? args?.DB_USER ?? string.Empty;
            string password = config["DB_PASSWORD"] ?? args?.DB_PASSWORD ?? string.Empty;
            actionName = config["ACTION_NAME"] ?? args?.ACTION_NAME ?? string.Empty;
            tableName = actionName + "_Dto";

            _connectionString = $"SERVER={server};PORT={port};DATABASE={database};UID={user};PWD={password};";
        }

        // Opens a new connection and ensures it's ready for use.
        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        // Truncate a table if it doesn't exist in the database.
        public async Task TruncateTableAsync()
        {
            var createTableQuery = $@"
                TRUNCATE TABLE {actionName}_Dto;";

            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(createTableQuery);
            }
        }

        // Drop a table if it doesn't exist in the database.
        public async Task DropTableAsync()
        {
            var createTableQuery = $@"
                DROP TABLE {actionName}_Dto;";

            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(createTableQuery);
            }
        }

                // Creates a table if it doesn't exist in the database.
        public async Task CreateTableAsync()
        {
            var createTableQuery = $@"
                CREATE TABLE IF NOT EXISTS {actionName}_Dto (
                    Id UUID PRIMARY KEY,
                    Descr TEXT,
                    Enabled BOOLEAN,
                    UpdDate TIMESTAMP
                );";

            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(createTableQuery);
            }
        }

        // Executes a given query against the database.
        public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string query)
        {
            using (var connection = await OpenConnectionAsync())
            {
                return await connection.QueryAsync(query);
            }
        }

        // Retrieves all records from the specified table.
        public async Task<IEnumerable<FncDto>> GetAllAsync()
        {
            using (var connection = await OpenConnectionAsync())
            {
                return await connection.QueryAsync<FncDto>($"SELECT * FROM {tableName}");
            }
        }

        // Fetches a single record by its ID.
        public async Task<FncDto> GetByIdAsync(Guid id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var result = await connection.QueryFirstOrDefaultAsync<FncDto>(
                    $"SELECT * FROM {tableName} WHERE Id = @Id", new { Id = id });

                return result!;
            }
        }

        // Inserts a new record into the specified table.
        public async Task<string> InsertAsync(FncDto dto)
        {
            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(
                    $"INSERT INTO {tableName} (Id, Descr, Enabled, UpdDate) VALUES (@Id, @Descr, @Enabled, @UpdDate)", dto);
                return dto.Id.ToString();
            }
        }

        // Updates an existing record in the specified table.
        public async Task<string> UpdateAsync(FncDto dto)
        {
            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(
                    $"UPDATE {tableName} SET Descr = @Descr, Enabled = @Enabled, UpdDate = @UpdDate WHERE Id = @Id", dto);
                return dto.Id.ToString();
            }
        }

        // Deletes a record by its ID from the specified table.
        public async Task<string> DeleteAsync(Guid id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                await connection.ExecuteAsync(
                    $"DELETE FROM {tableName} WHERE Id = @Id", new { Id = id });
                return id.ToString();
            }
        }
    }
}
