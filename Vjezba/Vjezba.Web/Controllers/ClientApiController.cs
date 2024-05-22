using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Models;

namespace Vjezba.Web.Controllers
{
    [Route("/api/client")]
    [ApiController]
    public class ClientApiController(
        ClientManagerDbContext _dbContext) : Controller
    {

        [HttpGet]
        public IActionResult Get()
        {
            var clients = _dbContext.Clients.Include(c => c.City).ToList();

            var clientDTOs = clients.Select(client => new ClientDTO
            {
                Id = client.ID,
                FullName = client.FullName,
                City = client.City != null ? new CityDTO
                {
                    Id = client.City.ID,
                    Name = client.City.Name
                } : null,
                Email = client.Email
            }).ToList();

            return Ok(clientDTOs);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Client client = _dbContext.Clients.Single(c => c.ID == id);

            return Ok(client);
        }

        [HttpGet("pretraga/{q}")]
        public IActionResult Get(string q) {
            var clients = _dbContext.Clients.Include(p => p.City)
                                            .Where(c => c.FirstName.Contains(q) || c.LastName.Contains(q))
                                            .ToList();

            var clientDTOs = clients.Select(client => new ClientDTO
            {
                Id = client.ID,
                FullName = client.FullName,
                City = client.City != null ? new CityDTO
                {
                    Id = client.City.ID,
                    Name = client.City.Name
                } : null,
                Email = client.Email
            }).ToList();

            return Ok(clientDTOs);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {

            Console.WriteLine("-------------------- " + client.FirstName + " --------------------");

            if (client == null)
            {
                return BadRequest("Client data is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dbContext.Clients.Add(client);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = client.ID }, client);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Client client)
        {
            Console.WriteLine(client.ID);
            if (client == null || client.ID != id)
            {
                return BadRequest();
            }

            var existingClient = _dbContext.Clients.SingleOrDefault(c => c.ID == id);
            if (existingClient == null)
            {
                return NotFound();
            }

            existingClient.FirstName = client.FirstName;
            existingClient.LastName = client.LastName;
            existingClient.Email = client.Email;
            existingClient.DateOfBirth = client.DateOfBirth;
            existingClient.WorkingExperience = client.WorkingExperience;
            existingClient.Gender = client.Gender;
            existingClient.Address = client.Address;
            existingClient.PhoneNumber = client.PhoneNumber;
            existingClient.CityID = client.CityID;
            existingClient.City = client.City;

            _dbContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var client = _dbContext.Clients.Find(id);
            if (client == null)
            {
                return NotFound($"Client with ID {id} not found.");
            }

            _dbContext.Clients.Remove(client);
            _dbContext.SaveChanges();

            var clients = _dbContext.Clients.ToList();
            return Ok(clients);
        }
    }
}
