using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour {

    public float timeScale;
    public float addBlockMutationRate;
    public int worldX, worldY, worldZ;
    public float worldNoiseGranuity;
    public int initialPopulationSize;
    public float birthCost;
    public int foodDispenserAmount;
    public float flowFieldGranuity;
    public float flowFieldIncrement;
    public float flowFieldStrength;
    public int maxFoods, maxCreatures; // for computational limits
    public GameObject creaturePrefab, foodDispenserPrefab, terrainPrefab;
    public GameObject baseBlockPrefab, mouthBlockPrefab, wombBlockPrefab, sensorBlockPrefab, motorBlockPrefab;
    public GameObject[] mutationBlocks, dynamicCostBlocks;
    public Transform creatures, foods, foodDispensers;
    private Terrain oceanFloor;
    private List<GameObject> population;
    private float flowFieldOffsetX, flowFieldOffsetY, flowFieldOffsetZ;
    private float flowFieldCounter;

    void Start() {
        InitializeTerrain();
        SpawnInitialPopulation();
        SpawnFoodDispensers();
        this.flowFieldCounter = 0f;
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter);
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter + 60);
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter + 120);
    }

    void FixedUpdate() {

        // fixed update won't get called if timescale is set to 0
        if (timeScale != 0)
            Time.timeScale = timeScale;

        UpdateFlowField();
        UpdateFoodParticles();
        UpdateCreatures();
    }

    private void UpdateFoodParticles() {
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
    }

    private void UpdateCreatures() {
        List<GameObject> creaturesToKill = new List<GameObject>();
        List<GameObject> futureParents = new List<GameObject>();

        foreach (GameObject creature in this.population) {
            ApplyFlowField(creature);
            CreatureScript creatureScript = creature.GetComponent<CreatureScript>();

            // ignore dead creatures, and kill them later
            if (creatureScript.dead) {
                creaturesToKill.Add(creature);
                continue;
            }

            // remember all creatures that want to have a child
            if (creatureScript.reproduce)
                futureParents.Add(creature);

            WrapObject(creature);
        }

        foreach (GameObject creature in futureParents) {
            if (this.population.Count < maxCreatures)
                SpawnChild(creature);
        }

        for (int i = 0; i < creaturesToKill.Count; i++) {
            KillCreature(creaturesToKill[0]);
        }
    }

    private void SpawnChild(GameObject creature) {

        // parent actions
        CreatureScript creatureScript = creature.GetComponent<CreatureScript>();
        creatureScript.reproduce = false;
        creatureScript.energy -= birthCost;

        // child actions
        GameObject newChild = Instantiate(creature, creatures);
        newChild.transform.SetPositionAndRotation(creature.transform.position + new Vector3(1, 1, 1),
                                                  creature.transform.rotation);
        MutateCreature(newChild);
        NewCreature(newChild);
    }

    private void SpawnCreature(Vector3 location) {
        GameObject newCreature = Instantiate(creaturePrefab, location, Random.rotation, creatures);
        NewCreature(newCreature);
    }

    private void NewCreature(GameObject newCreature) {
        this.population.Add(newCreature);
        newCreature.name = "Creature";
    }

    private void MutateCreature(GameObject creature) {
        if (Random.value < addBlockMutationRate) {
            addMutationBlock(creature);
        }
    }

    private void changeDynamicBlockCost(GameObject creature) {

    }

    private void addMutationBlock(GameObject creature) {
        CreatureScript creatureScript = creature.GetComponent<CreatureScript>();
        GameObject newBlock = Instantiate(this.mutationBlocks[Random.Range(0, this.mutationBlocks.Length)],
                                          creature.transform);
        Vector3 blockPosition = getRandomBlockPosition(creatureScript);
        newBlock.transform.localPosition = blockPosition;
        creatureScript.blocks.Add(newBlock);
    }

    private Vector3 getRandomBlockPosition(CreatureScript creature) {
        List<Vector3> options = new List<Vector3>();
        foreach (GameObject block in creature.blocks) {

            // can't spawn a block from a mouth (to prevent mouths being inside a body)
            if (block.tag == "Mouth")
                continue;

            // assume all faces have no adjascent blocks
            List<Vector3> blockOptions = new List<Vector3>();
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    for (int k = -1; k < 2; k++) {
                        Vector3 newOffset = new Vector3(i, j, k);
                        Vector3 newPosition = block.transform.localPosition + newOffset;
                        if (!options.Contains(newPosition) && newOffset.magnitude == 1)
                            blockOptions.Add(newPosition);
                    }
                }
            }

            // remove all options that contain a block (this also removes the block 'block')
            foreach (GameObject adj in creature.blocks) {
                if (blockOptions.Contains(adj.transform.localPosition))
                    blockOptions.Remove(adj.transform.localPosition);
            }

            // log block options into all options
            options.AddRange(blockOptions);
        }

        // return a random block
        if (options.Count != 0) {
            Vector3 randomPosition = options[Random.Range(0, options.Count)];
            return randomPosition;
        } else {
            throw new NotImplementedException("Creature has no room for new block, shouldn't be possible.");
        }
    }

    private void InitializeTerrain() {
        oceanFloor = Instantiate(terrainPrefab, Vector3.zero, Quaternion.identity).GetComponent<Terrain>();
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

    void KillCreature(GameObject creature) {
        creature.transform.parent = null;
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
        this.flowFieldCounter += flowFieldIncrement;
        if (this.flowFieldCounter == 360f) {
            this.flowFieldCounter = 0f;
        }
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter);
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter + 60);
        this.flowFieldOffsetX = Mathf.Sin(this.flowFieldCounter + 120);
    }

    void ApplyFlowField(GameObject creature) {

        // add a flow field vector for each creature using perlin noise
        Vector3 creaturePos = creature.transform.position;
        creature.GetComponent<Rigidbody>().AddForce(GetFlowFieldVector(creaturePos), ForceMode.Acceleration);
    }

    public Vector3 GetFlowFieldVector(Vector3 position) {

        // offset the noise values of each axis so the vectors look more natural
        float noiseX = Mathf.PerlinNoise((float)(position.x) / worldX * flowFieldGranuity,
                                         this.flowFieldOffsetX / worldX * flowFieldGranuity) - 0.5f;
        float noiseY = Mathf.PerlinNoise((float)(position.y) / worldY * flowFieldGranuity,
                                         this.flowFieldOffsetX / worldY * flowFieldGranuity) - 0.5f;
        float noiseZ = Mathf.PerlinNoise((float)(position.z) / worldZ * flowFieldGranuity,
                                         this.flowFieldOffsetX / worldZ * flowFieldGranuity) - 0.5f;
        return new Vector3(noiseX, noiseY, noiseZ) * flowFieldStrength;
    }

}
