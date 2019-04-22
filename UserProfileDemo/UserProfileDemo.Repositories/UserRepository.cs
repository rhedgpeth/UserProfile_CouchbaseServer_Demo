using System;
using System.Linq;
using System.Collections.Generic;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;
using Couchbase.N1QL;
using UserProfileDemo.Models;
using Couchbase.Search.Queries.Simple;
using Couchbase.Search.Queries.Compound;
using Couchbase.Search;
using System.Threading.Tasks;
using Couchbase;

namespace UserProfileDemo.Repositories
{
    public class UserRepository
    {
        private readonly IBucket _bucket;
        private readonly BucketContext _bucketContext;

        public UserRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("user_profile");
            _bucketContext = new BucketContext(_bucket);
        }

        public async Task<User> FindByIdAsync(string id)
        {
            User user = null;

            var result = await _bucket.GetAsync<User>(id);

            if (result?.Value != null)
            {
                user = result.Value;
                user.Id = id;
            }

            return user;
        }

        public void Save(User user)
        {
            _bucket.Upsert(user.Id, new
            {
                user.Addresses,
                user.CountryCode,
                user.Enabled,
                user.FirstName,
                user.LastName,
                user.MiddleName,
                user.Password,
                user.Preferences,
                user.SecurityRoles,
                user.SocialSecurityNumber,
                user.Telephones,
                user.TenantId,
                user.UserName,
                user.Type
            });
        }

        public Task SaveAsync(User user)
        {
            return _bucket.UpsertAsync(user.Id, new
            {
                user.Addresses,
                user.CountryCode,
                user.Enabled,
                user.FirstName,
                user.LastName,
                user.MiddleName,
                user.Password,
                user.Preferences,
                user.SecurityRoles,
                user.SocialSecurityNumber,
                user.Telephones,
                user.TenantId,
                user.UserName,
                user.Type
            });
        }

        public IOperationResult Remove(string key) => _bucket.Remove(key);

        public IOperationResult Remove(User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null!");
            }

