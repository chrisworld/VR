using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallKicker : MonoBehaviour
{

  public float hitForce = 0.5f;

  // Start is called before the first frame update
  void Start()
  {
      
  }

  // Update is called once per frame
  public void Kick(RaycastHit hit)
  {
    if (hit.rigidbody != null)
    {
      Vector3 direction = new Vector3(0, 1, 0);

      hit.rigidbody.AddForce(direction * hitForce, ForceMode.Impulse);
      Debug.Log("add force");
    }
  }
}
