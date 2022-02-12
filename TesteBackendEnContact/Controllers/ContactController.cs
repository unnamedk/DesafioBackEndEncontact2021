using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IContact> Post(Contact contact, [FromServices] IContactRepository contactRepo)
        {
            return await contactRepo.SaveAsync(contact);
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] IContactRepository contactRepo)
        {
            await contactRepo.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<IEnumerable<IContact>> Get([FromServices] IContactRepository contactRepo,
                                                     int? id,
                                                     int? contactBookId,
                                                     int? companyId,
                                                     string name,
                                                     string email,
                                                     string phone,
                                                     string address,
                                                     string companyName,
                                                     int pageNr = 1,
                                                     int pageSize = 10)
        {
            return await contactRepo.GetAsync(id, contactBookId, companyId, name, email, phone, address, companyName, pageNr, pageSize);
        }

        [HttpGet("{companyId}")]
        public async Task<IEnumerable<IContact>> Get([FromServices] IContactRepository contactRepo,
                                                                int companyId,
                                                                int contactBookId,
                                                                int pageNr = 1,
                                                                int pageSize = 10)
        {
            return await contactRepo.GetAsync(null, contactBookId, companyId, null, null, null, null, null, pageNr, pageSize);
        }

        [HttpPost("SaveCSV")]
        public async Task<IEnumerable<IContact>> Post(IFormFile file, [FromServices] IContactRepository contactRepo)
        {
            return await contactRepo.SaveAsync(file);
        }
    }
}
