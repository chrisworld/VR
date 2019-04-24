#define FULL_VERSION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClientReceiver : MonoBehaviour
{

	public string MultiCastIPAddress = "224.0.0.251";
	public int MultiCastPort = 2222;
	public TextAsset PrerecordedData = null;

	// for movement of head
	private Transform head_transform;

	// Only change this if you want to record something from the network. If you're running this on mobile, you will probably need to use PersistentDataPath for storing.
	private string RecordingPath = "";


	private TrackingDataProcessor data_processor;
	private UDPClient client;

	// Start is called before the first frame update
	void Start()
	{
		if (RecordingPath == "")
		{
			// Don't record anything.
			data_processor = new TrackingDataProcessor();
		}
		else
		{
			// Record binary data as received from the network
			data_processor = new TrackingDataProcessor(RecordingPath);
		}

		// Register all necessary callbacks for networking and recorded binary data
		data_processor.RegisterCallback<VRVUTypes.OptitrackPoseWithID>(NetworkMessageID.OptitrackPose, OptitrackPoseCallback);

		// Either use networking data or prerecorded data
		if (PrerecordedData == null)
		{
			client = new UDPClient();
			client.StartClient(MultiCastIPAddress, MultiCastPort, data_processor);
		}
		else
		{
			data_processor.LoadFromBytes(PrerecordedData.bytes);
		}

		// init head object
		head_transform = GameObject.Find("Head").transform;
	}


	void OptitrackPoseCallback(VRVUTypes.OptitrackPoseWithID pose)
	{
		//TODO: apply position and orientation to your head transform.
		head_transform.position = pose.Position;
		head_transform.rotation = pose.Orientation;
	}

	// FixedUpdate is called at a specific time interval, independent of framerate
	void FixedUpdate()
	{
		// When using file-mode, this will make sure that the data from the file(s) is passed into the data queues.
		data_processor.Update();

		// This method will call all registered callbacks of the data processor with all messages that have arrived since then, in the order they were received.
		data_processor.ProcessQueuedCallbacks();
	}

	private void OnDestroy()
	{
		// Destroy network connection, if exists
		if (client != null)
		{
			client.Destroy();
		}

		// Write recorded data, if necessary
		data_processor.OnDestroy();
	}
}
