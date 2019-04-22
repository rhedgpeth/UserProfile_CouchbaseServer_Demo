using System;
using Microsoft.Extensions.DependencyInjection;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using UserProfileDemo.Repositories;
using UserProfileDemo.Models;
using System.Collections.Generic;

namespace UserProfileDemo.Console
{
    class Program
    {
        static IServiceProvider ServiceProvider { get; set; }

        static UserRepository _userRepository;
        static UserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = ServiceProvider.GetService<UserRepository>(); ;
                }

                return _userRepository;
            }
        }

        static void Main(string[] args)
        {
            InitializeServiceProvider();

            try
            {
                var action = args[0];

                if (action == "add")
                {
                    int? userCount = null;
                    string firstName = null;
                    string countryCode = null;
                    bool? isEnabled = null;

                    for (int i = 1; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "-c":
                                userCount = int.Parse(args[i + 1]);
                                break;
                            case "-fn":
                                firstName = args[i + 1];
                                break;
                            case "-e":
                                isEnabled = bool.Parse(args[i + 1]);
                                break;
                            case "-cc":
                                countryCode = args[i + 1];
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(firstName) &&
                        isEnabled.HasValue && !string.IsNullOrEmpty(countryCode))
                    {
                        AddUser(firstName, isEnabled.Value, countryCode);
                    }
                    else if (userCount.HasValue)
                    {
                        AddUsers(userCount.Value);
                    }
                }
                else if (action == "clear")
                {
                    Clear();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            //System.Console.ReadKey(true);
        }

        static void InitializeServiceProvider()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            // Setup our DI
            ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddCouchbase(configuration.GetSection("Couchbase"))
                .AddTransient<UserRepository>()
                .BuildServiceProvider();
        }

        static void AddUser(string firstName, bool enabled, string countryCode)
        {
            var user = FakeUser.Create(firstName, enabled, countryCode);

            UserRepository.Save(user);

            System.Console.WriteLine($"User {firstName} saved successfully!");
        }

        static void AddUsers(int userCount)
        {
            var users = new List<User>();

            for (var i = 0; i < userCount; i++)
            {
                var user = FakeUser.Create();

                UserRepository.Save(user);

                System.Console.WriteLine($"Added user - Id={user.Id}");

                users.Add(user);
            }

            if (users?.Count > 0)
            {
                System.Console.WriteLine($"{users.Count} users saved successfully!");
            }
        }

        static void Clear()
        {
            UserRepository.ClearBucket();
            System.Console.WriteLine("Bucket cleared.");
        }
    }
}
