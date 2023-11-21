using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TCPTelemetry1 : MonoBehaviour
{
    //[SerializeField] public string serverIpAddress = "127.0.0.1";
    [SerializeField] public int serverPort = 80;

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            // Create a TCP client socket
            string serverIpAddress = VarManager.varEndpointURI_EventIngest;
            Debug.Log("[ INFO ] Sending ML data to " + serverIpAddress + ":" + serverPort);
            using (var client = new TcpClient(serverIpAddress, serverPort))
            {
                // Convert the UTC time to a Unix timestamp with millisecond precision
                var utcNow = DateTimeOffset.UtcNow;
                var unixTimestampMilliseconds = utcNow.ToUnixTimeMilliseconds();
                System.Random random = new System.Random();

                // Create the payload data as a JSON string
                var payload = $@"
                {{
                    ""eventid"": ""eventid_{unixTimestampMilliseconds}"",
                    ""eventtype"": ""ingame_action"",
                    ""timestamp"": {unixTimestampMilliseconds},
                    ""playerid"": ""{SetPlayerID.playerIDGlobal}"",
                    ""label"": ""Interaction"",
                    ""xcoord"": {random.NextDouble()},
                    ""ycoord"": {random.NextDouble()},
                    ""zcoord"": {random.NextDouble()},
                    ""dow"": {(int)utcNow.DayOfWeek},
                    ""hour"": {utcNow.Hour},
                    ""score"": {random.Next(1, 100)},
                    ""minutesplayed"": {random.Next(0, 60)},
                    ""timeinstore"": {random.Next(0, 30)}
                }}";

                Debug.Log("Payload");
                Debug.Log(payload);

                // Convert the payload string to bytes
                var payloadBytes = Encoding.UTF8.GetBytes(payload);

                // Get the client network stream
                var stream = client.GetStream();

                // Send the payload bytes to the server
                stream.Write(payloadBytes, 0, payloadBytes.Length);

                // Read the response bytes from the server
                var responseBytes = new byte[client.ReceiveBufferSize];
                var bytesRead = stream.Read(responseBytes, 0, client.ReceiveBufferSize);

                // Convert the response bytes to a string
                var response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

                // Log the response received from the server
                Debug.Log($"TCP response received: {response}");

                // Close the stream and client socket
                stream.Close();
                client.Close();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while sending TCP request: {e}");
        }
    }
}
