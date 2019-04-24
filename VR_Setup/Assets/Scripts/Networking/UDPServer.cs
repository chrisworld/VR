using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;



public class UDPServer
{
	private UdpClient client;
	private IPEndPoint remote_ep;

	// Start is called before the first frame update
	public void Start(string multicast_ip, int multicast_port)
	{
		client = new UdpClient();
		IPAddress multicast_address = IPAddress.Parse(multicast_ip);
		client.JoinMulticastGroup(multicast_address);
		remote_ep = new IPEndPoint(multicast_address, multicast_port);
	}


	public void Send(NetworkMessageID id, float time_stamp, byte[] serialized_data)
	{
		byte[] serialized_message = PrepareMessageWithID(serialized_data, (byte)id, time_stamp);
		client.Send(serialized_message, serialized_message.Length, remote_ep);
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
