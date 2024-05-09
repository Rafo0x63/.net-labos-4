using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Models;

namespace Vjezba.Web.Controllers
{
	public class ClientController(
        ClientManagerDbContext _dbContext) : Controller
    {
        public IActionResult Index(ClientFilterModel filter = null)
        {
			filter ??= new ClientFilterModel();

			var clientQuery = _dbContext.Clients.AsQueryable();

			//Primjer iterativnog građenja upita - dodaje se "where clause" samo u slučaju da je parametar doista proslijeđen.
			//To rezultira optimalnijim stablom izraza koje se kvalitetnije potencijalno prevodi u SQL
			if (!string.IsNullOrWhiteSpace(filter.FullName))
				clientQuery = clientQuery.Where(p => (p.FirstName + " " + p.LastName).ToLower().Contains(filter.FullName.ToLower()));

			if (!string.IsNullOrWhiteSpace(filter.Address))
                clientQuery = clientQuery.Where(p => p.Address.ToLower().Contains(filter.Address.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                clientQuery = clientQuery.Where(p => p.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.City))
				clientQuery = clientQuery.Where(p => p.CityID != null && p.City.Name.ToLower().Contains(filter.City.ToLower()));

            var model = clientQuery.Include(p => p.City).OrderBy(p => p.ID).ToList();

            return View(model);
        }

        public IActionResult Details(int? id = null)
        {
			var client = _dbContext.Clients
				.Include(p => p.City)
				.Where(p => p.ID == id)
				.FirstOrDefault();

			return View(client);
		}

		public IActionResult Create()
		{
			
			ViewBag.Cities = FillCitiesDropDown();

			return View();
		}

		[HttpPost]
		public IActionResult Create(Client model)
		{
            ViewBag.Cities = FillCitiesDropDown();
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.ValidationState == ModelValidationState.Invalid)
                {
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Property: {key}, Error: {error.ErrorMessage}");
                    }
                }
            }
            if (ModelState.IsValid)
			{
				_dbContext.Clients.Add(model);
				_dbContext.SaveChanges();

				return RedirectToAction(nameof(Index));
			}

			return View(model);
		}

		[ActionName("Edit")]
		public ActionResult EditGet(int id)
		{
			var client = _dbContext.Clients.Find(id);

			if (client == null)
			{
				return NotFound();
			}

			var cities = _dbContext.Cities
				.Select(c => new SelectListItem
				{
					Value = c.ID.ToString(),
					Text = c.Name
				})
				.ToList();

			ViewBag.Cities = FillCitiesDropDown();


			return View(client);
		}

		[HttpPost]
        [ActionName("Edit")]
        public async Task<ActionResult> EditPost(int id)
		{
			var client = _dbContext.Clients.FirstOrDefault(p => p.ID == id);
			var ok = await this.TryUpdateModelAsync(client);

			if (ok)
			{
				_dbContext.SaveChanges();
				return RedirectToAction(nameof(Index));
			}

			return View(client);

		}

		public List<SelectListItem> FillCitiesDropDown()
		{
			var selectItems = new List<SelectListItem>();

			var listItem = new SelectListItem();
			listItem.Text = "- odaberite -";
			listItem.Value = "";
			selectItems.Add(listItem);

			foreach (var city in _dbContext.Cities)
			{
				listItem = new SelectListItem();
				listItem.Text = city.Name;
				listItem.Value = city.ID.ToString();
				selectItems.Add(listItem);
			}

			return selectItems;
		}
	}
}