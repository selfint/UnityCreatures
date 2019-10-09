using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public List<GameObject> blocks;
    [System.NonSerialized]
    public CreatureBrainScript brain;
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
        // find how many inputs (sensors) and outputs (actions) the creature has
        int inputs = 0, outputs = 0;
        for (int i = 0; i < transform.childCount; i++) {
            GameObject block = transform.GetChild(i).gameObject;
            if (block.tag == "ScentSensor") {
                inputs += 1;
            } else if (block.tag == "Motor" || block.tag == "Womb") {
                outputs += 1;
            }
            this.blocks.Add(block);
        }

        this.brain = new CreatureBrainScript(inputs, outputs);
    }

    void FixedUpdate() {
        ManagePassiveStats();
        ManageReproduction();
        ManageBlocks();
    }

    private void ManageReproduction() {
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

    public void ManageBlocks() {
        foreach (GameObject block in this.blocks) {
            if (block.tag == "ScentSensor") {
                ManageScentSensor(block, 100);
            }
        }
    }

    public float ManageScentSensor(GameObject scentSensor, float scentSensorRange) {
        // get the distance to the nearest food particle
        float minDistance = scentSensorRange;
        foreach (GameObject food in GameObject.FindGameObjectsWithTag("Food")) {
            float distance = Vector3.Distance(food.transform.position, scentSensor.transform.position);
            if (distance < scentSensorRange) {
                minDistance = Mathf.Min(minDistance, distance);
            }
        }

        // return normalized distance
        return minDistance / scentSensorRange;
    }

    private void ManagePassiveStats() {
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

        // if health is 0 kill this creature
        if (this.health <= 0)
            this.dead = true;
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
