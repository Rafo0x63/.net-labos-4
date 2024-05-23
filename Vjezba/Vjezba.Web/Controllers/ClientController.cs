using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Vjezba.Web.Controllers
{
    public class ClientController : BaseController
    {
        private readonly ClientManagerDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public ClientController(ClientManagerDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_dbContext.Clients.ToList());
        }

        [Authorize]
        public IActionResult Details(int? id = null)
        {
            var client = _dbContext.Clients
                .Include(p => p.City)
                .Where(p => p.ID == id)
                .FirstOrDefault();

            return View(client);
        }

        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            this.FillDropdownValues();
            return View();
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public IActionResult Create(Client model)
        {
            Console.WriteLine("USER ID --------- " + UserId);
            if (ModelState.IsValid)
            {
                model.CreatedById = UserId;

                _dbContext.Clients.Add(model);
                _dbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    // Možete koristiti error.ErrorMessage za prikazivanje poruke
                    Console.WriteLine(error.ErrorMessage);
                }
                this.FillDropdownValues();
                return View();
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [ActionName(nameof(Edit))]
        public IActionResult Edit(int id)
        {
            var model = _dbContext.Clients.FirstOrDefault(c => c.ID == id);
            this.FillDropdownValues();
            return View(model);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(int id)
        {
            var client = _dbContext.Clients.Single(c => c.ID == id);
            var ok = await this.TryUpdateModelAsync(client);

            if (ok && this.ModelState.IsValid)
            {
                client.UpdatedById = UserId;

                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            this.FillDropdownValues();
            return View();
        }

        private void FillDropdownValues()
        {
            var selectItems = new List<SelectListItem>();

            // Polje je opcionalno
            var listItem = new SelectListItem
            {
                Text = "- odaberite -",
                Value = ""
            };
            selectItems.Add(listItem);

            foreach (var category in _dbContext.Cities)
            {
                listItem = new SelectListItem(category.Name, category.ID.ToString());
                selectItems.Add(listItem);
            }

            ViewBag.PossibleCities = selectItems;
        }

        [HttpPost]
        public IActionResult IndexAjax(ClientFilterModel filter)
        {
            var filteredClients = _dbContext.Clients
                .Include(c => c.City).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FullName))
                filteredClients = filteredClients.Where(p => (p.FirstName + " " + p.LastName).ToLower().Contains(filter.FullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                filteredClients = filteredClients.Where(p => p.Address.ToLower().Contains(filter.Address.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                filteredClients = filteredClients.Where(p => p.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.City))
                filteredClients = filteredClients.Where(p => p.CityID != null && p.City.Name.ToLower().Contains(filter.City.ToLower()));

            Console.WriteLine(filter.FullName);

            return PartialView("_IndexTable", filteredClients);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(int clientId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                ClientId = clientId,
                FilePath = filePath
            };

            _dbContext.Attachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { filePath });
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments(int id)
        {
            var attachments = await _dbContext.Attachments
                .Where(a => a.ClientId == id).ToListAsync();

            return PartialView("_AttachmentList", attachments);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            var attachment = await _dbContext.Attachments.SingleAsync(a => a.Id == id);
            if (attachment == null) return NotFound("File doesn't exist");

            var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _dbContext.Attachments.Remove(attachment);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
