using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public List<GameObject> blocks;
    public float initialHealth, initialEnergy;
    public float maxHealth, maxEnergy;
    public float health, energy;
    public bool dead;
    public float foodValueMultiplier;
    public float costOfLiving, dyingSpeed, healingSpeed, matingRate;
    public float blockMass;
    public float reproductionThreshold;
    public bool reproduce;
    [SerializeField]
    private float reproductionWill;

    void Start() {
        // set the creature mass to be the sum of its blocks mass
        this.health = initialHealth;
        this.energy = initialEnergy;
        this.reproductionWill = 0f;
        this.reproduce = false;
        this.dead = false;
        rb.mass = transform.childCount * blockMass;
        for (int i = 0; i < transform.childCount; i++) {
            this.blocks.Add(transform.GetChild(i).gameObject);
        }
    }

    void FixedUpdate() {
        if (this.energy > 0) {

            // heal creature both from starvation and injuries
            this.health = Mathf.Min(initialHealth, this.health + healingSpeed);

            // only birth one child at a time
            if (!this.reproduce)
                this.reproductionWill += matingRate;
            
            // the more blocks a creature has the faster it dies
            this.energy -= costOfLiving * this.blocks.Count;
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
