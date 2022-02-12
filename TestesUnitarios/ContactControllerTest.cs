using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository;
using Xunit;
using ExpectedObjects;
using System.IO;
using System.Linq;

namespace UnitTests
{
    public class ContactControllerTest : IClassFixture<DatabaseFixture>
    {
        private readonly ContactRepository _contactRepo;
        private readonly ContactBookRepository _contactBookRepo;

        public ContactControllerTest(DatabaseFixture db)
        {
            _contactRepo = new ContactRepository(db.Config);
            _contactBookRepo = new ContactBookRepository(db.Config);
        }

        [Fact]
        public async Task SaveContact()
        {
            int contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaA"))).Id;

            var ans = await _contactRepo.SaveAsync(new Contact(0, contactBookId, 0, "Marcos", "Marcos@hotmail.com", "943157891", "Rua Castelo Branco"));
            var expectedResult = new
            {
                ContactBookId = contactBookId,
                Name = "Marcos",
                Email = "Marcos@hotmail.com",
                Phone = "943157891",
                Address = "Rua Castelo Branco"
            }.ToExpectedObject();

            expectedResult.ShouldMatch(ans);
        }

        [Fact]
        public async Task SaveByCSV()
        {
            int contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaA"))).Id;

            var csv = $@"ContactBookId,Name,Email,Phone,Address,CompanyId
{contactBookId},Joao,joao@gmail.com,9941383195,""Av.Praia Grande, 131""";

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            var ans = Assert.Single(await _contactRepo.SaveAllFromCsv(reader));

            var expectedResult = new
            {
                ContactBookId = contactBookId,
                Name = "Joao",
                Email = "joao@gmail.com",
                Phone = "9941383195",
                Address = "\"Av.Praia Grande, 131\""
            }.ToExpectedObject();

            expectedResult.ShouldMatch(ans);
        }

        [Fact]
        public async Task GetContact_FromDB()
        {
            int contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "TestContactBookB"))).Id;

            var inserted = await _contactRepo.SaveAsync(new Contact(0, contactBookId, 0, "Marcos", "Marcos@hotmail.com", "943157891", "Rua Castelo Branco"));

            var ans = (await _contactRepo.GetAsync(inserted.Id)).ToExpectedObject();
            ans.ShouldEqual(inserted);
        }
    }
}