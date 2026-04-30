using UnityEngine;

public class GhostFollow : MonoBehaviour
{
    public float speed = 2f;
    private Transform player;

    void Start()
    {
        // シーン内の "Player" タグがついたオブジェクトを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // プレイヤーの方を向く
            transform.LookAt(player);
            // 前進する
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}