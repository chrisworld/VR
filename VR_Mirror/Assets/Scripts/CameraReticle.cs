using UnityEngine;

public class CameraReticle : MonoBehaviour
{
    private Camera mainCamera;
    private RaycastHit hit;
    private float rayLength;
    private int mask;

    public float maxLength = 1000000.0f;
    public float reticleSize = 0.02f;
    public GameObject reticle;
    private GameObject reticle_instance;

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
    }

    
    // Update is called once per frame
    void LateUpdate()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, maxLength, mask))
        {
            rayLength = hit.distance;
            reticle_instance.transform.position = hit.point - reticleSize * mainCamera.transform.forward;
        }
        else
        {
            rayLength = maxLength;
            reticle_instance.transform.position = mainCamera.transform.position + rayLength * mainCamera.transform.forward;
        }

        float s = reticleSize * rayLength;
        reticle_instance.transform.localScale = new Vector3(s, s, s);
    }
}
