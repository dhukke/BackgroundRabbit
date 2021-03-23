using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
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

            var connetionString =
                "Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Initial Catalog=messagedb;";

            using (IDbConnection db = new SqlConnection(connetionString))
            {
                var sqlInsert = "INSERT INTO Messages(Id, Content) VALUES (@id, @content)";

                db.Execute(sqlInsert, content);

                var sqlQuery = "select id, content from Messages where id = @id";

                var messageDb = db.Query<Message>(sqlQuery, new
                    {
                        content.Id
                    })
                    .FirstOrDefault();

                _logger.LogInformation("{0}, {1}", messageDb.Id, messageDb.Content);
            }
        }
    }
}
