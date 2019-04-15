using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileDemo.Models
{
    public static class FakeUser
    {
        public static User Create(string firstName = null, bool? enabled = null, string countryCode = null)
        {
            var user = new User
            {
                Addresses = CreateFakeAddresses(),
                CountryCode = countryCode ?? Faker.Address.Country().Substring(0, 2).ToUpper(),
                Enabled = enabled ?? (Faker.RandomNumber.Next(100, 200) % 2) == 0,
                FirstName = firstName ?? Faker.Name.First(),
                Id = "user::" + Guid.NewGuid().ToString(),
                LastName = Faker.Name.Last(),
                MiddleName = Faker.Name.First(),
                Password = Faker.Lorem.Words(1).First(),
                Preferences = CreateFakePreferences(),
                SecurityRoles = CreateFakeSecurityRoles(),
                SocialSecurityNumber = Faker.RandomNumber.Next(111111111, 999999999).ToString(),
                Telephones = CreateFakeTelephones(),
                TenantId = Faker.RandomNumber.Next(1, 10),
                UserName = Faker.Internet.UserName()
            };

            return user;
        }

        private static List<Address> CreateFakeAddresses()
        {
            var num = Faker.RandomNumber.Next(1, 3);
            var addresses = new List<Address>();

            for (var i = 0; i < num; i++)
            {
                var address = new Address
                {
                    City = Faker.Address.City(),
                    CountryCode = Faker.Address.Country().Substring(0, 2).ToUpper(),
                    Name = Faker.Name.FullName(),
                    Number = Faker.RandomNumber.Next(100, 9999).ToString(),
                    State = Faker.Address.UsState(),
                    Street = Faker.Address.StreetName(),
                    ZipCode = Faker.Address.ZipCode()
                };

                addresses.Add(address);
            }

            return addresses;
        }

        private static List<Preference> CreateFakePreferences()
        {
            var num = Faker.RandomNumber.Next(1, 3);
            var prefs = new List<Preference>();

            for (var i = 0; i < num; i++)
            {
                var pref = new Preference
                {
                    Name = Faker.Lorem.Words(1).First(),
                    Value = Faker.Internet.DomainWord()
                };

                prefs.Add(pref);
            }
            return prefs;
        }

        private static List<string> CreateFakeSecurityRoles()
        {
            var roles = new List<string>
            {
                "USER",
                "ADMIN",
                "EDITOR",
                "VIEWER",
                "MANAGE"
            };

            var num = Faker.RandomNumber.Next(1, 4);

            for (var i = 0; i < num; i++)
            {
                roles.RemoveAt(Faker.RandomNumber.Next(0, roles.Count));
            }

            return roles;
        }

        private static List<Telephone> CreateFakeTelephones()
        {
            var num = Faker.RandomNumber.Next(1, 3);
            var phones = new List<Telephone>();
            var phoneNames = new List<string> { "cell", "home", "office", "emergency" };

            for (var i = 0; i < num; i++)
            {
                var phone = new Telephone
                {
                    Name = phoneNames[Faker.RandomNumber.Next(0, phoneNames.Count)],
                    Number = Faker.Phone.Number()
                };

                phones.Add(phone);
            }

            return phones;
        }
    }
}
