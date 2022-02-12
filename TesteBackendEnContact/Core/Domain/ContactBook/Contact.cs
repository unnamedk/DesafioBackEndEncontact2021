using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Core.Domain.ContactBook
{
    public class Contact : IContact
    {
        public int Id { get; set; }

        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public Contact(int id, int contactBookId, int companyId, string name, string email, string phone, string address)
        {
            Id = id;
            ContactBookId = contactBookId;
            CompanyId = companyId;
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
        }
    }
}
