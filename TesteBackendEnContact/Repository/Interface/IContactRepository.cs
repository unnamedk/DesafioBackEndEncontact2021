using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<IContact> SaveAsync(IContact contact);
        Task<IEnumerable<IContact>> SaveAsync(IFormFile file);
        Task DeleteAsync(int id);
        Task<IEnumerable<IContact>> GetAllAsync();
        Task<IEnumerable<IContact>> GetAsync(int? id, int? contactBookId, int? companyId, string name, string email, string phone,
                                string address, string companyName, int pageNr, int pageSize);
    }
}
