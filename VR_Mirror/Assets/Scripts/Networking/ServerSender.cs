using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRVUTypes;

public class ServerSender : MonoBehaviour
{
  public int MultiCastPort = 2222;
  public int ReceivePort = 2223;
  private UDPServer server;
  public Transform HeadTransform;
  public Animator AvatarAnimator;
  // Start is called before the first frame update
  void Start()
  {
    server = new UDPServer();
    server.Start(MultiCastPort, ReceivePort);
  }


  // FixedUpdate is called at a specific time interval, independent of framerate
  void Update()
  {
    SerializableOptitrackSkeletonState skeleton_state = new SerializableOptitrackSkeletonState();

    var bone_enums = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();

    skeleton_state.BonePoses = new Dictionary<HumanBodyBones, SerializableOptitrackPose>();
    foreach (var bone_enum in bone_enums)
    {
      if (bone_enum == HumanBodyBones.LastBone)
        break;

      var bone_transform = AvatarAnimator.GetBoneTransform(bone_enum);

      if (bone_transform == null)
        continue;

      //skeleton_state.BonePoses[enum_int] = new SerializableOptitrackPose();
      var new_pose = new SerializableOptitrackPose();

      new_pose.Position = new SerializableVector(bone_transform.position);
      new_pose.Orientation = new SerializableQuaternion(bone_transform.rotation);
      skeleton_state.BonePoses[bone_enum] = new_pose;
    }



    Vector3 vec = HeadTransform.position;

    Quaternion rot = HeadTransform.rotation;

    SerializableOptitrackPoseWithID pose = new SerializableOptitrackPoseWithID();

    pose.Position = new SerializableVector(vec);
    pose.Orientation = new SerializableQuaternion(rot);
    pose.ID = 0;
    server.Send(NetworkMessageID.OptitrackPose, Time.time, ByteArraySerializer.Serialize(pose));

    server.Send(NetworkMessageID.OptitrackSkeletonState, Time.time, ByteArraySerializer.Serialize(skeleton_state));
  }

  private void OnDestroy()
  {
    if (server != null)
    {
      server.Destroy();
    }
  }
}
