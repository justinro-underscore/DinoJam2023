using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggController : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private Transform initParent;

    protected void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        initParent = transform.parent;
    }

    protected void Update()
    {

        // TODO see if you can define 2 colliders - 1 for collisions and 1 for triggers
    }

    public void GrabEgg(Transform playerTransform)
    {
        transform.parent = playerTransform;
        transform.localEulerAngles = Vector3.zero;
        transform.position = transform.position + new Vector3(0, 0.5f);
        rb2d.simulated = false;
    }

    // Should only be called from the player controller
    public void DropEgg()
    {
        rb2d.simulated = true;
        transform.parent = initParent;
        transform.localEulerAngles = Vector3.zero;
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Nest"))
        {
            // TODO: Win
            Debug.Log("WIN!");
        }
    }
}
