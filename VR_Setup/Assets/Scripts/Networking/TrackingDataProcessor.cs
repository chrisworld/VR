using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using VRVUTypes;

public interface INetworkMessageHandler
{
	void handleNetworkMessage(byte[] data);
}

public enum NetworkMessageID
{
	TrackingTest = 0,
	OptitrackPose = 1,
	OptitrackSkeleton = 2,
	UNUSED = 3
}

public static class ByteArraySerializer
{
	public static byte[] Serialize<T>(this T m)
	{
		using (var ms = new MemoryStream())
		{
			new BinaryFormatter().Serialize(ms, m);
			return ms.ToArray();
		}
	}

	public static T Deserialize<T>(this byte[] byteArray)
	{
		using (var ms = new MemoryStream(byteArray))
		{
			return (T)new BinaryFormatter().Deserialize(ms);
		}
	}
}



public class TrackingDataProcessor : INetworkMessageHandler
{

	private Dictionary<NetworkMessageID, Action<object>> message_callbacks;
	private Dictionary<NetworkMessageID, List<object>> data_queue;
	private Mutex data_queue_lock = new Mutex();
	private bool record_data = false;
	private string record_path = "";
	private List<byte[]> recorded_bytes;
	private int recorded_bytes_index = 0;
	private Mutex recorded_bytes_lock = new Mutex();
	private bool recorded_bytes_repeat = true;
	private bool playback_from_file = false;
	private float last_recorded_time_stamp = 0;
	private float last_update_time_stamp = 0;

	public TrackingDataProcessor()
	{

	}

	public TrackingDataProcessor(string recording_path)
	{
		record_path = recording_path;
		record_data = true;
		recorded_bytes = new List<byte[]>();
	}

	public void handleNetworkMessage(byte[] data)
	{
		//Debug.Log("handleNetworkMessage");
		Stream stream = new MemoryStream(data);

		// If we are in record mode, record the binary data
		if (record_data)
		{
			recorded_bytes_lock.WaitOne();
			recorded_bytes.Add(data);
			recorded_bytes_lock.ReleaseMutex();
		}

		NetworkMessageID message_id = (NetworkMessageID)stream.ReadByte();
		byte[] time_stamp_bytes = new byte[4];

		stream.Read(time_stamp_bytes, 0, sizeof(float));
		float time_stamp = BitConverter.ToSingle(time_stamp_bytes, 0);


		if (data_queue.ContainsKey(message_id) == false)
		{
			Debug.LogError("Error: Callback for message ID " + message_id.ToString() + " has not been setup!");
			return;
		}

		//Debug.Log("Received Message of Type: " + message_id);

		if (message_id == NetworkMessageID.TrackingTest)
		{
			// Read a vec3, call the corresponding callback that was defined for this message
			byte[] all_bytes = new byte[stream.Length];
			stream.Read(all_bytes, 0, (int)stream.Length);
			SerializableVector received_vector = new SerializableVector();
			try
			{
				received_vector = ByteArraySerializer.Deserialize<SerializableVector>(all_bytes);
			}
			catch (Exception ex)
			{
				Debug.LogError("Deserialization Error: " + ex.ToString());
			}

			AddToDataQueue(message_id, received_vector.ToVector());
		}
		else if (message_id == NetworkMessageID.OptitrackPose)
		{
			// Read an optitrack pose, call the corresponding callback that was defined for this message
			byte[] all_bytes = new byte[stream.Length];
			stream.Read(all_bytes, 0, (int)stream.Length);
			SerializableOptitrackPoseWithID received_pose = new SerializableOptitrackPoseWithID();
			try
			{
				received_pose = ByteArraySerializer.Deserialize<SerializableOptitrackPoseWithID>(all_bytes);
			}
			catch (Exception ex)
			{
				Debug.LogError("Deserialization Error: " + ex.ToString());
			}

			AddToDataQueue(message_id, received_pose.ToOptitrackPoseWithID());
		}
		else
		{
			Debug.LogError("Unknown Message ID: " + message_id.ToString());
		}

	}

	public void RegisterCallback<T>(NetworkMessageID id, Action<T> callback)
	{
		if (message_callbacks == null)
		{
			message_callbacks = new Dictionary<NetworkMessageID, Action<object>>();
		}

		message_callbacks.Add(id, o => callback((T)o));
		data_queue_lock.WaitOne();

		if (data_queue == null)
		{
			data_queue = new Dictionary<NetworkMessageID, List<object>>();
		}

		if (data_queue.ContainsKey(id) == false)
		{
			data_queue[id] = new List<object>();
		}


		data_queue_lock.ReleaseMutex();
	}

	public void LoadFromFile(string filename, bool repeat = true)
	{
		// Note: This is very simple, but should be sufficient for testing. It basically loads raw binary "messages" as they were recorded.

		record_data = false;
		record_path = "";
		recorded_bytes_index = 0;
		recorded_bytes_repeat = repeat;
		playback_from_file = true;

		recorded_bytes = ByteArraySerializer.Deserialize<List<byte[]>>(File.ReadAllBytes(filename));
	}

