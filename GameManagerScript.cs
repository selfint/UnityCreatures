using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour {

    public float timeScale = 1f;
    public int worldX, worldY, worldZ;
    public float worldNoiseGranuity;
    public int initialPopulationSize = 10;
    public int foodDispenserAmount;
    public float flowFieldGranuity;
    public float flowFieldIncrement;
    public float flowFieldStrength;
    public int maxFoods;
    public Terrain oceanFloor;
    public GameObject creaturePrefab;
    public GameObject foodDispenserPrefab;
    public Transform creatures, foods, foodDispensers;
    private List<GameObject> population;
    private float xOffset, yOffset, zOffset;

    void Start() {
        InitializeTerrain();
        SpawnInitialPopulation();
        SpawnFoodDispensers();
        this.xOffset = worldX * worldY;
        this.yOffset = worldX * worldY * 2;
        this.zOffset = worldX * worldY * 3;
    }

    void FixedUpdate() {

        // fixed update won't get called if timescale is set to 0
        if (timeScale != 0)
            Time.timeScale = timeScale;

        // update flow field to simulate changing currents
        UpdateFlowField();

        // iterate over all food particles
        for (int i = 0; i < foods.childCount; i++) {
            GameObject food = foods.GetChild(i).gameObject;
            ApplyFlowField(food);
            WrapObject(food);
        }

        // limit food amount
        for (int i = foods.childCount - 1; i >= maxFoods; i--) {
            Transform food = foods.GetChild(i);
            food.parent = null;
            Destroy(food.gameObject);
        }

        // iterate over all creatures
        foreach (GameObject creature in this.population) {
            ApplyFlowField(creature);
            CreatureScript creatureScript = creature.GetComponent<CreatureScript>();

            // kill dead creatures
            if (creatureScript.dead)
                KillCreature(creature);

            WrapObject(creature);
        }

    }

    private void InitializeTerrain() {
        oceanFloor.terrainData.heightmapResolution = worldX + 1;
        oceanFloor.terrainData.size = new Vector3(worldX, worldY, worldZ);
        oceanFloor.terrainData.SetHeights(0, 0, GenerateHeights());
    }

    private float[,] GenerateHeights() {
        float[,] heights = new float[worldX, worldZ];
        for (int i = 0; i < worldX; i++) {
            for (int j = 0; j < worldZ; j++) {
                float xCoord = (float)i / worldX * worldNoiseGranuity;
                float yCoord = (float)j / worldZ * worldNoiseGranuity;
                heights[i, j] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return heights;
    }

    private void WrapObject(GameObject creature) {
        Vector3 newPosition = creature.transform.position;
        if (newPosition.x > worldX) {
            float previousHeight = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.x = 0;
            newPosition.y = Terrain.activeTerrain.SampleHeight(new Vector3(newPosition.x, 0, newPosition.y)) + previousHeight;
        }
        if (newPosition.z > worldZ) {
            float previousHeight = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.z = 0;
            newPosition.y = Terrain.activeTerrain.SampleHeight(new Vector3(newPosition.x, 0, newPosition.y)) + previousHeight;
        }
        if (newPosition.x < 0) {
            float previousHeight = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.x = worldX;
            newPosition.y = Terrain.activeTerrain.SampleHeight(new Vector3(newPosition.x, 0, newPosition.y)) + previousHeight;
        }
        if (newPosition.z < 0) {
            float previousHeight = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.z = worldZ;
            newPosition.y = Terrain.activeTerrain.SampleHeight(new Vector3(newPosition.x, 0, newPosition.y)) + previousHeight;
        }
        newPosition.y = Mathf.Min(worldY, newPosition.y);
        creature.transform.SetPositionAndRotation(newPosition, creature.transform.rotation);
    }

    void SpawnFoodDispensers() {
        for (int i = 0; i < foodDispenserAmount; i++) {
            Vector3 randomPosition = RandomSpawnLocation();
            GameObject foodDispenser = Instantiate(foodDispenserPrefab, randomPosition, Quaternion.identity,
                                                   foodDispensers);
            foodDispenser.GetComponent<FoodDispenserScript>().gameManager = gameObject;
        }
    }

    void SpawnInitialPopulation() {
        this.population = new List<GameObject>();
        for (int i = 0; i < this.initialPopulationSize; i++) {
            Vector3 randomPosition = RandomSpawnLocation();
            randomPosition.y += 2f;
            SpawnCreature(randomPosition);
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

    Vector3 RandomSpawnLocation() {
        float x = Random.Range(0, worldX);
        float z = Random.Range(0, worldZ);
        float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
        return new Vector3(x, y, z);
    }

    private void UpdateFlowField() {
        this.xOffset += flowFieldIncrement;
        this.yOffset += flowFieldIncrement;
        this.zOffset += flowFieldIncrement;
    }

    void ApplyFlowField(GameObject creature) {

        // add a flow field vector for each creature using perlin noise
        Vector3 creaturePos = creature.transform.position;
        creature.GetComponent<Rigidbody>().AddForce(GetFlowFieldVector(creaturePos), ForceMode.Acceleration);
    }

    public Vector3 GetFlowFieldVector(Vector3 position) {

        // offset the noise values of each axis so the vectors look more natural
        float noiseX = Mathf.PerlinNoise((float)(position.x) / worldX * flowFieldGranuity,
                                         this.xOffset / worldX * flowFieldGranuity) - flowFieldGranuity / 2f;
        float noiseY = Mathf.PerlinNoise((float)(position.y) / worldY * flowFieldGranuity,
                                         this.yOffset / worldY * flowFieldGranuity) - flowFieldGranuity / 2f;
        float noiseZ = Mathf.PerlinNoise((float)(position.z) / worldZ * flowFieldGranuity,
                                         this.zOffset / worldZ * flowFieldGranuity) - flowFieldGranuity / 2f;
        return new Vector3(noiseX, noiseY, noiseZ) * flowFieldStrength;
    }

}
