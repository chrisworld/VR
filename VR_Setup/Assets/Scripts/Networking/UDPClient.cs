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

	private UdpClient client;
	private IPEndPoint remote_endpoint;

	// Start is called before the first frame update
	public void StartClient(string multicast_ip, int multicast_port, TrackingDataProcessor data_processor)
	{
		this.data_processor = data_processor;
		client = new UdpClient();
		IPAddress multicast_address = IPAddress.Parse(multicast_ip);
		IPEndPoint local_endpoint = new IPEndPoint(IPAddress.Any, multicast_port);
		client.Client.Bind(local_endpoint);
		client.JoinMulticastGroup(multicast_address);
		try
		{
			client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
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

		byte[] received = client.EndReceive(res, ref remote_endpoint);


		data_processor.handleNetworkMessage(received);

		client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
	}

	public void Destroy()
	{
		client.Close();
		client = null;
	}
}
