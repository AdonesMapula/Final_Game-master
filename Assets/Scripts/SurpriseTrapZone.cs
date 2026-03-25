using UnityEngine;

public class SurpriseTrapZone : MonoBehaviour
{
    public GameObject trapObject;

    private SurpriseTrap trap;

    void Start()
    {
        if (trapObject != null)
        {
            trap = trapObject.GetComponent<SurpriseTrap>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && trap != null)
        {
            trap.TriggerTrap();
        }
    }
}