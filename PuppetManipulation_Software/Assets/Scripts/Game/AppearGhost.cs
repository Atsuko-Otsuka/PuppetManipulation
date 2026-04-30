using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearGhost : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float appearInterval = 3f;
    public float appearHeight = 1f;

    [Header("Ghost Appear Position")]
    public float ghost1X = 1f;

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
            InvokeRepeating(nameof(SpawnCube), appearInterval, appearInterval);
        }
    }

    Vector3 GetSpawnPosition()
    {
        // 左右どちらかランダムで選ぶ
        bool isLeft = Random.value < 0.5f;

        float posZ;

        if (isLeft)
        {
            posZ = -0.5f;
        }
        else
        {
            posZ = 0.5f;
        }

        Vector3 pos = new Vector3(player.position.x + ghost1X, appearHeight, player.position.z + posZ);

        return pos;
    }

    void SpawnCube()
    {
        Vector3 pos = GetSpawnPosition();
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}
