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
    [Header("References")]
    [SerializeField] private GripBarController gripBarController;

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
    [SerializeField] [Range(0.01f, 0.125f)] private float innerWingDragForce = 0.01f;
    [SerializeField] [Range(0.01f, 0.125f)] private float outerWingDragForce = 0.01f;

    [Header("Grip Settings")]
    [SerializeField] [Range(0.0f, 1.0f)] private float initGripVal;
    [SerializeField] [Range(0.1f, 2.0f)] private float gripDrainScalar = 0.1f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripGainAmount = 0.01f;

    [Header("Player Settings")]
    [SerializeField] [Range(5.0f, 15.0f)] private float rotationScalar = 5.0f;
    [SerializeField] [Range(1.0f, 200.0f)] private float eggGrabForceScalar = 1.0f;

    private Rigidbody2D rb2d;
    private List<PolygonCollider2D> eggColliders;

    private List<WingData> wingDatas;

    private EggController eggController;
    private bool gripping;
    private float gripVal;

    private float initGrav;

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

        initGrav = rb2d.gravityScale;
        if ((innerWingDragForce * 2) + (outerWingDragForce * 2) > initGrav)
        {
            Debug.LogError("Drag forces too high!!!");
        }
    }

    protected void Update()
    {
        CheckForWingInput();
        HandleGrip();

        foreach (WingData wingData in wingDatas)
        {
            UpdateWingRot(wingData);
            CheckApplyForce(wingData);
        }
        CheckForWingDrag();

        transform.localEulerAngles = new Vector3(0, 0, rb2d.velocity.x * -rotationScalar);
    }

    private void CheckForWingInput()
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

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (eggController)
        //     {
        //         holdingEgg = !holdingEgg;
        //         if (holdingEgg)
        //         {
        //             holdingEgg = GrabEgg();
        //         }
        //         else
        //         {
        //             DropEgg();
        //         }
        //     }
        // }
    }

    private void HandleGrip()
    {
        if (gripping)
        {
            gripVal -= Time.deltaTime * gripDrainScalar;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gripVal += gripGainAmount;
                if (gripVal > 1) gripVal = 1;
            }
            gripBarController.SetGripPercentage(gripVal);

            if (gripVal <= 0)
            {
                DropEgg();
                gripping = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            GrabEgg(); // Sets gripping value in here
            if (gripping)
            {
                gripVal = initGripVal;
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

    private void CheckForWingDrag()
    {
        float gravOffset = 0;
        foreach (WingData wingData in wingDatas)
        {
            if (wingData.currRot < 0)
            {
                gravOffset += wingData.innerWing ? innerWingDragForce : outerWingDragForce;
            }
        }
        rb2d.gravityScale = initGrav - gravOffset;
    }

    private void GrabEgg()
    {
        if (!eggController) return;
        gripping = true;

        eggController.GrabEgg(transform);
        transform.localEulerAngles = new Vector3(0, 0, 0);
        transform.position = eggController.transform.position;
        eggController.transform.localPosition = new Vector2(0, -0.5f);
        rb2d.velocity = Vector2.zero;

        List<PolygonCollider2D> colliders = eggController.GetEggColliders();
        eggColliders = new List<PolygonCollider2D>();
        foreach (PolygonCollider2D eggCollider in colliders)
        {
            PolygonCollider2D newCollider = gameObject.AddComponent<PolygonCollider2D>().GetCopyOf(eggCollider);
            newCollider.isTrigger = eggCollider.isTrigger;
            newCollider.offset = new Vector2(0, newCollider.offset.y - 0.5f);
            eggColliders.Add(newCollider);
        }

        rb2d.AddForce(Vector2.up * eggGrabForceScalar);

        gripBarController.Enter();
    }

    private void DropEgg()
    {
        if (eggController)
        {
            eggController.DropEgg();
            foreach (PolygonCollider2D collider in eggColliders)
            {
                Destroy(collider);
            }
            eggColliders = null;
            eggController.SetEggOutlineVisible(false);
            eggController = null;
            gripBarController.Exit();
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Egg"))
        {
            eggController = other.transform.parent.gameObject.GetComponent<EggController>();
            eggController.SetEggOutlineVisible(true);
        }
        else if (other.gameObject.CompareTag("Nest") && gripping && eggController)
        {
            // TODO: Win
            Debug.Log("WIN!");
        }
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Egg") && !gripping)
        {
            eggController.SetEggOutlineVisible(false);
            eggController = null;
        }
    }
}
