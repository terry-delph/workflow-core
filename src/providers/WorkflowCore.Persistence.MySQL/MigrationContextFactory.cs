using Microsoft.EntityFrameworkCore.Design;

namespace WorkflowCore.Persistence.MySQL
{
    public class MigrationContextFactory : IDesignTimeDbContextFactory<MysqlContext>
    {
        public MysqlContext CreateDbContext(string[] args)
        {
            return new MysqlContext(@"Server=127.0.0.1;Port=5432;Database=workflow;User Id=postgres;Password=password;");
        }
    }
}
