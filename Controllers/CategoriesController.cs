﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactMVP.Data;
using ContactMVP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactMVP.Services;
using ContactMVP.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactMVP.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IContactMVPService _contactMVPService;
        private readonly IEmailSender _emailService;


        public CategoriesController(ApplicationDbContext context, UserManager<AppUser> userManager, IContactMVPService contactMVPService, IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _contactMVPService = contactMVPService;
            _emailService = emailService;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            string userId = _userManager.GetUserId(User)!;

            //var model = _context.Categories.Include(c => c.AppUser);

            List<Category> categories = new List<Category>();

            categories = await _context.Categories.Where(c => c.AppUserId == userId).Include(c => c.AppUser).ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> EmailCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string? userId = _userManager.GetUserId(User);
            Category? category = await _context.Categories
                                                .Include(c => c.Contacts)
                                                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            List<string> emails = category!.Contacts.Select(c => c.Email).ToList()!;

            EmailData emailData = new EmailData()
            {
                GroupName = category.Name,
                EmailAddress = string.Join(";", emails),
                EmailSubject = $"Group Message: {category.Name}"
            };

            return View(emailData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCategory(EmailData emailData)
        {
            if (ModelState.IsValid)
            {
                string? swalMessage = string.Empty;

                try
                {
                    await _emailService.SendEmailAsync(emailData!.EmailAddress!,
                                                       emailData.EmailSubject!,
                                                       emailData.EmailBody!);


                    swalMessage = "Success: Email Sent!";
                    return RedirectToAction(nameof(Index), new { swalMessage });
                }
                catch (Exception)
                {
                    swalMessage = "Error: Email Send Failed!";
                    return RedirectToAction(nameof(Index), new { swalMessage });
                    throw;

                }
            }

            return View(emailData);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            // Query and present the list of contacts for logged in user
            string userId = _userManager.GetUserId(User)!;


            IEnumerable<Contact> contactList = await _context.Contacts
                                                         .Where(c => c.AppUserId == userId)
                                                         .ToListAsync();


            ViewData["ContactList"] = new MultiSelectList(contactList, "Id", "FullName");



            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");

            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AppUserId")] Category category)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                category.AppUserId = _userManager.GetUserId(User);

                _context.Add(category);
                await _context.SaveChangesAsync();


                
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", category.AppUserId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AppUserId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", category.AppUserId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
