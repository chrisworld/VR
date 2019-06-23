using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public class UDPServer
{
  private UdpClient client;
  private IPEndPoint remote_ep;
  private Dictionary<IPEndPoint, bool> endpoints;
  private Mutex endpoints_lock = new Mutex();
  int send_port = 2222;
  // Start is called before the first frame update
  public void Start(int port, int receive_port)
  {
    client = new UdpClient();

    IPEndPoint receive_endpoint = new IPEndPoint(IPAddress.Any, receive_port);

    client.Client.Bind(receive_endpoint);
    endpoints = new Dictionary<IPEndPoint, bool>();
    send_port = port;

    try
    {
      client.BeginReceive(new AsyncCallback(ReceiveCallback), client);
    }
    catch (Exception e)
    {
      Debug.Log("Exception when calling client.BeginReceive: " + e.ToString());
    }
  }

  private void ReceiveCallback(IAsyncResult res)
  {
    if (client == null)
      return;


    //endpoints_lock.WaitOne();
    UdpClient c = (UdpClient)res.AsyncState;
    IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
    Byte[] receivedBytes = c.EndReceive(res, ref receivedIpEndPoint);

    // Make sure the client is registered
    if (endpoints.ContainsKey(receivedIpEndPoint) == false)
    {
      Debug.Log("Registered client with endpoint: " + receivedIpEndPoint.ToString());

    }
    endpoints[receivedIpEndPoint] = true;

    //endpoints_lock.ReleaseMutex();

    //Debug.Log("UDPServer: received message from " + receivedIpEndPoint.ToString() + ": " + receivedBytes.ToString());


    // Restart listening for udp data packages
    c.BeginReceive(ReceiveCallback, res.AsyncState);
    
  }


  public void Send(NetworkMessageID id, float time_stamp, byte[] serialized_data)
  {
    //endpoints_lock.WaitOne();
    foreach (var endpoint in endpoints)
    {
      byte[] serialized_message = PrepareMessageWithID(serialized_data, (byte)id, time_stamp);
      var new_ep = new IPEndPoint(endpoint.Key.Address, send_port);
      client.Send(serialized_message, serialized_message.Length, new_ep);
      //Debug.Log("Sent message to endpoint: " + new_ep.ToString());
    }
    //endpoints_lock.ReleaseMutex();
  }

  private byte[] PrepareMessageWithID(byte[] array, byte id, float time_stamp)
  {
    byte[] new_array = new byte[array.Length + sizeof(byte) + sizeof(float)];
    array.CopyTo(new_array, sizeof(byte) + sizeof(float));
    new_array[0] = id;
    byte[] time_stamp_bytes = BitConverter.GetBytes(time_stamp);
    new_array[1] = time_stamp_bytes[0];
    new_array[2] = time_stamp_bytes[1];
    new_array[3] = time_stamp_bytes[2];
    new_array[4] = time_stamp_bytes[3];
    return new_array;
  }

  public void Destroy()
  {
    client.Close();
  }
}
