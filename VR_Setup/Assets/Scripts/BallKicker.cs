using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallKicker : MonoBehaviour
{

  public float hitForce;

  // Start is called before the first frame update
  void Start()
  {
      
  }

  // Update is called once per frame
  public void Kick(RaycastHit hit, Vector3 direction)
  {
    if (hit.rigidbody != null)
    {
      if (hit.transform.gameObject.tag == "BowlingBall"){
        hitForce *= 4;
      }
      direction +=  new Vector3(0, 0.5f, 0);
      hit.rigidbody.AddForce(direction * hitForce, ForceMode.Impulse);
    }
  }
}
