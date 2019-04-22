using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserProfileDemo.Models;
using UserProfileDemo.Repositories;

namespace UserProfileDemo.API.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET api/users/user::05b6cc06-b48c-4b3d-be09-d2d442a3c975
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userRepository.FindByIdAsync(id);

            return Ok(user);
        }

        // GET: api/users?tenantid=2
        // GET: api/users?tenantid=2&offset=1
        // GET: api/users?tenantid=2&offset=0&limit=25
        [HttpGet]
        public async Task<IActionResult> Get(int tenantId, int offset = 0, int limit = 10)
        {
            var result = await _userRepository.ListTenantUsers(tenantId, offset, limit);

            return Ok(result);
        }

        // GET: api/users/filter?firstName=Rob&enabled=true&countryCode=BO&limit=5&offset=0
        [HttpGet]
        [Route("filter")]
        public async Task<IActionResult> Get(string firstName, bool? enabled, string countryCode, int limit = 10, int offset = 0)
        {
            if (string.IsNullOrEmpty(firstName))
                return BadRequest("FirstName is required!");
            if (!enabled.HasValue)
                return BadRequest("Enabled is required!");
            if (string.IsNullOrEmpty(countryCode))
                return BadRequest("CountryCode is required!");

            var result = await _userRepository.FindActiveUsersByFirstNameAsync(firstName, enabled.Value, countryCode, limit, offset);

            return Ok(result);
        }

        // GET: api/users/search?firstName=Rob&enabled=true&countryCode=BO&limit=5&offset=0
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search(string firstName, bool? enabled, string countryCode, int limit = 10, int skip = 0)
        {
            if (string.IsNullOrEmpty(firstName))
                return BadRequest("FirstName is required!");
            if (!enabled.HasValue)
                return BadRequest("Enabled is required!");
            if (string.IsNullOrEmpty(countryCode))
                return BadRequest("CountryCode is required!");

            var result = await _userRepository.FtsListActiveUsersAsync(firstName, enabled.Value, countryCode, limit, skip);

            return Ok(result);
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]User user)
        {
            if (user == null)
            {
                BadRequest("Insufficient user data!");
            }

            await _userRepository.SaveAsync(user);

            Ok();
        }

        // DELETE api/users/user::05b6cc06-b48c-4b3d-be09-d2d442a3c975
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var user = await _userRepository.RemoveAsync(id);

            Ok(user);
        }
    }
}
