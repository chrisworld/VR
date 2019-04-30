using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
  public GameObject teleport_object;

  // Start is called before the first frame update
  void Start()
  {
    teleport_object = GameObject.Find("Head");
  }

  public void Teleport(Vector3 new_pos, GameObject interact_circle)
  {
    teleport_object.transform.position = new Vector3(new_pos.x, teleport_object.transform.position.y, new_pos.z);
  }
}
