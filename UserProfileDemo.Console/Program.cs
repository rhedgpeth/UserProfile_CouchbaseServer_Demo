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
                    int userCount = 100;
                    bool addEvents = false;

                    for (int i = 1; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "-c":
                                userCount = int.Parse(args[i + 1]);
                                break;
                            case "-e":
                                addEvents = true;
                                break;
                        }
                    }

                    AddUsers(userCount, addEvents);
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

        static void AddUsers(int userCount,  bool addEvents)
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
                if (addEvents)
                {
                    AddEventsToUsers(users);
                }

                System.Console.WriteLine($"{users.Count} users saved successfully!");
            }
        }

        static async void AddEventsToUsers(List<User> users)
        {
            int randomMax = (int)(users.Count * .1);

            // Add random(ish) user events
            foreach(var user in users)
            {
                // Pull first 10% users (to increase event density)
                var randomUser = users[Faker.RandomNumber.Next(0, randomMax)];
                var fakeUserActivity = FakeUserEvent.Create(randomUser.Id);

                await UserRepository.AddEventAsync(fakeUserActivity);
            }
        }

        static void Clear()
        {
            UserRepository.ClearBucket();
            System.Console.WriteLine("Bucket cleared.");
        }
    }
}
