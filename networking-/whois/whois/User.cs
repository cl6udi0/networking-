using System;
using System.Collections.Generic;

namespace whois
{
    public class User
    {
        // Public so Program.cs can access DataBase directly
        public static Dictionary<string, User> DataBase = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "cssbct",
                new User
                {
                    UserID = "cssbct",
                    Surname = "Tompsett",
                    Fornames = "Brian C",
                    Title = "Eur Ing",
                    Position = "Lecturer of Computer Science",
                    Phone = "+44 1482 46 5222",
                    Email = "B.C.Tompsett@hull.ac.uk",
                    Location = "in RB-336"
                }
            }
        };

        // Properties (cleaned duplicate definitions)
        public string UserID { get; set; } = string.Empty;

        // Alias ID to UserID so both 'user.ID' and 'user.UserID' work seamlessly
        public string ID
        {
            get => UserID;
            set => UserID = value;
        }

        public string Surname { get; set; } = string.Empty;
        public string Fornames { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public static void Dump(string id)
        {
            if (DataBase.TryGetValue(id, out var user))
            {
                Console.WriteLine($"UserID={user.UserID}");
                Console.WriteLine($"Surname={user.Surname}");
                Console.WriteLine($"Fornames={user.Fornames}");
                Console.WriteLine($"Title={user.Title}");
                Console.WriteLine($"Position={user.Position}");
                Console.WriteLine($"Phone={user.Phone}");
                Console.WriteLine($"Email={user.Email}");
                Console.WriteLine($"location={user.Location}");
            }
            else
            {
                Console.WriteLine($"User '{id}' not found.");
            }
        }

        public static void Lookup(string id, string? field)
        {
            if (!DataBase.TryGetValue(id, out var user))
            {
                Console.WriteLine($"User '{id}' not found.");
                return;
            }

            string? result = field?.ToLower() switch
            {
                "location" => user.Location,
                "userid" => user.UserID,
                "surname" => user.Surname,
                "fornames" => user.Fornames,
                "title" => user.Title,
                "phone" => user.Phone,
                "position" => user.Position,
                "email" => user.Email,
                _ => null
            };

            Console.WriteLine(result ?? "Field unknown");
        }

        public static void Update(string id, string? field, string updateValue)
        {
            if (!DataBase.TryGetValue(id, out var user))
            {
                user = new User { UserID = id };
                DataBase[id] = user;
            }

            switch (field?.ToLower())
            {
                case "location":
                    user.Location = updateValue;
                    break;
                case "userid":
                    user.UserID = updateValue;
                    break;
                case "surname":
                    user.Surname = updateValue;
                    break;
                case "fornames":
                    user.Fornames = updateValue;
                    break;
                case "title":
                    user.Title = updateValue;
                    break;
                case "phone":
                    user.Phone = updateValue;
                    break;
                case "position":
                    user.Position = updateValue;
                    break;
                case "email":
                    user.Email = updateValue;
                    break;
                default:
                    user.Location = updateValue; // Fallback default
                    break;
            }

            Console.WriteLine("OK");
        }
    }
}