using System;

namespace UserProfileDemo.Models
{
    public static class FakeUserEvent
    {
        public static UserEvent Create(string userId)
        {
            var userEvent = new UserEvent
            {
                Id = "userevent::" + Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now.AddSeconds(-1 * Faker.RandomNumber.Next(0, 300)),
                UserId = userId
            };

            switch (Faker.RandomNumber.Next(0, 3))
            {
                case 0:
                    userEvent.EventType = EventType.ProductAddedToCart;
                    break;
                case 1:
                    userEvent.EventType = EventType.ProductViewed;
                    break;
                case 2:
                    userEvent.EventType = EventType.ProfileUpdated;
                    break;
            }

            return userEvent;
        }
    }
}
