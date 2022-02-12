using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactBookRepository : IContactBookRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactBookRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }


        public async Task<IContactBook> SaveAsync(IContactBook contactBook)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactBookDao(contactBook);

            if (dao.Id == 0)
                dao.Id = await connection.InsertAsync(dao);
            else
                await connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqliteConnection(databaseConfig.ConnectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var sql = new StringBuilder();
                sql.AppendLine("DELETE FROM ContactBook WHERE Id = @id;");
                sql.AppendLine("UPDATE Contact SET ContactBookId = null WHERE ContactBookId = @id;");
                sql.AppendLine("UPDATE Company SET ContactBookId = null WHERE ContactBookId = @id;");

                try
                {
                    await connection.ExecuteAsync(sql.ToString(), new { id }, transaction);
                    transaction.Commit();
                }
                catch
                {

                    try
                    {
                        transaction.Rollback();
                    }
                    catch { }
                }
            };
        }

        public async Task<IEnumerable<IContactBook>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM ContactBook";
            var result = await connection.QueryAsync<ContactBookDao>(query);

            return result?.Select(i => i.Export());
        }
        public async Task<IContactBook> GetAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM ContactBook WHERE Id = @Id";

            var parms = new DynamicParameters();
            parms.Add("@Id", id);
            return (await connection.QueryAsync<ContactBookDao>(query, parms)).FirstOrDefault();
        }
    }

    [Table("ContactBook")]
    public class ContactBookDao : IContactBook
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public ContactBookDao()
        {
        }

        public ContactBookDao(IContactBook contactBook)
        {
            Id = contactBook.Id;
            Name = contactBook.Name;
        }

        public IContactBook Export() => new ContactBook(Id, Name);
    }
}
