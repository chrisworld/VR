using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient
{
  private TrackingDataProcessor data_processor;

  private UdpClient client_receive;
  private UdpClient client_send;
  private IPEndPoint remote_endpoint;

  // Start is called before the first frame update
  public void StartClient(string server_ip, int send_port, int receive_port, TrackingDataProcessor data_processor)
  {
    this.data_processor = data_processor;

    IPAddress server_address = IPAddress.Parse(server_ip);
    IPEndPoint send_endpoint = new IPEndPoint(server_address, send_port);
    IPEndPoint receive_endpoint = new IPEndPoint(IPAddress.Any, receive_port);

    client_send = new UdpClient();
    client_send.Connect(send_endpoint);
   
    Debug.Log("Sending handshake to server");
    client_send.Send(new byte[] { 1, 3, 3, 7 }, 4);
    Debug.Log("Handshake sent");

    client_receive = new UdpClient();
    client_receive.Client.Bind(receive_endpoint);

    try
    {
      client_receive.BeginReceive(new AsyncCallback(ReceiveCallback), client_receive);
    }
    catch (Exception e)
    {
      Debug.Log("Exception when calling client.BeginReceive: " + e.ToString());
    }
  }

  private void ReceiveCallback(IAsyncResult res)
  {
    if (client_receive == null)
      return;

    byte[] received = client_receive.EndReceive(res, ref remote_endpoint);

    client_receive.BeginReceive(new AsyncCallback(ReceiveCallback), null);

    data_processor.handleNetworkMessage(received);
  }

  public void Destroy()
  {
    client_receive.Close();
    client_receive = null;

    client_send.Close();
    client_send = null;
  }
}
