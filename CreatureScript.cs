using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public List<GameObject> blocks;
    public float initialHealth, initialEnergy;
    public float maxHealth, maxEnergy;
    public float health, energy;
    public bool dead = false;
    public float foodValueMultiplier;
    public float costOfLiving, dyingSpeed, healingSpeed, matingRate;
    [SerializeField]
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

        if (this.energy > 0) {
            this.energy -= costOfLiving;
            this.health = Mathf.Min(initialHealth, this.health + healingSpeed);
            if (!this.reproduce)
                this.reproductionWill += matingRate;
        }

        // decrease health if energy is 0
        else {
            this.energy = 0f;
            this.health -= dyingSpeed;
        }

        // if health is 0 kill this creature
        if (this.health <= 0)
            this.dead = true;

        // if reproduction will is high enough, spawn a new child
        if (this.reproductionWill >= this.reproductionThreshold) {
            this.reproductionWill = 0f;

            // GameManager will take care of generating the new child
            this.reproduce = true;
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
