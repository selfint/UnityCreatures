using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

    public float timeScale = 1f;
    public int initialPopulationSize = 10;
    public GameObject creaturePrefab;
    private List<GameObject> population;
    public Transform spawnLocation;
    public float spawnNoise = 10f;
    public Vector3[][][] flowField;
    public int worldX, worldY, worldZ;
    private float xOffset, yOffset, zOffset;

    void Start() {
        spawnInitialPopulation();
        this.xOffset = 0f;
        this.yOffset = worldX * worldY;
        this.zOffset = worldX * worldY * 2;
        this.flowField = generateFlowField(this.worldX, this.worldY, this.worldZ);
    }

    void spawnInitialPopulation() {
        this.population = new List<GameObject>();
        for (int i = 0; i < this.initialPopulationSize; i++) {
            GameObject newCreature = Instantiate(creaturePrefab,
                                                 randomSpawnLocation(spawnLocation.position, spawnNoise),
                                                 Quaternion.identity);
            this.population.Add(newCreature);
        }
    }

    Vector3 randomSpawnLocation(Vector3 center, float noise) {
        float x = center.x + Random.Range(-noise, noise);
        float y = center.y + Random.Range(-noise, noise);
        float z = center.z;
        return new Vector3(x, y, z);
    }

    void FixedUpdate() {
        if (timeScale != 0)
            Time.timeScale = timeScale;
        applyFlowField();
    }

    void applyFlowField() {
        foreach (GameObject creature in this.population) {
            Vector3 creaturePos = creature.transform.position;
            creature.GetComponent<Rigidbody>().AddForce(getFlowVector(creaturePos), ForceMode.Acceleration);
        }
    }

    Vector3 getFlowVector(Vector3 position) {
        int vectorX = Mathf.RoundToInt(position.x) + worldX / 2;
        int vectorY = Mathf.RoundToInt(position.y) + worldY / 2;
        int vectorZ = Mathf.RoundToInt(position.z) + worldZ / 2;
        return this.flowField[vectorX][vectorY][vectorZ];
    }

    Vector3[][][] generateFlowField(int x, int y, int z) {
        Vector3[][][] flowField = new Vector3[x][][];
        for (int i = 0; i < x; i++) {
            Vector3[][] flowFieldy = new Vector3[y][];
            for (int j = 0; j < y; j++) {
                Vector3[] flowFieldz = new Vector3[z];
                for (int k = 0; k < z; k++) {
                    float noiseX = Mathf.PerlinNoise(i + k + this.xOffset, j + this.xOffset) - 0.5f;
                    Debug.Log(noiseX);
                    float noiseY = Mathf.PerlinNoise(i + k + this.yOffset, j + this.yOffset) - 0.5f;
                    float noiseZ = Mathf.PerlinNoise(i + k + this.zOffset, j + this.zOffset) - 0.5f;
                    flowFieldz[k] = new Vector3(noiseX, noiseY, noiseZ);
                }
                flowFieldy[j] = flowFieldz;
            }
            flowField[i] = flowFieldy;
        }

        return flowField;
    }

}
