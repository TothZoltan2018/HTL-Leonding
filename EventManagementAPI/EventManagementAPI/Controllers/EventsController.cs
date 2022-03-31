using EventManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventsRepository repository;

        public EventsController(IEventsRepository repository) // Dependency Injection
        {
            this.repository = repository;
        }

        [HttpGet]
        // We tell ASP.Net what is the return type, because it is not seen from return type "IActionResult"
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Event>))]
        public IActionResult GetAll() => Ok(repository.GetAll());
        
        // It gives a technical name with which we can reference this route in the "Add" method
        [HttpGet("{id}", Name = nameof(GetById))] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Event))]        
        public IActionResult GetById(int id)
        {
            var existingEvent = repository.GetById(id);
            if (existingEvent == null) return NotFound();
            return Ok(existingEvent);        
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Event))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]//, Type = typeof(string))] // Type is optional
        // Getting the Event data in JSon format From the Http body
        public IActionResult Add([FromBody] Event newEvent)
        {
            if (newEvent.Id < 1)
            {
                return BadRequest("Invalid id");
            }

            repository.Add(newEvent);
            return CreatedAtAction(nameof(GetById), new { id = newEvent.Id}, newEvent);        
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]        
        // e.g. http://api.myserver.com/api/events/6645
        [Route("{eventToDeleteId}")]// <-----|
        public IActionResult Delete(int eventToDeleteId)
        {
            try
            {
                repository.Delete(eventToDeleteId);                
            }
            catch (ArgumentException)
            {

                return NotFound();
            }

            return NoContent();
        }
    }
}
