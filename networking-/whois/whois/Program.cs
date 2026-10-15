using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using whois;


Boolean debug = true;

// Laboratory DataBase initialization matching lab sheet line 9
Dictionary<string, User> DataBase = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase)
{
    ["cssbct"] = new User { ID = "cssbct", Location = "Initial Location" }
};

if (args.Length == 0)
{
    Console.WriteLine("Starting Server");
    RunServer();
}
else
{
    ProcessCommand(args[0]);
}

// 2. LOCAL HELPER FUNCTIONS / BUSINESS LOGIC


/// Process the next database command request
void ProcessCommand(string command)
{
    Console.WriteLine($"\nCommand: {command}");

    string[] slice = command.Split(new char[] { '?' }, 2);

    string ID = slice[0];
    string? operation = null;
    string? field = null;
    string? update = null;

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
        Dump(ID);
    }
    else if (update == null)
    {
        Console.WriteLine($" lookup field '{field}'");
        Lookup(ID, field);
    }
    else
    {
        Console.WriteLine($" update field '{field}' to '{update}'");
        Update(ID, field, update);
    }
}

/// Functions to process database requests (Local Functions matching lab specification)
void Dump(string ID)
{
    if (DataBase.TryGetValue(ID, out var user))
    {
        Console.WriteLine($"ID: {user.ID}, Location: {user.Location}");
    }
    else
    {
        Console.WriteLine($"User '{ID}' not found.");
    }
}

void Lookup(string ID, string field)
{
    if (DataBase.TryGetValue(ID, out var user))
    {
        if (string.Equals(field, "location", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Location: {user.Location}");
        }
        else
        {
            Console.WriteLine($"Field '{field}' unknown.");
        }
    }
    else
    {
        Console.WriteLine($"User '{ID}' not found.");
    }
}

void Update(string ID, string field, string updateValue)
{
    if (DataBase.TryGetValue(ID, out var user))
    {
        user.Location = updateValue;
        Console.WriteLine($"Updated '{ID}' location to '{updateValue}'");
    }
    else
    {
        DataBase[ID] = new User { ID = ID, Location = updateValue };
        Console.WriteLine($"Created '{ID}' with location '{updateValue}'");
    }
}


// 3. NETWORK SERVER METHOD


/// Initiate the Web Server task
void RunServer()
{
    int port = 443; // Use 8080 or 443 if running Visual Studio as Administrator
    TcpListener listener;

    try
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        if (debug) Console.WriteLine($"Server started listening on port {port}...");

        while (true)
        {
            if (debug) Console.WriteLine("Server Waiting connection...");
            using (Socket connection = listener.AcceptSocket())

            using (NetworkStream socketStream = new NetworkStream(connection))
            {
                doRequest(socketStream);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Exception in Server: " + e.ToString());
    }

    if (debug) Console.WriteLine("Terminating Server");

    /// Handle a network request
    void doRequest(NetworkStream socketStream)
    {
        StreamWriter sw = new StreamWriter(socketStream) { AutoFlush = true };
        StreamReader sr = new StreamReader(socketStream);

        if (debug) Console.WriteLine("Waiting for input from client...");
        string? line = sr.ReadLine();
        if (line == null)
        {
            if (debug) Console.WriteLine("Ignoring null command");
            return;
        }

        if (string.IsNullOrEmpty(line)) return;

        Console.WriteLine($"Received Network Command: '{line}'");

        // Consume the rest of the HTTP headers sent by the client so the buffer clears safely
        string? headerLine;
        while (!string.IsNullOrEmpty(headerLine = sr.ReadLine()))
        {
            // This loops until it hits the blank line (\r\n) that ends HTTP headers
        }

        if (line.StartsWith("POST", StringComparison.OrdinalIgnoreCase))
        {
            if (debug) Console.WriteLine("Received an update request");

            try
            {
                // Task 8: Mock-up an update to a fixed test string as required by the assignment sheet
                DataBase["cssbct"].Location = "network update test string";

                SendHttpResponse(sw, "200 OK", "Mock update successful for ID 'cssbct'\r\n");
                Console.WriteLine("Performed mock update on 'cssbct' setting location to 'network update test string'");
            }
            catch (Exception ex)
            {
                SendHttpResponse(sw, "500 Internal Server Error", $"Server Error: {ex.Message}\r\n");
                if (debug) Console.WriteLine($"Error executing mock update: {ex.Message}");
            }
        }
        else
        {
            if (debug) Console.WriteLine("Received a lookup request");

            try
            {
                // Task 7: Extract the name string from the HTTP request line
                string[] slices = line.Split(' ');

                if (slices.Length < 2)
                {
                    SendHttpResponse(sw, "400 Bad Request", "Malformed Request Line\r\n");
                    return;
                }

                string requestPath = slices[1];
                string ID = string.Empty;

                if (requestPath.StartsWith("/?name="))
                {
                    ID = requestPath.Substring(7);
                }
                else if (requestPath.StartsWith("/?"))
                {
                    ID = requestPath.Substring(2);
                }
                else
                {
                    ID = requestPath.TrimStart('/');
                }

                if (!string.IsNullOrEmpty(ID) && DataBase.TryGetValue(ID, out var user))
                {
                    string result = user.Location;
                    SendHttpResponse(sw, "200 OK", result + "\r\n");
                    Console.WriteLine($"Performed Lookup on '{ID}' returning '{result}'");
                }
                else
                {
                    SendHttpResponse(sw, "404 Not Found", "404 Not Found\r\n");
                    Console.WriteLine($"Performed Lookup on '{ID}' returning '404 Not Found'");
                }
            }
            catch (Exception ex)
            {
                SendHttpResponse(sw, "400 Bad Request", "Bad Request\r\n");
                if (debug) Console.WriteLine($"Error parsing lookup line details: {ex.Message}");
            }
        }
    }
    /// Process the next database command request
    void ProcessCommand(string command)
    {
        if (debug) Console.WriteLine($"\nCommand: {command}");
        try
        {
            String[] slice = command.Split(new char[] { '?' }, 2);
            String ID = slice[0];
            String operation = null;
            String update = null;
            String field = null;
            if (slice.Length == 2)
            {
                operation = slice[1];
                String[] pieces = operation.Split(new char[] { '=' }, 2);
                field = pieces[0];
                if (pieces.Length == 2) update = pieces[1];
            }
            if (debug) Console.Write($"Operation on ID '{ID}'");
            if (operation == null) Dump(ID);
            else if (update == null) Lookup(ID, field);
            else Update(ID, field, update);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fault in Command Processing: {e.ToString()}");
        }
    }
     

    void SendHttpResponse(StreamWriter writer, string status, string body)
    {
        writer.Write($"HTTP/1.1 {status}\r\n");
        writer.Write("Content-Type: text/plain\r\n");
        writer.Write($"Content-Length: {System.Text.Encoding.UTF8.GetByteCount(body)}\r\n");
        writer.Write("\r\n");
        writer.Write(body);
    }
}
