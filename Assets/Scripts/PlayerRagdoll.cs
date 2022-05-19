using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    public bool isRagdoll;
    //:Components
    PlayerMovement pm;
    [SerializeField]
    Animator animator;
    [SerializeField]
    GameObject[] ragdollBones;
    private List<Rigidbody> rigidbodies;
    private List<Collider> colliders;
    //:Physics
    [SerializeField]
    float[] forceMultiplier;

    private void Start()
    {
        isRagdoll = false;
        pm = GetComponent<PlayerMovement>();
        rigidbodies = new List<Rigidbody>();
        colliders = new List<Collider>();
        foreach (GameObject bone in ragdollBones)
        {
            rigidbodies.Add(bone.GetComponent<Rigidbody>());
            colliders.Add(bone.GetComponent<Collider>());
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                rigidbody.useGravity = false;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RagdollToggle();
        }

    }

    public void RagdollToggle()
    {
        isRagdoll = !isRagdoll;
        //pm.trickToggle();
        foreach (Collider collider in colliders)
        {
            collider.enabled = !collider.enabled;
        }
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.useGravity = !rigidbody.useGravity;
            if (rigidbody.useGravity == true)
            {
                //rigidbody.AddForce(-transform.right * pm.VelocityZ * forceMultiplier[0]);
                //rigidbody.AddForce(Vector3.down * pm.VelocityZ * forceMultiplier[1]);
            }
        }
    }
}
