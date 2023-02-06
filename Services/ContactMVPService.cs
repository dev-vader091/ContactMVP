using ContactMVP.Data;
using ContactMVP.Models;
using ContactMVP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactMVP.Services
{
    public class ContactMVPService : IContactMVPService
    {
        private readonly ApplicationDbContext _context;

        public ContactMVPService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCategoriesAsync(IEnumerable<int> categoryIds, int contactId)
        {
            try
            {
                // Get contact from database
                // Include() is pulling in an ICollection of categories above,
                // which is a collection of Category objects,
                // the hash set is just there so it's never null, just empty.
                Contact? contact = await _context.Contacts
                                                  .Include(c => c.Categories)
                                                  .FirstOrDefaultAsync(c => c.Id == contactId);
                
                foreach (int categoryId in categoryIds) 
                {
                    // Get the category
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (contact != null && category != null)
                    {
                        contact.Categories.Add(category);
                    }
                }

                await _context.SaveChangesAsync();
            
            }
            catch (Exception)
            {

                throw;
            }
        }


        public Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetAppUserCategoriesAsync(string appUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            try
            {
                // Get contact from database
                Contact? contact = await _context.Contacts
                                                 .Include(c => c.Categories)
                                                 .FirstOrDefaultAsync(c => c.Id == contactId);

                bool isInCategory = contact!.Categories.Select(c => c.Id).Contains(categoryId);

                return isInCategory;
                
            }


            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveAllContactCategoriesAsync(int contactId)
        {
            try
            {
                // Get contact from database
                Contact? contact = await _context.Contacts
                                                  .Include(c => c.Categories)
                                                  .FirstOrDefaultAsync(c => c.Id == contactId);
             
                // interaction w/list of categories
                contact!.Categories.Clear();
                // clear the DBset
                _context.Update(contact);
                //update the database
                await _context.SaveChangesAsync();
                                                  
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
