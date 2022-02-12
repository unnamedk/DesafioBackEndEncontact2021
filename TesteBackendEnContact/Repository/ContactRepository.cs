using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqliteConnection(databaseConfig.ConnectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var sql = new StringBuilder();
                sql.AppendLine("DELETE FROM Contact WHERE Id = @id;");

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

        public async Task<IEnumerable<IContact>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact";
            var result = await connection.QueryAsync<ContactDao>(query);

            return result?.Select(i => i.Export());
        }

        public async Task<IContact> GetAsync(int id)
        {
            var list = await GetAllAsync();

            return list.ToList().Where(item => item.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Pesquisar contatos em qualquer campo (paginado)
        /// </summary>
        public async Task<IEnumerable<IContact>> GetAsync(int? id, int? contactBookId, int? companyId, string name, string email, string phone, string address, string companyName, int pageNr, int pageSz)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var opts = new Dictionary<string, object>();
            if (id is not null)
            {
                opts.Add("@Id", id.Value);
            }

            if (contactBookId is not null)
            {
                opts.Add("@ContactBookId", contactBookId.Value);
            }

            if (companyId is not null)
            {
                opts.Add("@CompanyId", companyId.Value);
            }

            if (name is not null)
            {
                opts.Add("@Name", name);
            }

            if (email is not null)
            {
                opts.Add("@Email", email);
            }

            if (phone is not null)
            {
                opts.Add("@Phone", phone);
            }

            if (address is not null)
            {
                opts.Add("@Address", address);
            }

            var query = new StringBuilder();
            query.AppendLine(@"SELECT Contact.Id AS ContactId, 
                                      Contact.ContactBookId,
                                      Contact.CompanyId,
                                      Contact.Name,
                                      Contact.Email,
                                      Contact.Phone,
                                      Contact.Address
                                      FROM Contact");

            if (companyName is not null)
            {
                opts.Add("@CompanyName", companyName);
                query.AppendLine("INNER JOIN Company ON Contact.CompanyId = Company.Id");
            }

            if (opts.Count > 0)
            {
                query.Append("WHERE ");
                foreach (var (varName, _) in opts)
                {
                    query.AppendLine($"{ (varName == "@CompanyName" ? "Company.Name" : $"Contact.{ varName[1..] }") } LIKE {varName} AND");
                }

                query.Length -= 4 + Environment.NewLine.Length;
            }

            // paginação
            int offset = (pageNr - 1) * pageSz;
            opts.Add("@Offset", offset);
            opts.Add("@Rows", pageSz);
            query.AppendLine(" LIMIT @Rows OFFSET @Offset");

            string finalQuery = query.ToString();
            var result = await connection.QueryAsync<ContactDao>(finalQuery, new DynamicParameters(opts));
            return result?.ToList();
        }

        public async Task<IContact> SaveAsync(IContact contact)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactDao(contact);

            if (dao.Id == 0)
                dao.Id = await connection.InsertAsync(dao);
            else
                await connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task<IEnumerable<IContact>> SaveAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var contacts = await SaveAllFromCsv(reader);
            return contacts;
        }

        public async Task<IEnumerable<IContact>> SaveAllFromCsv(StreamReader reader)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var contacts = new List<IContact>();

            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                // skip csv header
                line = await reader.ReadLineAsync();
            }

            var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            while (line != null)
            {
                var members = regex.Split(line);
                if (members.Length >= 5)
                {
                    if (!int.TryParse(members[0].Trim(), out int contactBookId))
                    {
                        Console.WriteLine("Error: Could not parse contact book id from CSV");
                        line = await reader.ReadLineAsync();
                        continue;
                    }
                    var name = members[1].Trim();
                    var email = members[2].Trim();
                    var phone = members[3].Trim();
                    var address = members[4].Trim();

                    // É obrigatório ter uma agenda vinculada ao contato.
                    {
                        var parms = new DynamicParameters();
                        parms.Add("@Id", contactBookId);

                        var query = "SELECT * FROM ContactBook WHERE Id = @Id";
                        if (!(await connection.QueryAsync<ContactBookDao>(query, parms)).Any())
                        {
                            continue;
                        }
                    }

                    // No arquivo, se for informada uma empresa ao contato, ela deve existir previamente no sistema. Caso não seja informado, o contato é registrado sem vinculo com a empresa.
                    var companyId = 0;
                    if (members.Length == 6)
                    {
                        var companyIdRaw = members[5].Trim();

                        var parms = new DynamicParameters();
                        parms.Add("@Id", companyIdRaw);
                        var query = "SELECT * FROM Company WHERE Id = @Id";
                        if (!(await connection.QueryAsync<ContactBookDao>(query, parms)).Any())
                        {
                            line = await reader.ReadLineAsync();
                            continue;
                        }
                        companyId = int.Parse(companyIdRaw);
                    }

                    try
                    {
                        var ans = await SaveAsync(new Contact(0, contactBookId, companyId, name, email, phone, address));
                        if (ans != null)
                        {
                            contacts.Add(ans);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: Exception caught while adding CSV. { ex.Message }");
                    }
                }
                else
                {
                    // Caso dê erro na importação de um registro, não deve impactar a importação dos demais.
                    Console.WriteLine("Error: Invalid CSV row: {0}", line);
                }

                line = await reader.ReadLineAsync();
            }

            return contacts;
        }
    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public ContactDao()
        {
        }

        public ContactDao(IContact contact)
        {
            this.Id = contact.Id;
            this.ContactBookId = contact.ContactBookId;
            this.CompanyId = contact.CompanyId;
            this.Name = contact.Name;
            this.Email = contact.Email;
            this.Phone = contact.Phone;
            this.Address = contact.Address;
        }

        public IContact Export() => new Contact(Id, ContactBookId, CompanyId, Name, Email, Phone, Address);
    }
}
