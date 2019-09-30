using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour {

    public float timeScale = 1f;
    public int initialPopulationSize = 10;
    public GameObject creaturePrefab;
    public GameObject foodDispenserPrefab;
    private List<GameObject> population;
    public Transform spawnLocation;
    public float spawnNoise = 10f;
    public int worldX, worldY, worldZ;
    private float xOffset, yOffset, zOffset;
    public int foodDispenserAmount;

    void Start() {
        spawnInitialPopulation();
        spawnFoodDispensers();
        this.xOffset = 0f;
        this.yOffset = worldX * worldY;
        this.zOffset = worldX * worldY * 2;
    }

    void spawnFoodDispensers() {
        for (int i = 0; i < foodDispenserAmount; i++) {
            Vector3 randomPosition = new Vector3(Random.Range(0, this.worldX), 0,
                                                 Random.Range(0, this.worldZ));
            GameObject foodDispenser = Instantiate(foodDispenserPrefab, randomPosition, Quaternion.identity);
            foodDispenser.GetComponent<FoodDispenserScript>().gameManager = gameObject;
        }
    }

    void spawnInitialPopulation() {
        this.population = new List<GameObject>();
        for (int i = 0; i < this.initialPopulationSize; i++) {
            spawnCreature(randomSpawnLocation(spawnLocation.position, spawnNoise));
        }
    }

    void spawnCreature(Vector3 location) {
        GameObject newCreature = Instantiate(creaturePrefab, location, Random.rotation);
        this.population.Add(newCreature);
    }

    void killCreature(GameObject creature) {
        this.population.Remove(creature);
        Destroy(creature);
    }

    Vector3 randomSpawnLocation(Vector3 center, float noise) {
        float x = center.x + Random.Range(-noise, noise);
        float y = center.y + Random.Range(0, noise / 10);
        float z = center.z + Random.Range(-noise, noise);
        return new Vector3(x, y, z);
    }

    void FixedUpdate() {

        // fixed update won't get called if timescale is set to 0
        if (timeScale != 0)
            Time.timeScale = timeScale;

        // iterate over all creatures
        foreach (GameObject creature in this.population) {
            applyFlowField(creature);
            CreatureScript creatureScript = creature.GetComponent<CreatureScript>();

            // kill dead creatrues
            if (creatureScript.dead)
                killCreature(creature);
        }

    }

    void applyFlowField(GameObject creature) {

        // add a flow field vector for each creature using perlin noise
        Vector3 creaturePos = creature.transform.position;
        creature.GetComponent<Rigidbody>().AddForce(calcFlowFieldVector(creaturePos), ForceMode.Acceleration);
    }

    public Vector3 calcFlowFieldVector(Vector3 position) {

        // offset the noise values of each axis so the vectors look more natural
        float noiseX = Mathf.PerlinNoise(position.x + position.z + this.xOffset, position.y + this.xOffset) - 0.5f;
        float noiseY = Mathf.PerlinNoise(position.x + position.z + this.yOffset, position.y + this.yOffset) - 0.5f;
        float noiseZ = Mathf.PerlinNoise(position.x + position.z + this.zOffset, position.y + this.zOffset) - 0.5f;
        return new Vector3(noiseX, noiseY, noiseZ);
    }

}
