using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Repository;
using Xunit;
using ExpectedObjects;

namespace UnitTests
{
    public class ContactBookControllerTest : IClassFixture<DatabaseFixture>
    {
        private readonly ContactBookRepository _contactBookRepo;

        public ContactBookControllerTest(DatabaseFixture db)
        {
            _contactBookRepo = new ContactBookRepository(db.Config);
        }

        [Fact]
        public async Task SaveContactBook()
        {
            var ans = await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaA"));

            var expectedResult = new
            {
                Name = "AgendaA"
            }.ToExpectedObject();

            expectedResult.ShouldMatch(ans);
        }

        [Fact]
        public async Task DeleteContactbook_FromDB()
        {
            var insertedContactBook = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaA"))).Id;

            await _contactBookRepo.DeleteAsync(insertedContactBook);
            Assert.Null(await _contactBookRepo.GetAsync(insertedContactBook));
        }

        [Fact]
        public async Task GetFromId()
        {
            var insertedContactBook = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaB"))).Id;

            var cb = await _contactBookRepo.GetAsync(insertedContactBook);
            var expectedResult = new
            {
                Id = insertedContactBook,
                Name = "AgendaB"
            }.ToExpectedObject();

            expectedResult.ShouldMatch(cb);
        }
    }
}
