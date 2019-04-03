﻿using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HMDCameras : MonoBehaviour
{
    [Header("Debug")]
    public bool calibrate = true;
    public TextMesh messageText;

    [Header("Distortion")]
    public float K1 = 0;
    public float K2 = 0;
    public float oversize = 0;

    [Header("Built-in dimensions")]
    public const int textureWidth = 1920;
    public const int textureHeight = 1080;
    public const int gridX = 20;
    public const int gridY = 20;
    public const float znear = 0.01f; // We choose a very tiny znear. Should be smaller than typical eye relief (~18mm)
    public const float zfar = 1000.0f; 
    public const float orthographicSize = 0.05f; // simulated phone screen size in unity. Set to 5 cm to not interfere much

    //These should be set after the methods have been filled in.
    private float aspect;
    private float magnification;
    private float initialNear;
    private float leftEyeLeft, leftEyeRight;
    private float rightEyeLeft, rightEyeRight;
    private float top, bottom;

    private int gridLayer;
    private int numVertices;
    private int numTiles;
    private Mesh mesh;
    private Vector2 leftEyeCenter, rightEyeCenter;
    private Vector2[] uvs;
    private Vector3[] verticesLeft;
    private Vector3[] verticesRight;
    private Vector3[] gridVertices;
    private int[] triangles;

    private GameObject leftEye;
    private GameObject rightEye;
    private RenderTexture eyesTexture;
    private Matrix4x4 perspectiveLeft, perspectiveRight;

    private bool touching = false;
    private float touchStartTime = 0;
    private string[] modeDescription = { "++K1", "--K1", "++K2", "--K2", "++OS", "--OS" };
    private enum ParamMode { K1Up, K1Down, K2Up, K2Down, OversizeUp, OversizeDown };
    private ParamMode touchState = 0;

    // Start is called before the first frame update
    void Start()
    {
        gridLayer = 9;
        gameObject.layer = gridLayer;                          //Only draw this object's meshes for those who want it
        eyesTexture = new RenderTexture(textureWidth, textureHeight, 1, RenderTextureFormat.ARGB32);
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        GetComponent<MeshRenderer>().material.mainTexture = eyesTexture;

        numTiles = gridX * gridY;
        triangles = new int[numTiles * 6 * 2];
        numVertices = (gridX + 1) * (gridY + 1);
        uvs = new Vector2[2 * numVertices];
        verticesLeft = new Vector3[numVertices];
        verticesRight = new Vector3[numVertices];
        gridVertices = new Vector3[2 * numVertices];

        leftEye = new GameObject("Left eye");
        leftEye.transform.SetParent(gameObject.transform, false);
        leftEye.AddComponent<Camera>();

        rightEye = new GameObject("Right eye");
        rightEye.transform.SetParent(gameObject.transform, false);
        rightEye.AddComponent<Camera>();

        ComputeAspect(out aspect);
        ComputeLeftEyeOffset(out leftEyeCenter);
        ComputeRightEyeOffset(out rightEyeCenter);
        ComputeMagnification(out magnification);
        ComputeInitialNear(out initialNear);
        ComputeTopBottom(out top, out bottom);
        ComputeLeftRight(out leftEyeLeft, out leftEyeRight, out rightEyeLeft, out rightEyeRight);

        TransformCameras();
        CreateGrids();

        MorphGrid(verticesLeft, leftEyeCenter, 0, ref gridVertices);
        MorphGrid(verticesRight, rightEyeCenter, numVertices, ref gridVertices);

        ScaleForOrthography(ref gridVertices);
        mesh.vertices = gridVertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    /*#######################START OF ASSIGNMENT 1#######################*/
    // TODO: All of the below shall be filled in by the students to achieve 
    // a VR experience.
    /*###################################################################*/

    private void ComputeAspect(out float aspect)
    {
        aspect = 1.791f;
    }

    private void ComputeLeftEyeOffset(out Vector2 leftEyeOffset)
    {
        //leftEyeOffset = new Vector2(0.132f, 0);
        leftEyeOffset = new Vector2(0.032f, 0);
    }

    private void ComputeRightEyeOffset(out Vector2 rightEyeOffset)
    {
        //rightEyeOffset = new Vector2(0.132f, 0);
        rightEyeOffset = new Vector2(-0.032f, 0);
    }

    private void ComputeMagnification(out float magnification)
    {
        magnification = 3.846f;
    }

    private void ComputeInitialNear(out float initialNear)
    {
        initialNear = 0.055f;
    }

    private void ComputeLeftRight(out float leftEyeLeft, out float leftEyeRight, out float rightEyeLeft, out float rightEyeRight)
    {
        //Left eye
        leftEyeLeft = -0.01725f * 3.846f / 0.055f * znear;
        leftEyeRight = 0.032f * 3.846f / 0.055f * znear;

        //Right eye
        rightEyeLeft = -0.032f * 3.846f / 0.055f * znear;
        rightEyeRight = 0.01725f * 3.846f / 0.055f * znear;
    }

    private void ComputeTopBottom(out float top, out float bottom)
    {
        //top = 0.106f / 0.055f * znear;
        //bottom = -0.106f / 0.055f * znear;
        top = 1.923f * znear;
        bottom = -1.923f * znear;
    }

    private void MorphGrid(Vector3[] screenVertices, Vector2 center, int offset, ref Vector3[] outVertices)
    {
        for (int i = 0; i < screenVertices.Length; i++)
        {
            outVertices[i + offset] = screenVertices[i];
        }
    }

    /*************************END OF ASSIGNMENT 1*************************/
    // What follows are functions that are prepared by the tutors for 
    // rendering. It should not be necessary to change them. 
    /*********************************************************************/
    void Update()
    {
        if (calibrate)
        {
            int state = ((int)touchState);
            string desc = modeDescription[state];
            if(desc.EndsWith("K1"))
            {
                desc += " = " + K1.ToString("0.000");
            }
            if(desc.EndsWith("K2"))
            {
                desc += " = " + K2.ToString("0.000");
            }
            if(desc.EndsWith("OS"))
            {
                desc += " = " + oversize.ToString("0.000");
            }
            messageText.text = desc;

            // Handle screen touches. Should work in HMD with helper button.
            if (Input.touchCount > 0)
            {
                float time_spent = (Time.fixedTime - touchStartTime);
                if (touching && (time_spent > 0.4f))
                {
                    switch (touchState)
                    {
                        case ParamMode.K1Down:
                            K1 -= 0.001f;
                            break;
                        case ParamMode.K1Up:
                            K1 += 0.001f;
                            break;
                        case ParamMode.K2Down:
                            K2 -= 0.001f;
                            break;
                        case ParamMode.K2Up:
                            K2 += 0.001f;
                            break;
                        case ParamMode.OversizeDown:
                            oversize -= 0.001f;
                            break;
                        case ParamMode.OversizeUp:
                            oversize += 0.001f;
                            break;
                        default:
                            break;
                    }
                }

                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                {
                    touching = false;
                    if (time_spent < 0.4f)
                    {
                        int new_val = (state + 1) % Enum.GetValues(typeof(ParamMode)).Length;
                        touchState = (ParamMode)new_val;
                    }
                }
                else if (touch.phase == TouchPhase.Began)
                {
                    touching = true;
                    touchStartTime = Time.fixedTime;
                }
            }

            //Keyboard fallback. Use in Unity Editor or if keyboard is connected to phone.
            if (Input.GetKey(KeyCode.Y))
                K1 -= 0.001f;
            if (Input.GetKey(KeyCode.X))
                K1 += 0.001f;
            if (Input.GetKey(KeyCode.C))
                K2 -= 0.001f;
            if (Input.GetKey(KeyCode.V))
                K2 += 0.001f;
            if (Input.GetKey(KeyCode.B))
                oversize -= 0.001f;
            if (Input.GetKey(KeyCode.N))
                oversize += 0.001f;

            ProduceLeftRightVertices();
            TransformCameras();

            MorphGrid(verticesLeft, leftEyeCenter, 0, ref gridVertices);
            MorphGrid(verticesRight, rightEyeCenter, numVertices, ref gridVertices);

            ScaleForOrthography(ref gridVertices);
            mesh.vertices = gridVertices;
        }
        else if(messageText.text != "")
        {
            messageText.text = "";
        }
    }

    private void TransformCameras()
    {
        Camera ref_cam = GetComponent<Camera>();
        ref_cam.orthographic = true;
        ref_cam.orthographicSize = orthographicSize;
        ref_cam.cullingMask = (1 << gridLayer);
        ref_cam.clearFlags = CameraClearFlags.SolidColor;
        ref_cam.backgroundColor = new Color(0, 0, 0);
        ref_cam.nearClipPlane = znear;
        ref_cam.farClipPlane = 2.0f * znear;
        ref_cam.depth = 2;

        float dh = 0.5f * oversize * (leftEyeRight - leftEyeLeft);
        float dv = 0.5f * oversize * (top - bottom);

        perspectiveLeft = PerspectiveOffCenter(leftEyeLeft - dh, leftEyeRight + dh, bottom - dv, top + dv, znear, zfar);
        perspectiveRight = PerspectiveOffCenter(rightEyeLeft - dh, rightEyeRight + dh, bottom - dv, top + dv, znear, zfar);
        SetEyeCamera(ref leftEye, new Rect(0, 0, 0.5f, 1.0f), perspectiveLeft, leftEyeCenter, 0);
        SetEyeCamera(ref rightEye, new Rect(0.5f, 0, 0.5f, 1.0f), perspectiveRight, rightEyeCenter, 1);
    }

    //Code for off-axis projection matrix
    //Taken from https://docs.unity3d.com/ScriptReference/Camera-projectionMatrix.html
    private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 m = Matrix4x4.zero;
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        m[0, 0] = x;
        m[0, 2] = a;
        m[1, 1] = y;
        m[1, 2] = b;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 2] = e;
        return m;
    }

    private void SetEyeCamera(ref GameObject eye, Rect rect, Matrix4x4 perspective, Vector3 position, float depth)
    {
        Camera camera = eye.GetComponent<Camera>();
        camera.depth = depth;
        camera.targetTexture = eyesTexture;
        camera.projectionMatrix = perspective;
        camera.transform.localPosition = position;
        camera.rect = rect;
        camera.cullingMask = ~0 ^ (1 << gridLayer);
        camera.nearClipPlane = znear;
    }

    private void CreateGrids()
    {
        ProduceMeshUVs();
        ProduceLeftRightVertices();
        for (int g = 0; g < 2; g++)
        {
            for (int i = 0; i < gridY; i++)
            {
                for (int j = 0; j < gridX; j++)
                {
                    int tri = (i * gridX + j);
                    int off = (tri + g * numTiles) * 6;
                    int start = i * (gridX + 1) + j + g * numVertices;

                    triangles[off + 0] = start;
                    triangles[off + 1] = start + 1;
                    triangles[off + 2] = start + (gridX + 1);
                    triangles[off + 3] = start + (gridX + 1);
                    triangles[off + 4] = start + 1;
                    triangles[off + 5] = start + 1 + (gridX + 1);
                }
            }
        }
    }

    private void ScaleForOrthography(ref Vector3[] gridVertices)
    {
        for(int i = 0; i < gridVertices.Length; i++)
        {
            Vector3 temp = gridVertices[i];
            temp.x *= orthographicSize * aspect;
            temp.y *= orthographicSize;
            gridVertices[i] = temp;
        }
    }

    private void ProduceMeshUVs()
    {
        for (int i = 0; i <= gridY; i++)
        {
            float v = (gridY - i) / ((float)gridY);
            for (int j = 0; j <= gridX; j++)
            {
                int off = i * (gridX + 1) + j;
                float u = j / ((float)gridX);
                uvs[off] = new Vector2(u * 0.5f, v);
                uvs[off + numVertices] = new Vector2(u * 0.5f + 0.5f, v);
            }
        }
    }

    private void ProduceLeftRightVertices()
    {
        for (int i = 0; i <= gridY; i++)
        {
            float v = ((gridY - i) / ((float)gridY));
            for (int j = 0; j <= gridX; j++)
            {
                int off = i * (gridX + 1) + j;
                float u = (j / ((float)gridX));

                float oversizedUL = (u - 1.0f) * (1.0f + oversize) + 0.5f * oversize;
                float oversiuedUR = u * (1.0f + oversize) - 0.5f * oversize;
                float oversizedV = (2.0f * v - 1.0f) * (1.0f + oversize);
                float z = 1.5f * znear;

                verticesLeft[off] = new Vector3(oversizedUL, oversizedV, z);
                verticesRight[off] = new Vector3(oversiuedUR, oversizedV, z);
            }
        }
    }
}
