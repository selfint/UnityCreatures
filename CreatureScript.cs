using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public List<GameObject> blocks;
    public float initialHealth;
    public float initialEnergy;
    public float health, energy;
    public bool dead = false;
    public float foodValueMultiplier;
    public float costOfLiving;
    public float dyingSpeed;
    private float reproductionWill;
    public float reproductionThreshold;
    public bool reproduce;

    void Start() {
        // set the creature mass to be the sum of its blocks mass
        this.health = initialHealth;
        this.energy = initialEnergy;
        this.reproductionWill = 0f;
        this.reproduce = false;
        rb.mass = 20 + this.blocks.Count * 10;    
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
            if (!this.reproduce)
                this.reproductionWill += 1f;
        }

        // if health is 0 kill this creature
        if (this.health <= 0)
            this.dead = true;

        // if reproduction will is high enough, spawn a new child
        if (this.reproductionWill >= this.reproductionThreshold) {
            this.reproductionWill = 0f;

            // GameManager will take care of generating the new child
            this.reproduce = false;
        }

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
