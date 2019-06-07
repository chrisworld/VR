using System.Collections.Generic;
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


  [System.Serializable]
  public struct SerializableOptitrackSkeletonDefinition
  {
    [System.Serializable]
    public class BoneDefinition
    {
      /// <summary>The ID of this bone within this skeleton.</summary>
      public int Id;

      /// <summary>The ID of this bone's parent bone. A value of 0 means that this is the root bone.</summary>
      public int ParentId;

      /// <summary>The name of this bone.</summary>
      public string Name;

      /// <summary>
      /// This bone's position offset from its parent in the skeleton's neutral pose.
      /// (The neutral orientation is always <see cref="Quaternion.identity"/>.)
      /// </summary>
      public SerializableVector Offset;
    }

    /// <summary>Skeleton ID. Used as an argument to <see cref="OptitrackStreamingClient.GetLatestSkeletonState"/>.</summary>
    public int Id;

    /// <summary>Skeleton asset name.</summary>
    public string Name;

    /// <summary>Bone names, hierarchy, and neutral pose position information.</summary>
    public List<BoneDefinition> Bones;
  }

  [System.Serializable]
  public struct SerializableOptitrackSkeletonState
  {
    /// <summary>Maps from Unity bone IDs to their corresponding bone poses.</summary>
    public Dictionary<HumanBodyBones, SerializableOptitrackPose> BonePoses;

    public OptitrackSkeletonState ToOptitrackSkeletonState()
    {
      OptitrackSkeletonState skeleton_state = new OptitrackSkeletonState();
      skeleton_state.BonePoses = new Dictionary<HumanBodyBones, OptitrackPose>();
      foreach (KeyValuePair<HumanBodyBones, VRVUTypes.SerializableOptitrackPose> bone_pose_pair in BonePoses)
      {
        skeleton_state.BonePoses[bone_pose_pair.Key] = bone_pose_pair.Value.ToOptitrackPose();
      }

      return skeleton_state;
    }
  }


  public struct OptitrackSkeletonState
  {
    public Dictionary<HumanBodyBones, OptitrackPose> BonePoses;
  }
}


