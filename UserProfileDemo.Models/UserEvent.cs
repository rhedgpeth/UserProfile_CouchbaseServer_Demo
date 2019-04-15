using System;

namespace UserProfileDemo.Models
{
    public class UserEvent
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public EventType EventType { get; set; }
        public string Type => "userEvent";
    }
}
