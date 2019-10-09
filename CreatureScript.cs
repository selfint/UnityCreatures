using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureScript : MonoBehaviour {

    public Rigidbody rb;
    public List<GameObject> blocks;
    [System.NonSerialized]
    public CreatureBrainScript brain;
    public GameObject[] inputBlocks, outputBlocks;
    public int passiveInputAmount;
    public float initialHealth, initialEnergy;
    public float maxHealth, maxEnergy;
    public float health, energy;
    public bool dead;
    public float foodValueMultiplier;
    public float costOfLiving, dyingSpeed, healingSpeed;
    public float blockMass;
    public bool reproduce;

    void Start() {

        // initialize variables
        this.health = initialHealth;
        this.energy = initialEnergy;
        this.reproduce = false;
        this.dead = false;

        // set the creature mass to be the sum of its blocks mass
        rb.mass = transform.childCount * blockMass;
        UpdateBody();
    }

    public void UpdateBody() {
        // map the children of this objects as a list
        // find how many inputs (sensors) and outputs (actions) the creature has
        List<GameObject> inputs = new List<GameObject>(), outputs = new List<GameObject>();
        this.blocks = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++) {
            GameObject block = transform.GetChild(i).gameObject;
            switch (block.tag) {
                case "ScentSensor":
                    inputs.Add(block);
                    break;
                case "Womb":
                    outputs.Add(block);
                    break;
                case "Motor":
                    outputs.Add(block);
                    break;
            }
            this.blocks.Add(block);
        }
        this.passiveInputAmount = 2;
        this.inputBlocks = inputs.ToArray();
        this.outputBlocks = outputs.ToArray();
        this.brain = new CreatureBrainScript(inputBlocks.Length + passiveInputAmount, outputBlocks.Length);
    }

    void FixedUpdate() {
        ManagePassiveStats();
        ManageBlocks();
    }

    public void ManageBlocks() {
        List<float> inputs = GetInputs();
        PerformActions(inputs);
    }

    private List<float> GetInputs() {
        List<float> inputs = new List<float>();
        inputs.AddRange(GetActiveInputs());
        inputs.AddRange(GetPassiveInputs());
        return inputs;
    }

    private List<float> GetPassiveInputs() {
        List<float> inputs = new List<float> {
            this.health / maxHealth,
            this.energy / maxEnergy
        };
        return inputs;
    }

    private List<float> GetActiveInputs() {
        List<float> inputs = new List<float>();
        for (int i = 0; i < inputBlocks.Length; i++) {
            switch (this.inputBlocks[i].tag) {
                case "ScentSensor":
                    inputs.Add(GetScentSensorInput(this.inputBlocks[i]));
                    break;
            }
        }

        return inputs;
    }

    private void PerformActions(List<float> inputs) {
        float[] outputs = this.brain.FeedForward(inputs);
        for (int i = 0; i < outputs.Length; i++) {
            switch (this.outputBlocks[i].tag) {
                case "Motor":
                    ActivateMotor(this.outputBlocks[i], outputs[i]);
                    break;
                case "Womb":
                    ActivateWomb(outputs[i]);
                    break;
            }
        }
    }

    public void ActivateMotor(GameObject motor, float input) {
        float motorStrength = 1f;
        float motorForce = input * motorStrength;
        float motorCost = motorForce * 10f;
        if (input > 0.5f && this.energy >= motorCost) {
            rb.AddForce(motor.transform.forward * Time.deltaTime * motorForce, ForceMode.Force);
            this.energy -= motorCost;
        }
    }

    private void ActivateWomb(float input) {
        float wombCost = 100f;
        if (input > 0.5f && this.energy >= wombCost && !this.reproduce) {

            // GameManager will set this to false when it creates the child
            this.reproduce = true;
            this.energy -= wombCost;
        }
    }

    public float GetScentSensorInput(GameObject scentSensor) {
        float scentSensorRange = 100f;
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
            this.health = Mathf.Min(maxHealth, this.health + healingSpeed);

            // the more blocks a creature has the faster it dies
            this.energy -= costOfLiving * this.blocks.Count;
        } else {
            this.energy = 0f;
            this.reproduce = false;

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
