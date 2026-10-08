using System;
using System.Collections.Generic;

namespace whois
{
    public class User
    {
        static Dictionary<string, User> DataBase = new Dictionary<string, User>
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

        public string UserID { get; set; }
        public string Surname { get; set; }
        public string Fornames { get; set; }
        public string Title { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }

        public static void Dump(string ID)
        {
            Console.WriteLine($"UserID={DataBase[ID].UserID}");
            Console.WriteLine($"Surname={DataBase[ID].Surname}");
            Console.WriteLine($"Fornames={DataBase[ID].Fornames}");
            Console.WriteLine($"Title={DataBase[ID].Title}");
            Console.WriteLine($"Position={DataBase[ID].Position}");
            Console.WriteLine($"Phone={DataBase[ID].Phone}");
            Console.WriteLine($"Email={DataBase[ID].Email}");
            Console.WriteLine($"location={DataBase[ID].Location}");
        }

        public static void Lookup(string ID, string field)
        {
            string result = null;

            switch (field)
            {
                case "location":
                    result = DataBase[ID].Location;
                    break;

                case "UserID":
                    result = DataBase[ID].UserID;
                    break;

                case "Surname":
                    result = DataBase[ID].Surname;
                    break;

                case "Fornames":
                    result = DataBase[ID].Fornames;
                    break;

                case "Title":
                    result = DataBase[ID].Title;
                    break;

                case "Phone":
                    result = DataBase[ID].Phone;
                    break;

                case "Position":
                    result = DataBase[ID].Position;
                    break;

                case "Email":
                    result = DataBase[ID].Email;
                    break;
            }

            Console.WriteLine(result);
        }

        public static void Update(string ID, string field, string update)
        {
            switch (field)
            {
                case "location":
                    DataBase[ID].Location = update;
                    break;

                case "UserID":
                    DataBase[ID].UserID = update;
                    break;

                case "Surname":
                    DataBase[ID].Surname = update;
                    break;

                case "Fornames":
                    DataBase[ID].Fornames = update;
                    break;

                case "Title":
                    DataBase[ID].Title = update;
                    break;

                case "Phone":
                    DataBase[ID].Phone = update;
                    break;

                case "Position":
                    DataBase[ID].Position = update;
                    break;

                case "Email":
                    DataBase[ID].Email = update;
                    break;
            }

            Console.WriteLine("OK");
        }
    }
}