using UnityEngine;

namespace VRVUTypes
{
	[System.Serializable]
	public struct SerializableVector
	{
		public float x;
		public float y;
		public float z;

		public SerializableVector(Vector3 vec)
		{
			x = vec.x;
			y = vec.y;
			z = vec.z;
		}

		public Vector3 ToVector()
		{
			return new Vector3(x, y, z);
		}

	}


	[System.Serializable]
	public struct SerializableQuaternion
	{
		public float w;
		public float x;
		public float y;
		public float z;

		public SerializableQuaternion(Quaternion quat)
		{
			w = quat.w;
			x = quat.x;
			y = quat.y;
			z = quat.z;
		}

		public Quaternion ToQuaternion()
		{
			return new Quaternion(x, y, z, w);
		}
	}


	/// <summary>Describes the position and orientation of a streamed tracked object.</summary>
	/// Taken from OptitrackStreamingClient.cs
	public class OptitrackPose
	{
		public Vector3 Position;
		public Quaternion Orientation;
	}

	public class OptitrackPoseWithID
	{
		public Vector3 Position;
		public Quaternion Orientation;
		public int ID;
	}

	[System.Serializable]
	public struct SerializableOptitrackPose
	{
		public SerializableVector Position;
		public SerializableQuaternion Orientation;

		public OptitrackPose ToOptitrackPose()
		{
			OptitrackPose pose = new OptitrackPose();
			pose.Position = Position.ToVector();
			pose.Orientation = Orientation.ToQuaternion();
			return pose;
		}
	}


	[System.Serializable]
	public struct SerializableOptitrackPoseWithID
	{
		public SerializableVector Position;
		public SerializableQuaternion Orientation;
		public int ID;

		public OptitrackPoseWithID ToOptitrackPoseWithID()
		{
			OptitrackPoseWithID pose = new OptitrackPoseWithID();
			pose.Position = Position.ToVector();
			pose.Orientation = Orientation.ToQuaternion();
			pose.ID = ID;
			return pose;
		}
	}


}


