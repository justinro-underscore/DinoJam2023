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
public class PlayerController : IManagedController
{
    [Header("References")]
    [SerializeField] private GripBarController gripBarController;
    [SerializeField] private LayerMask nestLayer;

    private class WingData
    {
        public Transform transform;
        public KeyCode key;
        public bool leftWing;
        public bool innerWing;
        public bool rotateClockwise;

        public float wingPosRotBound;
        public float wingNegRotBound;

        public bool lifting;
        public float lastRot;
        public float currRot;
        public float downTime;
    }
    [Header("Wings")]
    [SerializeField] private List<WingController> wingControllers;

    [Header("Wing Settings")]
    [SerializeField] [Range(0.0f, 60.0f)] private float innerWingUpRotationBound;
    [SerializeField] [Range(0.0f, 40.0f)] private float outerWingUpRotationBound;
    [SerializeField] [Range(0.0f, 50.0f)] private float innerWingDownRotationBound;
    [SerializeField] [Range(0.0f, 40.0f)] private float outerWingDownRotationBound;
    [SerializeField] [Range(10.0f, 500.0f)] private float wingUpSpeed = 10.0f;
    [SerializeField] [Range(10.0f, 500.0f)] private float wingDownSpeed = 10.0f;
    [SerializeField] [Range(0.1f, 4.0f)] private float innerWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 4.0f)] private float outerWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 3.0f)] private float lateralWingForce = 0.1f;
    [SerializeField] [Range(0.0f, 0.3f)] private float wingForceScalarTime; // Time it takes to get to 100% of wing force
    [SerializeField] [Range(1, 5)] private int wingForceScalarPower = 1;
    [SerializeField] [Range(0.01f, 0.33f)] private float innerWingDragForce = 0.01f;
    [SerializeField] [Range(0.01f, 0.33f)] private float outerWingDragForce = 0.01f;

    [Header("Grip Settings")]
    [SerializeField] [Range(0.0f, 1.0f)] private float initGripVal;
    [SerializeField] [Range(0.1f, 2.0f)] private float gripDrainScalar = 0.1f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripGainAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCollisionDrainAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCollisionSaveAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCrackThreshold = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCrackedWaitThreshold = 0.01f;
    [SerializeField] [Range(0.0f, 2.0f)] private float invulnerabilityInitTime;

    [Header("Player Settings")]
    [SerializeField] [Range(5.0f, 15.0f)] private float rotationScalar = 5.0f;
    [SerializeField] [Range(1.0f, 200.0f)] private float eggGrabForceScalar = 1.0f;
    [SerializeField] [Range(0.1f, 8.0f)] private float maxVelocityX = 0.1f;

    private Rigidbody2D rb2d;
    private List<PolygonCollider2D> eggColliders;

    private List<WingData> wingDatas;
    private bool canInput;
    private bool canGrip;

    private EggController eggController;
    private bool gripping;
    private float gripVal;
    private bool invulnerable;
    private float invulnerabilityTime;
    private bool gripCracked;

    private float initGrav;

    override protected void ManagedStart()
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

                wingPosRotBound = wingControllers[i].rotateClockwise ?
                    (wingControllers[i].innerWing ? innerWingDownRotationBound : outerWingDownRotationBound) :
                    (wingControllers[i].innerWing ? innerWingUpRotationBound : outerWingUpRotationBound),
                wingNegRotBound = wingControllers[i].rotateClockwise ?
                    (wingControllers[i].innerWing ? -innerWingUpRotationBound : -outerWingUpRotationBound) :
                    (wingControllers[i].innerWing ? -innerWingDownRotationBound : -outerWingDownRotationBound),

                lifting = false,
                lastRot = wingControllers[i].wingTransform.localEulerAngles.z
            });
        }

        initGrav = rb2d.gravityScale;
        if ((innerWingDragForce * 2) + (outerWingDragForce * 2) > initGrav)
        {
            Debug.LogError("Drag forces too high!!!");
        }

        canInput = true;
        canGrip = true;
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        rb2d.simulated = newState != PlayState.PAUSE;
        switch (newState)
        {
            case PlayState.WIN:
            {
                canGrip = false;
                if (gripping) DropEgg();
                break;
            }
            case PlayState.LOSE:
            {
                canInput = false;
                if (gripping) DropEgg();
                break;
            }
        }
    }

    override public void ManagedUpdate()
    {
        CheckForWingInput();
        if (canGrip) HandleGrip();

        foreach (WingData wingData in wingDatas)
        {
            UpdateWingRot(wingData);
            CheckApplyForce(wingData);
        }
        CheckForWingDrag();

        transform.localEulerAngles = new Vector3(0, 0, rb2d.velocity.x * -rotationScalar);
    }

    private bool GetPlayerInputKey(KeyCode key, bool keyDown=true)
    {
        if (!canInput) return false;
        return keyDown ? Input.GetKeyDown(key) : Input.GetKey(key);
    }

    private void CheckForWingInput()
    {
        for (int i = 0; i < wingDatas.Count; i++)
        {
            WingData wingData = wingDatas[i];
            if (!wingData.lifting && GetPlayerInputKey(wingData.key, false))
            {
                wingData.lifting = true;
            }
            else if (wingData.lifting && !GetPlayerInputKey(wingData.key, false))
            {
                wingData.lifting = false;
            }
        }
    }

    private void HandleGrip()
    {
        if (invulnerabilityTime > 0)
        {
            invulnerabilityTime -= Time.deltaTime;
            if (invulnerabilityTime <= 0)
            {
                invulnerable = false;
            }
        }

        if (gripping)
        {
            if (GetPlayerInputKey(KeyCode.LeftControl) || GetPlayerInputKey(KeyCode.RightControl))
            {
                DropEgg();
                return;
            }

            gripVal -= Time.deltaTime * gripDrainScalar;
            if (gripCracked)
            {
                if (gripVal < gripCrackedWaitThreshold) gripCracked = false;
            }
            else if (GetPlayerInputKey(KeyCode.Space))
            {
                // If the player tries gripping harder while they're past the gripCrackThreshold
                //  and the amount added will hit the top of the grip bar, take egg damage
                if (gripVal >= gripCrackThreshold && (gripVal + gripGainAmount) >= 1)
                {
                    PlayController.Instance.TakeEggDamage();
                    // Make sure they don't do it again by forcing the player to wait until they grip again
                    gripCracked = true;
                }
                gripVal += gripGainAmount;
                if (gripVal > 1) gripVal = 1;
            }
            gripBarController.SetGripPercentage(gripVal);

            if (gripVal <= 0)
            {
                DropEgg();
            }
        }
        else if (GetPlayerInputKey(KeyCode.Space))
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
        newRot = Mathf.Clamp(newRot, wingData.wingNegRotBound, wingData.wingPosRotBound);
        wingData.currRot = newRot * (wingData.rotateClockwise ? 1 : -1);
        wingData.transform.localEulerAngles = new Vector3(0, 0, newRot);

        if (wingData.currRot - wingData.lastRot > 0)
            wingData.downTime += Time.deltaTime;
        else if (wingData.downTime > 0)
            wingData.downTime = 0;
    }

    private void CheckApplyForce(WingData wingData)
    {
        float rotDiff = wingData.currRot - wingData.lastRot;
        if (rotDiff > 0)
        {
            float forceScalar = Mathf.Min(Mathf.Pow(wingData.downTime / wingForceScalarTime, wingForceScalarPower), 1);
            Vector2 wingForceVec = new Vector2(lateralWingForce * (wingData.leftWing ? 1 : -1), 1) * rotDiff * (wingData.innerWing ? innerWingForce : outerWingForce) * forceScalar;
            rb2d.AddForce(wingForceVec);
            if (Mathf.Abs(rb2d.velocity.x) > maxVelocityX) rb2d.velocity = new Vector2(rb2d.velocity.x > 0 ? maxVelocityX : -maxVelocityX, rb2d.velocity.y);
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
        Physics2D.IgnoreLayerCollision(gameObject.layer, Mathf.FloorToInt(Mathf.Log(nestLayer.value, 2)));

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
        SetInvulnerable();

        gripBarController.Enter();
    }

    private void DropEgg()
    {
        if (gripping && eggController)
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
            Physics2D.IgnoreLayerCollision(gameObject.layer, Mathf.FloorToInt(Mathf.Log(nestLayer.value, 2)), false);

            gripping = false;
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if (other.gameObject.CompareTag("Egg"))
        {
            eggController = other.transform.parent.gameObject.GetComponent<EggController>();
            eggController.SetEggOutlineVisible(true);
        }
        else if (other.gameObject.CompareTag("Nest") && gripping && eggController)
        {
            // TODO Maybe when in nestshow sprite mask so wings don't clip out of nest?
            PlayController.Instance.WinLevel();
        }

        // TODO: ideally we handle this better with a script we can attach to any game object separate from main logic
        // TODO: something like a damage emitter and damage accumulator script or whatever and that can handle checking and storing
        // TODO: damage on its own
        if (other.gameObject.CompareTag(Constants.damageEmitterTag))
        {
            // TODO: temporary code
            Debug.Log("HIT");
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
    }
    
    protected void OnTriggerExit2D(Collider2D other)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if (other.gameObject.CompareTag("Egg") && !gripping)
        {
            eggController.SetEggOutlineVisible(false);
            eggController = null;
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if (collision.gameObject.CompareTag("Walls") && gripping && !invulnerable)
        {
            float drainAmount = gripCollisionDrainAmount;
            if (gripVal > gripCollisionSaveAmount)
            {
                drainAmount = Mathf.Min(gripCollisionDrainAmount, gripVal - gripCollisionSaveAmount);
            }
            gripVal -= drainAmount;
            gripBarController.SetGripPercentage(gripVal);
            gripBarController.OnCollision();
            SetInvulnerable();
        }
    }

    private void SetInvulnerable()
    {
        invulnerabilityTime = invulnerabilityInitTime;
        invulnerable = true;
    }
}
