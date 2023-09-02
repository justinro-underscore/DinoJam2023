using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WingController
{
    public Transform wingTransform;
    public bool leftWing;
    public bool innerWing;
    public bool rotateClockwise;
    public KeyCode key;
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private class WingData
    {
        public Transform transform;
        public KeyCode key;
        public bool leftWing;
        public bool innerWing;
        public bool rotateClockwise;

        public bool lifting;
        public float lastRot;
        public float currRot;
    }

    [Header("Wings")]
    [SerializeField] private List<WingController> wingControllers;

    [Header("Wing Settings")]
    [SerializeField] [Range(0.0f, 50.0f)] private float wingUpRotationBound;
    [SerializeField] [Range(0.0f, 50.0f)] private float wingDownRotationBound;
    [SerializeField] [Range(10.0f, 400.0f)] private float wingUpSpeed = 10.0f;
    [SerializeField] [Range(10.0f, 400.0f)] private float wingDownSpeed = 10.0f;
    [SerializeField] [Range(0.1f, 2.0f)] private float innerWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float outerWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float lateralWingForce = 0.1f;

    [Header("Player Settings")]
    [SerializeField] [Range(5.0f, 15.0f)] private float rotationScalar = 5.0f;

    private Rigidbody2D rb2d;

    private List<WingData> wingDatas;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        wingDatas = new List<WingData>(wingControllers.Count);
        for (int i = 0; i < wingControllers.Count; i++)
        {
            wingDatas.Add(new WingData
            {
                transform = wingControllers[i].wingTransform,
                key = wingControllers[i].key,
                leftWing = wingControllers[i].leftWing,
                innerWing = wingControllers[i].innerWing,
                rotateClockwise = wingControllers[i].rotateClockwise,

                lifting = false,
                lastRot = wingControllers[i].wingTransform.localEulerAngles.z
            });
        }
    }

    protected void Update()
    {
        CheckForInput();

        foreach (WingData wingData in wingDatas)
        {
            UpdateWingRot(wingData);
            CheckApplyForce(wingData);
        }

        transform.localEulerAngles = new Vector3(0, 0, rb2d.velocity.x * -rotationScalar);
    }

    private void CheckForInput()
    {
        for (int i = 0; i < wingDatas.Count; i++)
        {
            WingData wingData = wingDatas[i];
            if (!wingData.lifting && Input.GetKey(wingData.key))
            {
                wingData.lifting = true;
            }
            else if (wingData.lifting && !Input.GetKey(wingData.key))
            {
                wingData.lifting = false;
            }
        }
    }

    private void UpdateWingRot(WingData wingData)
    {
        float rot = wingData.transform.localEulerAngles.z;
        if (rot >= 180) rot -= 360;
        wingData.lastRot = rot * (wingData.rotateClockwise ? 1 : -1);
        float newRot = rot + ((wingData.lifting ? -wingUpSpeed : wingDownSpeed) * Time.deltaTime * (wingData.rotateClockwise ? 1 : -1));
        float wingPosBound = wingData.rotateClockwise ? wingDownRotationBound : wingUpRotationBound;
        float wingNegBound = wingData.rotateClockwise ? -wingUpRotationBound : -wingDownRotationBound;
        newRot = Mathf.Clamp(newRot, wingNegBound, wingPosBound);
        wingData.currRot = newRot * (wingData.rotateClockwise ? 1 : -1);
        wingData.transform.localEulerAngles = new Vector3(0, 0, newRot);
    }

    private void CheckApplyForce(WingData wingData)
    {
        float rotDiff = wingData.currRot - wingData.lastRot;
        if (rotDiff > 0)
        {
            Vector2 wingForceVec = new Vector2(lateralWingForce * (wingData.leftWing ? 1 : -1), 1) * rotDiff * (wingData.innerWing ? innerWingForce : outerWingForce);
            rb2d.AddForce(wingForceVec);
        }
    }
}