	public void LoadFromBytes(byte[] bytes, bool repeat = true)
	{
		// Note: This is very simple, but should be sufficient for testing. It basically loads raw binary "messages" as they were recorded.

		record_data = false;
		record_path = "";
		recorded_bytes_index = 0;
		recorded_bytes_repeat = repeat;
		playback_from_file = true;

		recorded_bytes = ByteArraySerializer.Deserialize<List<byte[]>>(bytes);
	}

	public void Update()
	{
		// This simply passes all recorded byte arrays into the same handleNetworkMessage handler as if we were using networking.
		if (playback_from_file == true && recorded_bytes.Count != 0)
		{


			if (recorded_bytes_repeat)
			{
				float time_stamp = 0;
				float chosen_time_stamp = 0;

				bool repeat = true;
				bool first_frame_future = true;

				while (repeat)
				{
					repeat = false;

					chosen_time_stamp = time_stamp;
					time_stamp = BitConverter.ToSingle(recorded_bytes[recorded_bytes_index % recorded_bytes.Count], 1);

					if (last_recorded_time_stamp <= 0 && last_update_time_stamp <= 0)
					{
						// This handles offsets at the start of the path
						last_update_time_stamp = time_stamp;
						last_recorded_time_stamp = time_stamp;
					}


					// HACK: handle the edge case of wraparound when using a looping recording, it's annoying to handle the timestamps otherwise.
					if (recorded_bytes_index % recorded_bytes.Count == 0)
					{
						// This effectively always forces it to stop at frame 0.
						recorded_bytes_index++;
						chosen_time_stamp = time_stamp;
						break;
					}

					if ((time_stamp - last_recorded_time_stamp) > (Time.time - last_update_time_stamp))
					{
						// When the timestamp is too far in the future, we skip this frame
						if (first_frame_future)
							return;

					}
					else if ((time_stamp - last_recorded_time_stamp) <= (Time.time - last_update_time_stamp))
					{
						// If we are behind, we can repeat and try to catch up.
						repeat = true;
						recorded_bytes_index++;
						first_frame_future = false;
					}

				}

				// After this loop, we have the chosen time_stamp (which is one behind our "checked" timestamp) as well as the respective byte array index
				recorded_bytes_index--;
				last_recorded_time_stamp = chosen_time_stamp;

				handleNetworkMessage(recorded_bytes[recorded_bytes_index % recorded_bytes.Count]);
			}
			else if (recorded_bytes_index < recorded_bytes.Count)
			{

				float time_stamp = 0;
				float chosen_time_stamp = 0;

				bool repeat = true;
				bool first_frame_future = true;

				while (repeat && recorded_bytes_index < recorded_bytes.Count)
				{
					repeat = false;

					chosen_time_stamp = time_stamp;
					time_stamp = BitConverter.ToSingle(recorded_bytes[recorded_bytes_index], 1);

					if (last_recorded_time_stamp <= 0 && last_update_time_stamp <= 0)
					{
						// This handles offsets at the start of the path
						last_update_time_stamp = time_stamp;
						last_recorded_time_stamp = time_stamp;
					}


					if ((time_stamp - last_recorded_time_stamp) > (Time.time - last_update_time_stamp))
					{
						// When the timestamp is too far in the future, we skip this frame
						if (first_frame_future)
							return;

					}
					else if ((time_stamp - last_recorded_time_stamp) <= (Time.time - last_update_time_stamp))
					{
						// If we are behind, we can repeat and try to catch up.
						repeat = true;
						recorded_bytes_index++;

						first_frame_future = false;
					}

				}

				// After this loop, we have the chosen time_stamp (which is one behind our "checked" timestamp) as well as the respective byte array index
				recorded_bytes_index--;
				last_recorded_time_stamp = chosen_time_stamp;

				handleNetworkMessage(recorded_bytes[recorded_bytes_index]);
			}

			recorded_bytes_index++;
		}

		last_update_time_stamp = Time.time;
	}

	public void ProcessQueuedCallbacks()
	{
		data_queue_lock.WaitOne();
		foreach (KeyValuePair<NetworkMessageID, Action<object>> entry in message_callbacks)
		{
			if (!data_queue.ContainsKey(entry.Key))
			{
				continue;
			}

			int queue_size = data_queue[entry.Key].Count;
			for (int data_index = queue_size - 1; data_index >= 0; data_index--)
			{
				entry.Value(data_queue[entry.Key][data_index]);
			}

			data_queue[entry.Key].Clear();
		}

		data_queue_lock.ReleaseMutex();
	}

	private void AddToDataQueue(NetworkMessageID id, object item)
	{
		data_queue_lock.WaitOne();
		data_queue[id].Add(item);
		data_queue_lock.ReleaseMutex();
	}


	public void OnDestroy()
	{
		// If we're recording, save the recorded binary data
		if (!record_data)
		{
			return;
		}

		// Note: This is very simple, but should be sufficient for testing. It basically stores raw binary "messages" as they were recorded.
		recorded_bytes_lock.WaitOne();
		byte[] raw_data = ByteArraySerializer.Serialize(recorded_bytes);
		recorded_bytes_lock.ReleaseMutex();

		File.WriteAllBytes(record_path, raw_data);
	}
}
