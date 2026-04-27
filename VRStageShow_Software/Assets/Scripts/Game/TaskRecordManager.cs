using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TaskRecordManager : MonoBehaviour
{
    public Transform avatarTarget;
    public Transform rotationTarget; // アバター頭部
    public Transform hmdTarget; // Puppetモード時にHMDの動きを記録する用
    private int hitCount = 0;
    public TMP_Text hitCountText;
    //public float interval = 1f;

    float startTime;
    bool started = false;
    float nextTime;



    public bool record = true;
    public enum ViewMode { Puppet, HMD }
    public ViewMode viewMode = ViewMode.Puppet;

    StringBuilder csv = new();
    public string customFolder = @"C:\Users\yanoo\Documents\Otsuka\Data\CollectCubeTask";

    void Start()
    {
        if(viewMode == ViewMode.Puppet)
        {
            csv.AppendLine("t(sec),ava_x,ava_z,ava_y,head_roll,head_pitch,head_yaw,hmd_roll,hmd_pitch,hmd_yaw");     // ヘッダ
        }
        else
        {
            csv.AppendLine("t(sec),ava_x,ava_z,ava_y,head_roll,head_pitch,head_yaw");     // ヘッダ
        }  
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    // スタート判定
    //    if (other.CompareTag("Start"))
    //    {
    //        if(started) return;
    //        Destroy(other.gameObject);
    //        startTime = Time.time;
    //        nextTime = startTime;
    //        hitCount = 0;
    //        started = true;
    //        Debug.Log("スタート Task");
    //    }
        
    //}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CollectCube"))
        {
            if (!started)
            {
                started = true;
                startTime = Time.time;
                nextTime = startTime;
                hitCount = 0;
                Debug.Log("最初のキューブを取得：計測開始");
            }

            // キューブ取得処理
            Debug.Log("Hit " + collision.gameObject.name);
            Destroy(collision.gameObject);
            hitCount++;
            hitCountText.text = "Count: " + hitCount.ToString();

            // ゴール判定
            if (hitCount >= 10)
            {
                started = false;
                float elapsed = Time.time - startTime;
                Debug.Log($"全キューブ回収完了！ 経過時間：{elapsed:F2} 秒");

                // データの最後に合計時間を記録
                csv.AppendLine("Total Elapsed Time");
                csv.AppendLine($"{elapsed:F4}");

                WriteCsv(elapsed);
            }
        }
    }

    void Update()
    {
        if (!started) return;
        Vector3 avatarPosition = avatarTarget.position;
        Vector3 headRot = rotationTarget.transform.eulerAngles;
        
        float t = Time.time - startTime;
        if (viewMode == ViewMode.Puppet)
        {
            Vector3 hmdRot = hmdTarget.transform.eulerAngles;
            csv.AppendLine($"{Time.frameCount},{t:F4},{avatarPosition.x:F4},{avatarPosition.z:F4},{avatarPosition.y:F4},{headRot.x:F4},{headRot.z:F4},{headRot.y:F4},{hmdRot.x:F4},{hmdRot.z:F4},{hmdRot.y:F4}");
        }
        else
        {
            csv.AppendLine($"{Time.frameCount},{t:F4},{avatarPosition.x:F4},{avatarPosition.z:F4},{avatarPosition.y:F4},{headRot.x:F4},{headRot.z:F4},{headRot.y:F4}");
        }
        
    }

    void WriteCsv(float totalTime)
    {
        if (!record) return;

        if (!Directory.Exists(customFolder))
        {
            Directory.CreateDirectory(customFolder);
        }

        string viewLabel = viewMode == ViewMode.Puppet ? "Puppet" : "HMD";
        string fileName = $"TaskRecord_{viewLabel}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(customFolder, fileName);
        File.WriteAllText(path, csv.ToString(), Encoding.UTF8);

        Debug.Log($"Goal Time = {totalTime:F2} s\n CSV saved to: {path}");
    }

}
