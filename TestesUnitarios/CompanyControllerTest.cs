using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Repository;
using Xunit;
using ExpectedObjects;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;

namespace UnitTests
{
    public class CompanyControllerTest : IClassFixture<DatabaseFixture>
    {
        private readonly CompanyRepository _companyRepo;
        private readonly ContactBookRepository _contactBookRepo;

        public CompanyControllerTest(DatabaseFixture db)
        {
            _companyRepo = new CompanyRepository(db.Config);
            _contactBookRepo = new ContactBookRepository(db.Config);
        }

        [Fact]
        public async Task SaveCompany()
        {
            var contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaEmpresaTeste"))).Id;
            var ans = await _companyRepo.SaveAsync(new Company(0, contactBookId, "EmpresaTeste"));

            var expectedResult = new
            {
                ContactBookId = contactBookId,
                Name = "EmpresaTeste"
            }.ToExpectedObject();

            expectedResult.ShouldMatch(ans);
        }

        [Fact]
        public async Task DeleteCompany()
        {
            var contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaEmpresaTeste"))).Id;
            var insertedCompanyId = (await _companyRepo.SaveAsync(new Company(0, contactBookId, "EmpresaTeste"))).Id;
            await _companyRepo.DeleteAsync(insertedCompanyId);

            Assert.Null(await _companyRepo.GetAsync(insertedCompanyId));
        }

        [Fact]
        public async Task GetFromId()
        {
            var contactBookId = (await _contactBookRepo.SaveAsync(new ContactBook(0, "AgendaEmpresaTeste"))).Id;
            var insertedCompanyId = (await _companyRepo.SaveAsync(new Company(0, contactBookId, "EmpresaTeste"))).Id;

            var cb = await _companyRepo.GetAsync(insertedCompanyId);

            var expectedResult = new
            {
                ContactBookId = contactBookId,
                Name = "EmpresaTeste"
            }.ToExpectedObject();

            expectedResult.ShouldMatch(cb);
        }
    }
}