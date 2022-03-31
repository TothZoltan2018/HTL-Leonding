using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementAPI.Services
{
    public record Event(int Id, DateTime Date, string Location, string Description);
    public class EventsRepository : IEventsRepository
    {
        // In real life use EFCore
        private List<Event> Events { get; } = new();
        public Event Add(Event newEvent)
        {
            Events.Add(newEvent);
            return newEvent;
        }

        public IEnumerable<Event> GetAll() => Events;

        public Event GetById(int id) => Events.FirstOrDefault(e => e.Id == id);

        public void Delete(int id)
        {
            Event eventToDelete = GetById(id);
            if (eventToDelete == null)
            {
                throw new ArgumentException("No event exists with the given id", nameof(id));
            }

            Events.Remove(eventToDelete);
        }
    }
}
