using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDispenserScript : MonoBehaviour {

    public GameObject FoodPrefab;
    public Transform FoodSpawnLocation;
    public float FoodSpawnRate;
    private float LastSpawn;

    // Start is called before the first frame update
    void Start() {
        this.LastSpawn = 0f;
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate() {
        if (this.LastSpawn >= this.FoodSpawnRate) {
            SpawnFood();
            this.LastSpawn = 0f;
        }
        this.LastSpawn += Time.fixedDeltaTime;
    }

    void SpawnFood() {
        Instantiate(FoodPrefab, FoodSpawnLocation.position, FoodSpawnLocation.rotation);
    }
}
