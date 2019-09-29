using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public GameObject mouth;
    public GameObject womb;
    public GameObject[] blocks;

    void Start() {

        // set the creature mass to be the sum of its blocks mass
        rb.mass = 20 + blocks.Length * 10;
    }

    void FixedUpdate() {
    
    }

    public void EatFood(GameObject food) {
        Destroy(food);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Food") {
            EatFood(other.gameObject);
        }
    }

}
