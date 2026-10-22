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
    try
    {
        string[] slice = command.Split(new char[] { '?' }, 2);

        string ID = slice[0];
        string? operation = null;
        string? field = null;
        string? update = null;

        if (slice.Length == 2)
        {
            operation = slice[1];

            // Task 5: Handle record delete command if operation is empty (e.g. "whois userId?")
            if (operation == "")
            {
                Delete(ID);
                return;
            }

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
    catch (Exception e)
    {
        Console.WriteLine($"Fault in Command Processing: {e.ToString()}");
    }
}

/// Functions to process database requests
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
    // Task 5: Ensure key is in database before updating
    if (!DataBase.ContainsKey(ID)) DataBase.TryAdd(ID, new User { ID = ID });

    if (DataBase.TryGetValue(ID, out var user))
    {
        user.Location = updateValue;
        Console.WriteLine($"Updated '{ID}' location to '{updateValue}'");
    }
}

void Delete(string ID)
{
    if (debug) Console.WriteLine($"Delete record '{ID}' from DataBase");
    DataBase.Remove(ID);
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

            try
            {
                using (Socket connection = listener.AcceptSocket())
                {
                    // Task 3: Set timeouts to 1000ms (1 second)[cite: 5]
                    connection.SendTimeout = 1000;
                    connection.ReceiveTimeout = 1000;

                    using (NetworkStream socketStream = new NetworkStream(connection))
                    {
                        doRequest(socketStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error or timeout: {ex.Message}");
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

        try
        {
            string? line = sr.ReadLine();

            // Task 2: Ignore null/empty commands[cite: 4]
            if (line == null)
            {
                if (debug) Console.WriteLine("Ignoring null command");
                return;
            }

            if (string.IsNullOrEmpty(line)) return;

            Console.WriteLine($"Received Network Command: '{line}'");

            if (line.StartsWith("POST", StringComparison.OrdinalIgnoreCase))
            {
                if (debug) Console.WriteLine("Received an update request");

                // Task 4: Parse headers to find Content-Length and skip to blank line[cite: 5, 6]
                int content_length = 0;
                while (line != "")
                {
                    if (line.StartsWith("Content-Length: "))
                    {
                        content_length = Int32.Parse(line.Substring(16));
                    }
                    line = sr.ReadLine();
                    if (debug) Console.WriteLine($"Skipped Header Line: '{line}'");
                }

                // Task 4: Read exactly content_length characters from the stream body[cite: 5, 6]
                line = "";
                for (int i = 0; i < content_length; i++)
                {
                    line += (char)sr.Read();
                }

                // Task 4 & 5: Extract ID and value, handle missing keys[cite: 6, 7]
                string[] slices = line.Split(new char[] { '&' }, 2);
                if (slices.Length >= 2)
                {
                    string ID = slices[0].Substring(5);
                    string value = slices[1].Substring(9);

                    if (debug) Console.WriteLine($"Received an update request for '{ID}' to '{value}'");

                    if (!DataBase.ContainsKey(ID)) DataBase.TryAdd(ID, new User { ID = ID });
                    DataBase[ID].Location = value;

                    SendHttpResponse(sw, "200 OK", $"Update successful for ID '{ID}'\r\n");
                }
                else
                {
                    SendHttpResponse(sw, "400 Bad Request", "Malformed POST Body\r\n");
                }
            }
            else
            {
                if (debug) Console.WriteLine("Received a lookup request");

                // Consume the rest of the HTTP headers sent by the client so the buffer clears safely
                string? headerLine;
                while (!string.IsNullOrEmpty(headerLine = sr.ReadLine()))
                {
                    // Loops until blank line
                }

                // Task 7 (from previous lab): Extract name string from HTTP request line
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
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fault in Command Processing: {e.ToString()}");
        }
        finally
        {
            // Task 1: Ensure streams are closed in finally block[cite: 4]
            sw.Close();
            sr.Close();
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