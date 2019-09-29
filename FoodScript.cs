using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodScript : MonoBehaviour {

    public float CurrentStrength = 1f;
    public Rigidbody rb;

    void FixedUpdate() {
        rb.AddForce(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                    ForceMode.Acceleration);
    }
}
