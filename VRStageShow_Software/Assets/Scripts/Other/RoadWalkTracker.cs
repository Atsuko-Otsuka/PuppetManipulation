using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

public class RoadWalkTracker : MonoBehaviour
{
    public Transform target;
    public float interval = 1f;

    float startTime;
    bool started = false;
    float nextTime;

    float touchTime = 0f;  // 壁と触れていた累積時間
    bool touchingWall = false;
    float touchCount = 0;

    public bool record = true;
    public enum ViewMode { FirstPerson, ThirdPerson }
    public ViewMode viewMode = ViewMode.FirstPerson;

    StringBuilder csv = new();
    public string customFolder = @"C:\Users\yanoo\Documents\Otsuka\Data\LazyVR";

    void Start()
    {
        if (target == null) target = transform;
        csv.AppendLine("t(sec),x,z,y");     // ヘッダ
    }

    void OnTriggerEnter(Collider other)
    {
        // スタート判定
        if (other.CompareTag("Start"))
        {
            startTime = Time.time;
            nextTime = startTime;
            touchTime = 0f;
            touchCount = 0;
            started = true;
            Debug.Log("スタート");
        }
        // ゴール判定
        else if (other.CompareTag("Goal") && started)
        {
            float elapsed = Time.time - startTime;
            Debug.Log($"ゴール  経過時間：{elapsed:F2} 秒, 壁接触時間: {touchTime:F2} 秒, 接触回数: {touchCount} 回");

            Vector3 p = target.position;
            csv.AppendLine("経過時間, 接触時間, 接触回数");
            csv.AppendLine($"{elapsed:F4},{touchTime:F4}, {touchCount}");

            started = false;
            WriteCsv(elapsed);
            //EditorApplication.isPlaying = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (started && col.gameObject.CompareTag("Wall"))
        {
            touchingWall = true;
            touchCount++;
            Debug.Log("touchEnter");
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (started && col.gameObject.CompareTag("Wall"))
        {
            touchingWall = false;
            Debug.Log("touchExit");
        }
    }

    void Update()
    {
        if (started && touchingWall)
            touchTime += Time.deltaTime;   // 1 フレーム分を加算

        if (!started || Time.fixedTime < nextTime) return;
        nextTime = Time.fixedTime + interval;

        Vector3 p = target.position;
        float t = Time.fixedTime - startTime;   // スタートからの経過秒
        csv.AppendLine($"{t:F4},{p.x:F4},{p.z:F4},{p.y:F4}");

        
    }

    void WriteCsv(float totalTime)
    {
        if (!record) return;

        string viewLabel = viewMode == ViewMode.FirstPerson ? "FirstPerson" : "ThirdPerson";
        string fileName = $"Run_{viewLabel}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(customFolder, fileName);
        File.WriteAllText(path, csv.ToString(), Encoding.UTF8);

        Debug.Log($"Goal Time = {totalTime:F2} s\n" +
                  $"CSV saved to: {path}");
    }

}
