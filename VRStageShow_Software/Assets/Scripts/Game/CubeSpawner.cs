using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnInterval = 3f;
    public float spawnHeight = 5f;

    [Header("Cube1 Position")]
    public float cube1X = 0;
    public float cube1Z = -0.5f;

    private bool started = false;
    public Transform player;


    void Start()
    {
        Debug.Log("Spawner Start");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed");
        }
        // まだ開始していない状態でキー入力
        if (!started && Input.GetKeyDown(KeyCode.Space))
        {
            started = true;

            SpawnCube(); // 最初の1個

            // その後は自動生成
            InvokeRepeating(nameof(SpawnCube), spawnInterval, spawnInterval);
        }
    }

    Vector3 GetSpawnPosition()
    {
        float radius = 0.5f;

        // 左右どちらかランダムで選ぶ
        bool isLeft = Random.value < 0.5f;

        float angle;

        if (isLeft)
        {
            angle = Random.Range(-90f, -60f);
        }
        else
        {
            angle = Random.Range(60f, 90f);
        }

        // 角度を方向ベクトルに変換
        Quaternion rot = Quaternion.Euler(0, angle, 0);
        Vector3 dir = rot * player.forward;

        Vector3 pos = player.position + dir * radius;
        pos.y = spawnHeight;

        return pos;
    }

    void SpawnCube()
    {
        Vector3 pos = GetSpawnPosition();
        Instantiate(cubePrefab, pos, Quaternion.identity);
    }
}
