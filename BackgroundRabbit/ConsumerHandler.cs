using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BackgroundRabbit
{
    public class ConsumerHandler
    {
        private readonly ILogger _logger;

        public ConsumerHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsumerHandler>();
        }

        public void HandleMessage(Message content)
        {
            _logger.LogInformation("ConsumerHandler [x] received {0}", content);

            //var connetionString = @"Server=localhost,1433;Database=messagedb;User ID=sa;Password=yourStrong(!)Password;";
            //connetionString = @"Data Source=WIN-50GP30FGO75;Initial Catalog=Demodb;User ID=sa;Password=demol23";

            using (var connection =
                new SqlConnection("Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Initial Catalog=messagedb;")
            )
            {
                connection.Open();

                var sql = "INSERT INTO Messages(Id, Content) VALUES (@id, @content)";
                var command = new SqlCommand(sql, connection);

                command.Parameters.Add(new SqlParameter("@id", content.Id));
                command.Parameters.Add(new SqlParameter("@content", content.Text));

                command.ExecuteNonQuery();

                var sqlGet = "select top(1) id, content from Messages where id = @id";

                var commandGet = new SqlCommand(sqlGet, connection);
                commandGet.Parameters.AddWithValue("@id", content.Id);

                var reader = commandGet.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        _logger.LogInformation("{0}, {1}", reader["id"], reader["content"]);
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }

                //fecha a conexao
                connection.Close();
            }
        }
    }
}
