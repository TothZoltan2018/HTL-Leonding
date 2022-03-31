using System.Collections.Generic;

namespace EventManagementAPI.Services
{
    public interface IEventsRepository
    {
        Event Add(Event newEvent);
        void Delete(int id);
        IEnumerable<Event> GetAll();
        Event GetById(int id);
    }
}