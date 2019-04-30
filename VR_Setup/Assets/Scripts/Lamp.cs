using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{ 
  private Light light_instance;

  // Start is called before the first frame update
  void Start()
  {
    light_instance = gameObject.GetComponentsInChildren<Light>()[0];
  }

  // turn on / off light
  public void TurnOnOffLight()
  {
    if (light_instance.enabled)
    {
      light_instance.enabled = false;
    }
    else
    {
      light_instance.enabled = true;
    }
  }
}
