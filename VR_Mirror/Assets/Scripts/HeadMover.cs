using UnityEngine;

public class HeadMover : MonoBehaviour
{
    public bool useGyro = true;
    public Vector3 poseCorrection = new Vector3(90, 0, 0);
    private bool hasGyro = false;
    private Quaternion baseRotation;

    public float moveSpeed = 2.0f;
    public float turnSpeed = 40.0f;
    public float fixedTurnY = 0.0f;
    public float fixedTurnX = 0.0f;

    // Use this for initialization
    void Start()
    {
        baseRotation = transform.localRotation;
        if (useGyro && SystemInfo.supportsGyroscope)
        {
            hasGyro = true;
            Input.gyro.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = (new Vector3(transform.forward.x, 0, transform.forward.z)).normalized;
        Vector3 right = (new Vector3(transform.right.x, 0, transform.right.z)).normalized;

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += moveSpeed * forward * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= moveSpeed * forward * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += moveSpeed * right * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= moveSpeed * right * Time.deltaTime;
        }

        float keyTurn = 0.0f;
        if (Input.GetKey(KeyCode.E))
        {
            keyTurn = turnSpeed;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            keyTurn = -turnSpeed;
        }
        baseRotation *= Quaternion.Euler(fixedTurnX * Time.deltaTime, (keyTurn + fixedTurnY) * Time.deltaTime, 0);

        Quaternion addedRotation = Quaternion.identity;
        if (hasGyro)
        {
            Quaternion q = Input.gyro.attitude;
            Quaternion leftHanded = new Quaternion(q.x, q.y, -q.z, -q.w);
            Quaternion phoneCorrection = Quaternion.Euler(poseCorrection);
            addedRotation = phoneCorrection * leftHanded;

            Vector3 eulers = addedRotation.eulerAngles;
            eulers.y *= 2.0f;

            addedRotation = Quaternion.Euler(eulers);
        }

        transform.localRotation = baseRotation * addedRotation;
    }
}
