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
        
        // initialize variables
        this.health = initialHealth;
        this.energy = initialEnergy;
        this.reproductionWill = 0f;
        this.reproduce = false;
        this.dead = false;

        // set the creature mass to be the sum of its blocks mass
        rb.mass = transform.childCount * blockMass;

        // map the children of this objects as a list
        for (int i = 0; i < transform.childCount; i++) {
            this.blocks.Add(transform.GetChild(i).gameObject);
        }
    }

    void FixedUpdate() {
        ManageHealthEnergy();

        // if health is 0 kill this creature
        if (this.health <= 0)
            this.dead = true;

        // only birth one child at a time
        if (!this.reproduce)
            this.reproductionWill += matingRate;

        // if reproduction will is high enough, spawn a new child
        if (this.reproductionWill >= this.reproductionThreshold) {
            this.reproductionWill = 0f;

            // GameManager will take care of generating the new child
            this.reproduce = true;
        }
    }

    private void ManageHealthEnergy() {
        if (this.energy > 0) {

            // heal creature both from starvation and injuries
            this.health = Mathf.Min(initialHealth, this.health + healingSpeed);

            // the more blocks a creature has the faster it dies
            this.energy -= costOfLiving * this.blocks.Count;
        } else {
            this.energy = 0f;

            // decrease health if creature has no energy (starvation)
            this.health -= dyingSpeed;
        }
    }

    public void EatFood(GameObject food) {
        this.energy += food.transform.localScale.magnitude * foodValueMultiplier;
        Destroy(food);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Food") {
            EatFood(other.gameObject);
        }
    }
}
