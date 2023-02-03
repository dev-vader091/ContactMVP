using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactMVP.Data;
using ContactMVP.Models;
using ContactMVP.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactMVP.Services.Interfaces;

namespace ContactMVP.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IContactMVPService _contactMVPService;


        public ContactsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager, 
                                  IImageService imageService,
                                  IContactMVPService contactMVPService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _contactMVPService = contactMVPService;
        }
                                 


        // GET: Contacts

        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User)!;

            List<Contact> contacts = new List<Contact>();

            contacts = await _context.Contacts.Where(c => c.AppUserId == userId)
                                              .Include(c => c.Categories)
                                              .ToListAsync();

            return View(contacts);
        }

        // GET: Contacts/Details/5
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
      
        public async Task<IActionResult> Create()
        {
            // Query and present the list of categories for logged in user
            string? userId = _userManager.GetUserId(User);

            IEnumerable<Category> categoriesList = await _context.Categories
                                                         .Where(c => c.AppUserId == userId)
                                                         .ToListAsync();


            ViewData["CategoryList"] = new MultiSelectList(categoriesList, "Id", "Name");



            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,Phone,Created,ImageFile")] Contact contact, IEnumerable<int> selected)
        {
            // ModelState -- built in class to check for required/non-nullable
            // reflects what is expected from db
            // ModelState will be invalid if check fails

            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.UtcNow;

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;

                }   

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                // TODO: Add service to call
                await _contactMVPService.AddContactToCategoriesAsync(selected, contact.Id);

                return RedirectToAction(nameof(Index));
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
            return View(contact);
        }

        // GET: Contacts/Edit/5
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                                        .Include(c => c.Categories)
                                        .FirstOrDefaultAsync(c => c.Id == id);

            // Query and present the list of categories for logged in user
            string? userId = _userManager.GetUserId(User);

            IEnumerable<Category> categoriesList = await _context.Categories
                                                         .Where(c => c.AppUserId == userId)
                                                         .ToListAsync();

            IEnumerable<int> currentCategories = contact!.Categories.Select(c => c.Id);


            if (contact == null)
            {
                return NotFound();
            }


            // MultiSelectList -- "Id" is what's chosen(category in this case), "Name" is what is sent/shown to client(view)
            ViewData["CategoryList"] = new MultiSelectList(categoriesList, "Id", "Name", currentCategories);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
            
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,Phone,Created,ImageData,ImageType,ImageFile")] Contact contact, IEnumerable<int> selected)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {      
                try
                {

                    // Reformat Created Date
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);  

                    // Check if image was updated
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    // Reformat Birth Date
                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }
              
                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //TODO: 
                    // Add use of the ContactMVPService ???
                    //DONE!

                    if (selected != null)
                    {
                        // 1. Remove Contact's categories
                        await _contactMVPService.RemoveAllContactCategoriesAsync(contact.Id);

                        // 2. Add selected categories to contact
                        await _contactMVPService.AddContactToCategoriesAsync(selected, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                     
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
            return View(contact);
        }

        // GET: Contacts/Delete/5
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
          return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
