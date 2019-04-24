using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRVUTypes;

public class ServerSenderTest : MonoBehaviour
{

	public string MultiCastIPAddress = "224.0.0.251";
	public int MultiCastPort = 2222;
	private UDPServer server;
	public Transform test_object;

	// Start is called before the first frame update
	void Start()
	{
		server = new UDPServer();
		server.Start(MultiCastIPAddress, MultiCastPort);
	}


	// FixedUpdate is called at a specific time interval, independent of framerate
	void FixedUpdate()
	{
		//TODO: interface with Optitrack to send specific messages depending on what we need

		Vector3 vec = test_object.position;

		Quaternion rot = test_object.rotation;

		SerializableOptitrackPoseWithID pose = new SerializableOptitrackPoseWithID();

		pose.Position = new SerializableVector(vec);
		pose.Orientation = new SerializableQuaternion(rot);
		pose.ID = 0;
		server.Send(NetworkMessageID.OptitrackPose, Time.time, ByteArraySerializer.Serialize(pose));
	}

	private void OnDestroy()
	{
		if (server != null)
		{
			server.Destroy();
		}
	}
}
