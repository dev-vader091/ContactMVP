using ContactMVP.Models;
using ContactMVP.Services.Interfaces;

namespace ContactMVP.Services
{
    public class ContactMVPService : IContactMVPService
    {
        public Task AddContactToCategoriesAsync(IEnumerable<int> category, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetAppUserCategoriesAsync(string appUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllContactCategoriesAsync(int contactId)
        {
            throw new NotImplementedException();
        }
    }
}
