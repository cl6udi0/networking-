using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

// ==========================================
// 1. CONFIGURATION & ENTRY POINT RULES
// ==========================================
bool debug = true;

if (args.Length > 0)
{
    ProcessCommand(args[0]);
}
else
{
    if (debug) Console.WriteLine("No arguments detected. Starting the Network Server...");
    RunServer();
}

// ==========================================
// 2. CORE BUSINESS LOGIC METHOD
// ==========================================
// REMOVED 'static' keyword so your local server functions can call it safely
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

// ==========================================
// 3. NETWORK SERVER METHODS
// ==========================================
void RunServer()
{
    TcpListener listener;
    Socket connection;
    NetworkStream socketStream;
    try
    {
        // Note: Port 443 requires running your application as Administrator.
        // If it crashes with a SocketException, change 443 to a testing port like 8080.
        listener = new TcpListener(IPAddress.Any, 443);
        listener.Start();
        while (true)
        {
            if (debug) Console.WriteLine("Server Waiting connection...");
            connection = listener.AcceptSocket();
            socketStream = new NetworkStream(connection);
            doRequest(socketStream);
            socketStream.Close();
            connection.Close();
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
        StreamWriter sw = new StreamWriter(socketStream);
        StreamReader sr = new StreamReader(socketStream);

        if (debug) Console.WriteLine("Waiting for input from client...");
        string? line = sr.ReadLine();

        if (line == null) return;

        Console.WriteLine($"Received Network Command: '{line}'");

        // FIXED: Moved the HTTP validation routing logic INSIDE the doRequest brackets
        if (line == "POST / HTTP/1.1")
        {
            // Then we have an update
            if (debug) Console.WriteLine("Received an update request");
        }
        else
        {
            // Then we have a lookup
            if (debug) Console.WriteLine("Received a lookup request");
        }

        // Send HTTP Response headers back to the browser or network tool
        sw.WriteLine("HTTP/1.1 400 Bad Request");
        sw.WriteLine("Content-Type: text/plain");
        sw.WriteLine();
        sw.Flush();
    } // <-- Braces close properly here now
}



//    /// Handle a network request
void doRequest(NetworkStream socketStream)
{
    StreamWriter sw = new StreamWriter(socketStream);
    StreamReader sr = new StreamReader(socketStream);

    if (debug) Console.WriteLine("Waiting for input from client...");
    string? line = sr.ReadLine();

    if (string.IsNullOrEmpty(line)) return;

    Console.WriteLine($"Received Network Command: '{line}'");

    if (line.StartsWith("POST"))
    {
        if (debug) Console.WriteLine("Received an update request");

        try
        {
            // Task 8: Mock-up an update to a fixed test string as required by the assignment sheet
            // Note: Ensure your data dictionary matches 'DataBase' exactly in your application
            DataBase["cssbct"].Location = "network update test string";

            // Send an HTTP 200 OK response back to the client/browser to confirm the mock update
            sw.WriteLine("HTTP/1.1 200 OK");
            sw.WriteLine("Content-Type: text/plain");
            sw.WriteLine();
            sw.WriteLine("Mock update successful for ID 'cssbct'");
            sw.Flush();

            Console.WriteLine("Performed mock update on 'cssbct' setting location to 'network update test string'");
        }
        catch (Exception ex)
        {
            sw.WriteLine("HTTP/1.1 500 Internal Server Error");
            sw.WriteLine("Content-Type: text/plain");
            sw.WriteLine();
            sw.Flush();

            if (debug) Console.WriteLine($"Error executing mock update: {ex.Message}");
        }
    }
    else
    {
        if (debug) Console.WriteLine("Received a lookup request");

        try
        {
            // Task 7: Extract the name string from the middle of the HTTP request line
            string[] slices = line.Split(' ');           // Split into 3 pieces
            string ID = slices[1].Substring(7);          // Start at 7th index to skip "/?name="

            // Check if the ID exists in your lab dictionary
            if (DataBase.ContainsKey(ID))
            {
                string result = DataBase[ID].Location;

                // Send successful HTTP 200 OK sequence back to the client
                sw.WriteLine("HTTP/1.1 200 OK");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.WriteLine(result);
                sw.Flush();

                Console.WriteLine($"Performed Lookup on '{ID}' returning '{result}'");
            }
            else
            {
                // Send HTTP 404 Not Found sequence back to the client
                sw.WriteLine("HTTP/1.1 404 Not Found");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.Flush();

                Console.WriteLine($"Performed Lookup on '{ID}' returning '404 Not Found'");
            }
        }
        catch (Exception ex)
        {
            // Fallback response if the HTTP request format was malformed
            sw.WriteLine("HTTP/1.1 400 Bad Request");
            sw.WriteLine("Content-Type: text/plain");
            sw.WriteLine();
            sw.Flush();

            if (debug) Console.WriteLine($"Error parsing lookup line details: {ex.Message}");
        }
    }
}