            return Remove(user.Id);
        }

        public Task<IOperationResult> RemoveAsync(string key) => _bucket.RemoveAsync(key);

        public Task<IOperationResult> RemoveAsync(User user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null!");
            }

            return RemoveAsync(user.Id);
        }

        public void RemoveAll()
        {
            var users = GetAll();

            foreach(var user in users)
            {
                var result = Remove(user);

                if (!result.Success)
                {
                    Console.WriteLine($"ERROR Deleting Id={user?.Id}: {result.Message}");
                }
            }
        }

        public async Task RemoveAllAsync()
        {
            var users = GetAll();
            
            foreach (var user in users)
            {
                var result = await RemoveAsync(user);

                if (!result.Success)
                {
                    Console.WriteLine($"ERROR Deleting Id={user?.Id}: {result.Message}");
                }
            }
        }

        IQueryRequest GetAllQueryRequest()
        {
            var n1ql = $"select * from `{_bucket.Name}`";

            return QueryRequest.Create(n1ql);
        }

        // This is used for testing purposes only
        List<User> GetAll() =>_bucket?.Query<User>(GetAllQueryRequest())?.Rows;

        // This is used for testing purposes only
        async Task<List<User>> GetAllAsync()
        {
            var results = await _bucket?.QueryAsync<User>(GetAllQueryRequest());

            if (results.Success && results != null)
            {
                return results.Rows;
            }

            return null;
        }

        public async Task<List<User>> ListTenantUsers(int tenantId, int offset, int limit)
        {
            var users = new List<User>();

            var n1ql = $@"Select meta().id as id, username, tenantId, firstName, lastname
                from `{_bucket.Name}`
                where type = 'user'
                and tenantId = $tenantId
                order by firstName asc
                limit $limit
                offset $offset";

            var query = QueryRequest.Create(n1ql);
            query.AddNamedParameter("tenantId", tenantId);
            query.AddNamedParameter("limit", limit);
            query.AddNamedParameter("offset", offset);

            var results = await _bucket.QueryAsync<User>(query);

            if (results != null && results.Success && results.Rows.Count > 0)
            {
                users = results.Rows;
            }

            return users;
        }

        public Task<List<User>> FindActiveUsersByFirstNameAsync(string firstName, bool enabled, string countryCode, int limit, int offset)
        {
            return Task.Run(() =>
            {
                var results = _bucketContext.Query<User>()
                    .Where(u => u.Type == "user")
                    .Where(u => u.FirstName.ToLower() == firstName.ToLower())
                    .Where(u => u.Enabled == enabled)
                    .Where(u => u.CountryCode == countryCode)
                    .Select(u => new { key = N1QlFunctions.Meta(u).Id, document = u })
                    .Skip(offset)
                    .Take(limit)
                    .ToList();
                    
                results?.ForEach(r => r.document.Id = r.key);

                return results?.Select(r => r.document)?.ToList();
            });
        }

        public async Task<List<User>> FtsListActiveUsersAsync(string firstName, bool enabled, string countryCode, int limit, int skip)
        {
            // Allows a match with a Levenshtein (Edit) Distance of 1.
            var firstNameFuzzy = new MatchQuery(firstName).Fuzziness(1).Field("firstName");

            // This is the exact match for the term.
            var firstNameSimple = new MatchQuery(firstName).Field("firstName");

            //Disjunction queries are similar to "OR" operators in SQL
            var nameQuery = new DisjunctionQuery(firstNameSimple, firstNameFuzzy);

            var isEnabled = new BooleanFieldQuery(enabled).Field("enabled");
            var countryFilter = new MatchQuery(countryCode).Field("countryCode");

            // Conjunction queries are similar to "AND" operators in SQL
            var conj = new ConjunctionQuery(nameQuery, isEnabled, countryFilter);

            var searchQuery = new SearchQuery();

            // Indicate the fields we'd like returned in the search results.
            searchQuery.Fields("id", "tenantId", "firstName", "lastName", "userName", "enabled");

            // Assign the FTS index we've created.
            searchQuery.Index = "user_index";

            // Assign our combined query.
            searchQuery.Query = conj;
            searchQuery.Skip(skip);
            searchQuery.Limit(limit);

            var result = await _bucket.QueryAsync(searchQuery);

            var users = new List<User>();

            if (result != null && result.Success)
            {
                foreach (var hit in result.Hits)
                {
                    var user = new User
                    {
                        Id = hit.Id,
                        TenantId = int.Parse(hit.Fields["tenantId"].ToString()),
                        FirstName = hit.Fields["firstName"],
                        LastName = hit.Fields["lastName"],
                        UserName = hit.Fields["userName"],
                        Enabled = hit.Fields["enabled"]
                    };

                    users.Add(user);
                }
            }

            return users;
        }

        public Task AddEventAsync(UserEvent userEvent) => AddEventsAsync(new List<UserEvent> { userEvent });

        public Task AddEventsAsync(List<UserEvent> events)
        {
            var tasks = events.Select(e => _bucket.InsertAsync(e.Id, new
            {
                e.CreatedDate,
                e.EventType,
                e.UserId,
                e.Type
            }));

            return Task.WhenAll(tasks);
        }

        public async Task<List<UserEvent>> FindLatestUserEventsAsync(string userId, EventType eventType, int limit, int offset)
        {
            var userEvents = new List<UserEvent>();

            var n1ql = $@"SELECT META(e).id, e.userId, e.createdDate, e.eventType
                            FROM `{_bucket.Name}` e
                            WHERE e.type = 'userEvent'
                            AND e.eventType = $eventType
                            AND e.userId = $userId
                            LIMIT $limit
                            OFFSET $offset";

            var query = QueryRequest.Create(n1ql);
            query.AddNamedParameter("eventType", eventType);
            query.AddNamedParameter("userId", userId);
            query.AddNamedParameter("limit", limit);
            query.AddNamedParameter("offset", offset);

            var result = await _bucket.QueryAsync<UserEvent>(query);

            if (result != null && result.Success)
            {
                userEvents = result.Rows;
            }

            return userEvents;
        }

        // For testing purposes only!!!
        public void ClearBucket()
        {
            var n1ql = $"delete from `{_bucket.Name}`";

            var query = QueryRequest.Create(n1ql);

            _bucket.Query<dynamic>(query);
        }
    }
}
