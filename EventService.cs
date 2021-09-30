using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EventsSample
{
    public class EventService
    {
        private readonly List<Event> _events;
        private readonly ILogger<EventService> _log;
        private readonly NotificationService _notify;

        public EventService(ILogger<EventService> log, NotificationService notify)
        {
            _log = log;
            _notify = notify;

            _events = new List<Event>();
            _events.Add(new Event() {
                Id = "0",
                Date = "3/16/2021",
                Type = "Manufacturing",
                Product = "Blue Shoes",
                Description = "Laces attached"
            });
            _events.Add(new Event() {
                Id = "1",
                Date = "3/18/2021",
                Type = "Testing",
                Product = "Blue Shoes",
                Description = "Ran up, down hallway"
            });
        }

        public IEnumerable<Event> Get() =>
            _events;

        public Event Get(string id) =>
            _events.Find(e => e.Id == id);

        public async Task<Event> CreateAsync(Event eventItem)
        {
            // _events.InsertOne(Event);
            _events.Add(eventItem);
            _log.LogInformation($"Created event id:{eventItem.Id}");
            await _notify.PublishAsync(eventItem);

            return eventItem;
        }
    }
}