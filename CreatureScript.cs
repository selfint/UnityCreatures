using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public GameObject mouth;
    public GameObject womb;
    public GameObject[] blocks;
    public float initialHealth;
    public float initialEnergy;
    public float health, energy;
    public bool dead = false;
    public float foodValueMultiplier;
    public float costOfLiving;
    public float dyingSpeed;

    void Start() {
        // set the creature mass to be the sum of its blocks mass
        rb.mass = 20 + blocks.Length * 10;
        this.health = initialHealth;
        this.energy = initialEnergy;
    }

    void FixedUpdate() {
        // decrease energy as long as creature is alive
        this.energy = Mathf.Max(0, this.energy - costOfLiving);

        // if energy is 0 decrease the creature's health
        if (this.energy <= 0) {
            this.health -= dyingSpeed;
        }

        // if the creature has energy, regain health up to initial health
        else {
            this.health = Mathf.Min(initialHealth, this.health + 1);
        }

        // if health is 0 kill this creature
        if (this.health <= 0)
            this.dead = true;
    }

    public void EatFood(GameObject food) {
        this.energy += food.transform.localScale.x * foodValueMultiplier;
        Destroy(food);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Food") {
            EatFood(other.gameObject);
        }
    }

}
