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
                //Adicionando o valor das textBox nos parametros do comando
                command.Parameters.Add(new SqlParameter("@id", content.Id));
                command.Parameters.Add(new SqlParameter("@content", content.Text));

                command.ExecuteNonQuery();

                //fecha a conexao
                connection.Close();
            }
        }
    }
}
