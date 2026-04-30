using UnityEngine;

public class GhostCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 手（Handタグ）に当たった場合
        if (other.CompareTag("Hand"))
        {
            Success();
        }
        // 体（Playerタグ）に当たった場合
        else if (other.CompareTag("Player"))
        {
            Failure();
        }
    }

    void Success()
    {
        Debug.Log("成功");
        Destroy(gameObject); // お化けを消す
    }

    void Failure()
    {
        Debug.Log("失敗");
        Destroy(gameObject); // 当たったお化けは消す
    }
}