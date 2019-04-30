using UnityEngine;
using UnityEditor;

public class CameraReticle : MonoBehaviour
{
    private Camera mainCamera;
    private RaycastHit hit;
    private float rayLength;
    private int mask;

    public float maxLength = 1000000.0f;
    public float reticleSize = 0.02f;
    public GameObject reticle;
    public GameObject interact_circle;

    private GameObject reticle_instance;
    private GameObject interact_circle_instance;

    // -- added stuff
    // gazing
    public float gaze_range = 0.2f;
    public int gaze_thresh = 5;
    public int look_frame_update = 3;
    public int gaze_frame_update = 45;

    private Vector3 old_pos;
    private int gaze_frame;
    private int look_frame;
    private bool gazing;

    // teleporter
    private Teleporter teleporter;
    private const int teleport_layer = 10;
    private const int light_layer = 11;

    // ball kicker
    private BallKicker ball_kicker;
    
    // for movement of head
    private Transform head_transform;

    // Start is called before the first frame update
    void Start()
    {
        rayLength = maxLength;
        mainCamera = GetComponent<Camera>();

        int grid_layer = 9;
        int ignore_layer = 2;

        mask = ~0 ^ (1 << grid_layer) ^ (1 << ignore_layer);
        mask |= (1 << teleport_layer) | (1 << light_layer);

        reticle_instance = Instantiate(reticle);
        reticle_instance.name = "Reticle";
        reticle_instance.layer = ignore_layer;

        // interact circle
        if (interact_circle != null)
        {
            interact_circle_instance = Instantiate(interact_circle);
        }
        if (interact_circle_instance != null)
        {
            interact_circle_instance.name = "Interact_Circle";
            interact_circle_instance.layer = ignore_layer;
            interact_circle.SetActive(false);
        }

        // gazing
        old_pos = Vector3.zero;
        gaze_frame = 0;
        look_frame = 0;
        gazing = false;

        // teleporter
        teleporter = (Teleporter)FindObjectOfType(typeof(Teleporter));

        // ball kicker
        ball_kicker = (BallKicker)FindObjectOfType(typeof(BallKicker));

        // head
        head_transform = GameObject.Find("Head").transform;
    }

    
    // Update is called once per frame
    void LateUpdate()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, maxLength, mask))
        {
            rayLength = hit.distance;
            reticle_instance.transform.position = hit.point - reticleSize * mainCamera.transform.forward;

            // interact circle
            if (interact_circle_instance != null)
            {
                interact_circle_instance.transform.position = hit.point - reticleSize * mainCamera.transform.forward;
                interact_circle_instance.transform.rotation = head_transform.rotation;
            }

            // draw teleport brezier
            //Vector3 start_tangent = mainCamera.transform.forward + new Vector3(0f, 0.1f, 0f);
            //Vector3 end_tangent = mainCamera.transform.forward + new Vector3(0f, -0.1f, 0f);
            //Handles.DrawBezier(mainCamera.transform.position, hit.point, start_tangent, end_tangent, Color.red, null, 2f);
            // draw a line
            //Debug.Log("hit: ");
            //Vector3 draw_start = mainCamera.transform.position + new Vector3(0f, 0f, 0f);
            //Debug.DrawLine(draw_start, hit.point);

            // gazer
            if (GazeInteract(hit.point, interact_circle_instance))
            {
                // teleporter
                if (hit.transform.gameObject.layer == teleport_layer)
                {
                    if (teleporter != null && interact_circle_instance != null)
                        teleporter.Teleport(hit.point, interact_circle_instance);
                }

                // light
                else if (hit.transform.gameObject.layer == light_layer)
                {
                    hit.transform.parent.GetComponent<Lamp>().TurnOnOffLight();
                }
            }

            // ball kicker
            if (ball_kicker != null)
            {
                ball_kicker.Kick(hit);
            }
        }

        else
        {
            rayLength = maxLength;
            reticle_instance.transform.position = mainCamera.transform.position + rayLength * mainCamera.transform.forward;

            if(interact_circle_instance != null)
            {
                interact_circle_instance.transform.position = mainCamera.transform.position + rayLength * mainCamera.transform.forward;
                interact_circle_instance.transform.rotation = head_transform.rotation;
            }
        }

        float s = reticleSize * rayLength;
        reticle_instance.transform.localScale = new Vector3(s, s, s);

        if (interact_circle_instance != null)
        {
            interact_circle_instance.transform.localScale = new Vector3(s, s, s);
        }
    }


    // Interact with gazing
    public bool GazeInteract(Vector3 new_pos, GameObject interact_circle)
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
            // do something with the gaze
            gazing = false;
            gaze_frame = 0;
            look_frame = 0;
            interact_circle.SetActive(false);
            return true;
          }
        }

        return false;
    }

    // range for gazing
    private bool InGazeRange(Vector3 new_pos)
    {
        Vector3 range_vector = new Vector3(Mathf.Abs(new_pos.x - old_pos.x),  Mathf.Abs(new_pos.y - old_pos.y), Mathf.Abs(new_pos.z - old_pos.z));

        if (range_vector.x < gaze_range  && range_vector.y < gaze_range && range_vector.z < gaze_range)
        {
          return true;
        }

        return false;
    } 


}
