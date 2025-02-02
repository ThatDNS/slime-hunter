using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// DamageDealer needs a trigger collider and rigidbody to be able to call OnTriggerEnter.
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class DamageDealer : MonoBehaviour
{
    public LayerMask hitLayers;
    public Damage damage;

    [Tooltip("Invoked with layer of the object hit.")]
    public UnityEvent<int> OnSuccessfulHit;

    // Active damage dealer deals damage. Inactive does not.
    protected bool active = false;
    // attackDetected can be used by child classes to do something on attack
    protected bool attackDetected = false;

    [Header("Camera Shake")]
    [SerializeField] float cameraShakeIntensity = 1.0f;
    [SerializeField] float cameraShakeTime = 0.5f;
    protected bool applyCameraShake = false;

    public bool Active { 
        get { 
            return active;
        } 
        set {
            GetComponent<Collider>().enabled = value;
            active = value;
        }
    }

    protected virtual void Start()
    {
        // Ensure the rigid body is kinematic
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    protected virtual void Update()
    {
        if (!active)
            attackDetected = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if ((hitLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            damage.direction = (other.transform.position - transform.position).normalized;
            ITakeDamage damageReceiver = other.gameObject.GetComponent<ITakeDamage>();
            bool? damaged = damageReceiver?.TakeDamage(damage, true);

            if (damaged != null && (bool)damaged)
            {
                attackDetected = true;
                OnSuccessfulHit.Invoke(other.gameObject.layer);

                if (applyCameraShake)
                {
                    CameraManager.Instance.ShakeCamera(cameraShakeIntensity, cameraShakeTime);
                }
            }
        }
    }
}
