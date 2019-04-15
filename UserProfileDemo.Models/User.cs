using System.Collections.Generic;

namespace UserProfileDemo.Models
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public bool Enabled { get; set; }
        public int TenantId { get; set; }
        public string CountryCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SocialSecurityNumber { get; set; }
        public List<Telephone> Telephones { get; set; }
        public List<Preference> Preferences { get; set; }
        public List<Address> Addresses { get; set; }
        public List<string> SecurityRoles { get; set; }
        public string Type => "user";
    }
}
