using System;

Boolean debug = true;

ProcessCommand(args.Length > 0 ? args[0] : "");

static void ProcessCommand(string command)
{
    Console.WriteLine($"\nCommand: {command}");

    string[] slice = command.Split(new char[] { '?' }, 2);

    string ID = slice[0];
    string operation = null;
    string field = null;
    string update = null;

    if (slice.Length == 2)
    {
        operation = slice[1];

        string[] pieces = operation.Split(new char[] { '=' }, 2);

        field = pieces[0];

        if (pieces.Length == 2)
        {
            update = pieces[1];
        }
    }

    Console.Write($"Operation on ID '{ID}'");

    if (operation == null)
    {
        Console.WriteLine(" output all fields");
        whois.User.Dump(ID);
    }
    else if (update == null)
    {
        Console.WriteLine($" lookup field '{field}'");
        whois.User.Lookup(ID, field);
    }
    else
    {
        Console.WriteLine($" update field '{field}' to '{update}'");
        whois.User.Update(ID, field, update);
    }
}