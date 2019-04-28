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
    // teleporter
    private Teleporter teleporter;
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

            // teleporter
            if (teleporter != null && interact_circle_instance != null)
            {
                teleporter.Teleport(hit.point, interact_circle_instance);
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
}
