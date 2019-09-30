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
    public float maxAltitude;
    public Transform creatures, foods, foodDispensers;

    void Start() {
        SpawnInitialPopulation();
        SpawnFoodDispensers();
        this.xOffset = 0f;
        this.yOffset = worldX * worldY;
        this.zOffset = worldX * worldY * 2;
    }

    void FixedUpdate() {

        // fixed update won't get called if timescale is set to 0
        if (timeScale != 0)
            Time.timeScale = timeScale;

        // update flow field to simulate changing currents
        UpdateFlowField();

        // apply flow field to food particles
        for (int i = 0; i < foods.childCount; i++) {
            Transform food = foods.GetChild(i);
            food.GetComponent<Rigidbody>().AddForce(GetFlowFieldVector(food.position), ForceMode.Acceleration);
        }
        
        // iterate over all creatures
        foreach (GameObject creature in this.population) {
            ApplyFlowField(creature);
            CreatureScript creatureScript = creature.GetComponent<CreatureScript>();

            // kill dead creatures
            if (creatureScript.dead)
                KillCreature(creature);

            // limit creature y value (top of the ocean)
            LimitCreatureAltitude(creature);
        }

    }

    private void LimitCreatureAltitude(GameObject creature) {
        Vector3 newPosition = creature.transform.position;
        newPosition.y = Mathf.Min(maxAltitude, newPosition.y);
        creature.transform.SetPositionAndRotation(newPosition, creature.transform.rotation);
    }

    void SpawnFoodDispensers() {
        for (int i = 0; i < foodDispenserAmount; i++) {
            Vector3 randomPosition = RandomSpawnLocation(spawnLocation.position, spawnNoise);
            randomPosition.y = 0;
            GameObject foodDispenser = Instantiate(foodDispenserPrefab, randomPosition, Quaternion.identity,
                                                   foodDispensers);
            foodDispenser.GetComponent<FoodDispenserScript>().gameManager = gameObject;
        }
    }

    void SpawnInitialPopulation() {
        this.population = new List<GameObject>();
        for (int i = 0; i < this.initialPopulationSize; i++) {
            SpawnCreature(RandomSpawnLocation(spawnLocation.position, spawnNoise));
        }
    }

    void SpawnCreature(Vector3 location) {
        GameObject newCreature = Instantiate(creaturePrefab, location, Random.rotation, creatures);
        this.population.Add(newCreature);
    }

    void KillCreature(GameObject creature) {
        this.population.Remove(creature);
        Destroy(creature);
    }

    Vector3 RandomSpawnLocation(Vector3 center, float noise) {
        float x = center.x + Random.Range(-noise, noise);
        float y = center.y;
        float z = center.z + Random.Range(-noise, noise);
        return new Vector3(x, y, z);
    }

    private void UpdateFlowField() {
        this.xOffset += 1f;
        this.yOffset += 1f;
        this.zOffset += 1f;
    }

    void ApplyFlowField(GameObject creature) {

        // add a flow field vector for each creature using perlin noise
        Vector3 creaturePos = creature.transform.position;
        creature.GetComponent<Rigidbody>().AddForce(GetFlowFieldVector(creaturePos), ForceMode.Acceleration);
    }

    public Vector3 GetFlowFieldVector(Vector3 position) {

        // offset the noise values of each axis so the vectors look more natural
        float noiseX = Mathf.PerlinNoise(position.x + position.z + this.xOffset, position.y + this.xOffset) - 0.5f;
        float noiseY = Mathf.PerlinNoise(position.x + position.z + this.yOffset, position.y + this.yOffset) - 0.5f;
        float noiseZ = Mathf.PerlinNoise(position.x + position.z + this.zOffset, position.y + this.zOffset) - 0.5f;
        return new Vector3(noiseX, noiseY, noiseZ);
    }

}
