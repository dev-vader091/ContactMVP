using ContactMVP.Models;

namespace ContactMVP.Services.Interfaces
{
    public interface IContactMVPService
    {
        public Task AddContactToCategoryAsync(int categoryId, int contactId);
        public Task AddContactToCategoriesAsync(IEnumerable<int> category, int contactId);

        public Task<IEnumerable<Category>> GetAppUserCategoriesAsync(string appUserId);
    
        public Task<bool> IsContactInCategory(int categoryId, int contactId);
    
        public Task RemoveAllContactCategoriesAsync(int contactId);
    }
}

        
        // Abstraction - utilize functionality and/or data of defined class to do something else
        // ex. taking subset of class/functionality to use in your application
    