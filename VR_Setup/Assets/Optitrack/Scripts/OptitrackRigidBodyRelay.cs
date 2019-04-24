// This file is taken and modified from the OptiTrack SDK to relay the RigidBody information via local
// multicast using our custom networking. You will not need this unless you interface with OptiTrack
// and/or Motive yourself.
// ~ April 2019, Thomas Neff
#if OPTITRACK_STREAMING_RELAY

//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRVUTypes;


public class OptitrackRigidBodyRelay : MonoBehaviour
{
	public OptitrackStreamingClient StreamingClient;
	public Int32 RigidBodyId;

	public string MultiCastIPAddress = "239.0.0.222";
	public int MultiCastPort = 2222;
	private UDPServer server;
	public Transform test_object;

	// Start is called before the first frame update
	void Start()
	{
		server = new UDPServer();
		server.Start(MultiCastIPAddress, MultiCastPort);
		// If the user didn't explicitly associate a client, find a suitable default.
		if (this.StreamingClient == null)
		{
			this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

			// If we still couldn't find one, disable this component.
			if (this.StreamingClient == null)
			{
				Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
				this.enabled = false;
				return;
			}
		}
	}

	private void OnDestroy()
	{
		if (server != null)
		{
			server.Destroy();
		}
	}


#if UNITY_2017_1_OR_NEWER
	void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
	}


	void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
	}


	void OnBeforeRender()
	{
		UpdatePose();
	}
#endif


	void Update()
	{
		UpdatePose();
	}


	void UpdatePose()
	{
		OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId);
		if (rbState != null)
		{
			//TODO: interface with Optitrack to send specific messages depending on what we need

			Vector3 vec = rbState.Pose.Position;

			Quaternion rot = rbState.Pose.Orientation;

			SerializableOptitrackPoseWithID pose = new SerializableOptitrackPoseWithID();

			pose.Position = new SerializableVector(vec);
			pose.Orientation = new SerializableQuaternion(rot);
			pose.ID = 0;
			server.Send(NetworkMessageID.OptitrackPose, Time.time, ByteArraySerializer.Serialize(pose));


			Debug.Log("pos: " + vec);
			Debug.Log("rot: " + rot);

			test_object.position = vec;
			test_object.rotation = rot;
		}
	}
}
#endif