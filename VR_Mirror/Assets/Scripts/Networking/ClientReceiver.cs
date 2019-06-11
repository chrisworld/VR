using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimatorControl
{
  public Animator animator = null;
  public bool lock_position = true;
  public Animator track_position_animator = null;
}

public class ClientReceiver : MonoBehaviour
{


  public string ServerIPAddress = "172.28.0.1";
  public int SendPort = 2223;
  public int ReceivePort = 2222;
  public TextAsset PrerecordedData = null;
  public Transform HMDTrackingTransform;
  public Animator RetargetingAvatarAnimator;
  public List<AnimatorControl> DestinationAvatarAnimators = null;


  // Only change this if you want to record something from the network. If you're running this on mobile, you will probably need to use PersistentDataPath for storing.
  private string RecordingPath = "";

  // Human Pose Handlers
  private HumanPoseHandler source_pose_handler;
  private List<HumanPoseHandler> dest_pose_handler = null;

  private TrackingDataProcessor data_processor;
  private UDPClient client;


  void SetupHumanPoseHandlers()
  {

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // STUDENT_TODO: 1.) Setup HumanPoseHandlers
    // 1a.) Setup HumanPoseHandler for "source", using RetargetingAvatarAnimator.avatar and RetargetingAvatarAnimator.transform
    // 1b.) Setup HumanPoseHandlers for "destination", using DestinationAvatarAnimators[...].animator.avatar and DestinationAvatarAnimators[...].animator.transform
    //       
    // With these handlers, we can retarget a humanoid animation from our RetargetingAvatar to any other humanoid avatar, using Unity's Mecanim Animation system.
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // -- 1a) 
    // source handler
    source_pose_handler = new HumanPoseHandler(RetargetingAvatarAnimator.avatar, RetargetingAvatarAnimator.transform);

    // -- 1b)
    // destination handler
    foreach (AnimatorControl dest_anim in DestinationAvatarAnimators)
    {
      dest_pose_handler.Add(new HumanPoseHandler(dest_anim.animator.avatar, dest_anim.animator.transform));
    }
  }


  // Start is called before the first frame update
  void Start()
  {

    SetupHumanPoseHandlers();


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

    // Uncomment this for network logging
    //data_processor.enable_logging = true;

    // Register all necessary callbacks for networking and recorded binary data
    data_processor.RegisterCallback<VRVUTypes.OptitrackPoseWithID>(NetworkMessageID.OptitrackPose, OptitrackPoseCallback);
    data_processor.RegisterCallback<VRVUTypes.OptitrackSkeletonState>(NetworkMessageID.OptitrackSkeletonState, OptitrackSkeletonStateCallback);

    // Either use networking data or prerecorded data
    if (PrerecordedData == null)
    {
      client = new UDPClient();
      client.StartClient(ServerIPAddress, SendPort, ReceivePort, data_processor);
    }
    else
    {
      data_processor.LoadFromBytes(PrerecordedData.bytes);
    }

  }

  void OptitrackPoseCallback(VRVUTypes.OptitrackPoseWithID pose)
  {
    HMDTrackingTransform.position = new Vector3(pose.Position.x, pose.Position.y, pose.Position.z);
    HMDTrackingTransform.rotation = pose.Orientation;
  }

  void OptitrackSkeletonStateCallback(VRVUTypes.OptitrackSkeletonState skeleton_state)
  {
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // STUDENT_TODO:
    // Implement animation retargeting by using skeleton_state
    //
    //       2.) Iterate over skeleton_state.BonePoses and set the position/rotation of the hidden RetargetingAvatar (RetargetingAvatarAnimator)
    //           skeleton_state.BonePoses is a Dictionary, where the key is equal to the HumanBodyBones Unity enum.
    //           Animator.GetBoneTransform can be used to get the respective transform object of the RetargetingAvatar
    //           Finally, make sure that the bone transform is set to the respective values inside skeleton_state.BonePoses.
    //           NOTE: skeleton_state.BonePoses may contain bones that are not inside the RetargetingAvatarAnimator!
    //                 In this case, RetargetingAvatarAnimator.GetBoneTransform will return null.
    //           required Functions: Animator.GetBoneTransform
    //           required Classes/Structs: HumanBodyBones, VRVUTypes.OptitrackPose, Transform
    //
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //
    //       3.) After we have our animated, hidden RetargetingAvatar (you can verify this by enabling "RetargetingAvatar" in Unity), we can
    //           retarget the humanoid animation from this RetargetingAvatar to the Destination Avatar(s). 
    //           For this, we use the GetHumanPose and SetHumanPose Unity/Mecanim functions.
    //           This will require the HumanPoseHandlers we initialized inside the Start method. 
    //           First, we call GetHumanPose on our "source" HumanPoseHandler that corresponds to the RetargetingAvatar. 
    //           We pass a HumanPose as a reference, which will be updated according to the newly set pose from 1.)
    //           Then we iterate over all destination pose handlers, and call SetHumanPose with the HumanPose we got from GetHumanPose before.
    //           Additionally, if to lock your destination avatars in place (only moving in place),
    //             you have to lock their position to the base position (DestinationAvatarAnimators[...].animator.transform)
    //           You can toggle the locking by checking for DestinationAvatarAnimators[...].lock_position,
    //             which can be set in the Unity inspector of the ClientReceiver script.
    //           Finally, since differently scaled objects lead to offsets in translation/position, you will need to adjust the position of your destination target
    //             if you want to use absolute coordinates of full-body tracking. You can use the "RetargetingAvatar's" hip position for this
    //             (DestinationAvatarAnimators[...].track_position_animator.GetBoneTransform(HumanBodyBones.Hips)). You can then apply just the x/y movement of the
    //             RetargetingAvatar such that the translation is the same in world coordinates, but the movement is still correctly scaled to the model.
    //
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //
    //       4.) Build your own virtual hall of mirrors! 
    //           Find free models on the internet, add them to your scene in interesting ways and let them mirror the streamed pose!
    //           DestinationAvatarAnimators allows for an arbitrary number of avatars - however, make sure that your phone can handle it performance-wise!
    //           Feel free to edit the pose-retargeting code and/or anything else in the framework if you need extra special handling.
    //           HINT: To mirror the movement, you will probably need to rotate and/or scale the models accordingly, which can simply be done in the Unity Scene Editor.
    //
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // 1
    //skeleton_state.BonePoses
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
