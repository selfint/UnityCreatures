using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDispenserScript : MonoBehaviour {

    public GameObject foodPrefab;
    [System.NonSerialized]
    public GameObject gameManager;
    public Transform foodSpawnLocation;
    public float foodSpawnRate;
    private float lastSpawn;
    public float foodVariance;
    public float foodMinSize;
    public float ejectionForce;

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
        GameObject food = Instantiate(foodPrefab, foodSpawnLocation.position, foodSpawnLocation.rotation,
                                      gameManager.GetComponent<GameManagerScript>().foods);
        float foodSize = Random.Range(foodMinSize, foodMinSize + foodVariance);
        food.transform.localScale = new Vector3(foodSize, foodSize, foodSize);
        food.GetComponent<Rigidbody>().AddForce(
            new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)).normalized * ejectionForce, 
            ForceMode.Impulse);
    }
}
