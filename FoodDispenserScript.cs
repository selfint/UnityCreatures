using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDispenserScript : MonoBehaviour {

    public GameObject foodPrefab;
    public GameObject gameManager;
    public Transform foodSpawnLocation;
    public float foodSpawnRate;
    private float lastSpawn;

    // Start is called before the first frame update
    void Start() {
        this.lastSpawn = 0f;
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate() {
        if (this.lastSpawn >= this.foodSpawnRate) {
            SpawnFood();
            this.lastSpawn = 0f;
        }
        this.lastSpawn += Time.fixedDeltaTime;
    }

    void SpawnFood() {
        GameObject food = Instantiate(foodPrefab, foodSpawnLocation.position, foodSpawnLocation.rotation);
        food.GetComponent<FoodScript>().gameManager = gameManager.GetComponent<GameManagerScript>();
    }
}
