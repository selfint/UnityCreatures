using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodScript : MonoBehaviour {

    public float CurrentStrength = 1f;
    public Rigidbody rb;
    public GameManagerScript gameManager;

    void FixedUpdate() {
        rb.AddForce(gameManager.GetFlowFieldVector(transform.position), ForceMode.Acceleration);
    }
}
