using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRecorder : MonoBehaviour
{
    private List<Vector3> positions = new List<Vector3>();
    private List<Quaternion> rotations = new List<Quaternion>();
    private List<float> timestamps = new List<float>(); // フレームの経過時間を記録するリスト
    private bool isRecording = false;
    private float startTime;
    public GameObject moveObject;

    void Update()
    {
        if (isRecording)
        {
            RecordData();
        }
    }

    public void StartRecording()
    {
        positions.Clear();
        rotations.Clear();
        timestamps.Clear(); // タイムスタンプもクリアする
        isRecording = true;
        startTime = Time.time; // レコーディング開始時刻を保存
        moveObject.GetComponent<OptitrackRigidBody>().enabled = true;
    }

    public void StopRecording()
    {
        isRecording = false;
        moveObject.GetComponent<OptitrackRigidBody>().enabled = false;
    }

    void RecordData()
    {
        positions.Add(moveObject.transform.position);
        rotations.Add(moveObject.transform.rotation);
        timestamps.Add(Time.time - startTime); // 現在の経過時間を記録
    }

    public List<Vector3> GetRecordedPositions()
    {
        return positions;
    }

    public List<Quaternion> GetRecordedRotations()
    {
        return rotations;
    }

    public List<float> GetRecordedTimestamps()
    {
        return timestamps;
    }
}
