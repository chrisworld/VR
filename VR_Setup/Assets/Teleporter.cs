using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
  public GameObject teleport_object;

  public float gaze_range = 0.1f;

  public int gaze_thresh = 5;
  public int look_frame_update = 5;
  public int gaze_frame_update = 45;



  private Vector3 old_pos;
  private int gaze_frame;
  private int look_frame;
  private bool gazing;


  void Start()
  {
    teleport_object = GameObject.Find("Head");
    old_pos = Vector3.zero;
    gaze_frame = 0;
    look_frame = 0;
    gazing = false;
  }

  public void Teleport(Vector3 new_pos, GameObject interact_circle)
  {
    
    // just looking
    if (!gazing)
    {
      look_frame += 1;

      if (look_frame > look_frame_update && !gazing)
      {
        look_frame = 0;

        if (InGazeRange(new_pos))
        {
          gaze_frame += 1;
          //Debug.Log("gaze_frame: " + gaze_frame);
        }
        else
        {
          gaze_frame = 0;
          old_pos = new_pos;
        }

        if (gaze_frame > gaze_thresh)
        {
          gazing = true;
          gaze_frame = 0;
          interact_circle.SetActive(true);
        }
      }
    }

    // gazing
    else
    {
      if (InGazeRange(new_pos))
      {
        gaze_frame += 1;
        Color circ_color = interact_circle.GetComponent<SpriteRenderer>().color;
        circ_color.a = 1f - (float)gaze_frame / gaze_frame_update;
        interact_circle.GetComponent<SpriteRenderer>().color = circ_color;
      }
      else
      {
        look_frame = 0;
        gaze_frame = 0;
        gazing = false;
        interact_circle.SetActive(false);
      }

      // update gaze
      if (gaze_frame > gaze_frame_update)
      {
        // teleport
        teleport_object.transform.position = new Vector3(new_pos.x, teleport_object.transform.position.y, new_pos.z);
        gazing = false;
        gaze_frame = 0;
        look_frame = 0;
        interact_circle.SetActive(false);
      }
    }

    // update look
  



  }

  // range
  private bool InGazeRange(Vector3 new_pos)
  {
    //Vector3 range_pos = new Vector3(Mathf.Abs(old_pos.x) + gaze_range, Mathf.Abs(old_pos.y) + gaze_range, Mathf.Abs(old_pos.z) + gaze_range);

    Vector3 range_vector = new Vector3(Mathf.Abs(new_pos.x - old_pos.x),  Mathf.Abs(new_pos.y - old_pos.y), Mathf.Abs(new_pos.z - old_pos.z));

    if (range_vector.x < gaze_range  && range_vector.y < gaze_range && range_vector.z < gaze_range)
    {
      //Debug.Log("in range: " + "new_pos " + new_pos + " old_pos: " + old_pos + "range: " + range_vector);
      return true;
    }

    return false;
  } 
}
