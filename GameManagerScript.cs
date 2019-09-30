using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour {

    public float timeScale = 1f;
    public int initialPopulationSize = 10;
    public int worldX, worldY, worldZ;
    public int foodDispenserAmount;
    public float flowFieldScale;
    public float flowFieldIncrement;
    public int maxFoods;
    public Terrain terrain;
    public GameObject creaturePrefab;
    public GameObject foodDispenserPrefab;
    public Transform creatures, foods, foodDispensers;
    private List<GameObject> population;
    private float xOffset, yOffset, zOffset;

    void Start() {
        SpawnInitialPopulation();
        SpawnFoodDispensers();
        this.xOffset = 0f;
        this.yOffset = worldX * worldY;
        this.zOffset = worldX * worldY * 2;
    }

    void FixedUpdate() {

        // TODO: implement actual terrain generation
        terrain.terrainData = GenerateTerrain(terrain.terrainData, 20f);

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
        while (foods.childCount > maxFoods) {
            Destroy(foods.GetChild(maxFoods));
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

    private TerrainData GenerateTerrain(TerrainData terrainData, float y) {
        terrainData.heightmapResolution = worldX + 1;
        terrainData.size = new Vector3(worldX, worldY, worldZ);
        terrainData.SetHeights(0, 0, GenerateHeights(y));
        return terrainData;
    }

    private float[,] GenerateHeights(float y) {
        float[,] heights = new float[worldX, worldZ];
        for (int i = 0; i < worldX; i++) {
            for (int j = 0; j < worldZ; j++) {
                Vector3 position = new Vector3(i, y, j);
                // heights[i, j] = GetFlowFieldVector(position).x + GetFlowFieldVector(position).z + 0.5f;
                heights[i, j] = 0f;
            }
        }
        return heights;
    }

    private void WrapObject(GameObject creature) {
        Vector3 newPosition = creature.transform.position;
        if (newPosition.x > worldX)
            newPosition.x = 0;
        if (newPosition.z > worldZ)
            newPosition.z = 0;
        if (newPosition.x < 0)
            newPosition.x = worldX;
        if (newPosition.z < 0)
            newPosition.z = worldZ;
        newPosition.y = Mathf.Min(worldY, newPosition.y);
        creature.transform.SetPositionAndRotation(newPosition, creature.transform.rotation);
    }

    void SpawnFoodDispensers() {
        for (int i = 0; i < foodDispenserAmount; i++) {
            Vector3 randomPosition = RandomSpawnLocation();
            randomPosition.y = 0;
            GameObject foodDispenser = Instantiate(foodDispenserPrefab, randomPosition, Quaternion.identity,
                                                   foodDispensers);
            foodDispenser.GetComponent<FoodDispenserScript>().gameManager = gameObject;
        }
    }

    void SpawnInitialPopulation() {
        this.population = new List<GameObject>();
        for (int i = 0; i < this.initialPopulationSize; i++) {
            SpawnCreature(RandomSpawnLocation());
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
        float y = Random.Range(0, worldY);
        float z = Random.Range(0, worldZ);
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
        float noiseX = Mathf.PerlinNoise((float)(position.x) / worldX * flowFieldScale,
                                         this.xOffset / worldX * flowFieldScale) - 0.5f;
        float noiseY = Mathf.PerlinNoise((float)(position.y) / worldY * flowFieldScale,
                                         this.yOffset / worldY * flowFieldScale) - 0.5f;
        float noiseZ = Mathf.PerlinNoise((float)(position.z) / worldZ * flowFieldScale,
                                         this.zOffset / worldZ * flowFieldScale) - 0.5f;
        return new Vector3(noiseX, noiseY, noiseZ);
    }

}
