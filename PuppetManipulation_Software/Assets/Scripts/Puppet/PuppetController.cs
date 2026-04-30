using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;
using System.Linq;

//加速度センサで首を制御するプログラム（使ってない）

public class PuppetController : MonoBehaviour
{
    public SerialHandler serialHandler;

    private List<Vector3> angleCache = new List<Vector3>();
    public int angleCacheNum = 10;
    public GameObject cam;
    public Vector3 angle
    {
        private set
        {
            angleCache.Add(value);
            if (angleCache.Count > angleCacheNum)
            {
                angleCache.RemoveAt(0);
            }
        }
        get
        {
            if (angleCache.Count > 0)
            {
                var sum = Vector3.zero;
                angleCache.ForEach(angle => { sum += angle; });
                return sum / angleCache.Count;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    private Vector3 accel = new Vector3(0, 0, 0);
    private Vector3 aim = new Vector3(0, 1, 0);

    private List<float> data_listX;
    private List<float> data_listY;
    private List<float> data_listZ;
    private int data_count;
    private int data_num;
    private int insert_index;

    private float ave_x;
    private float ave_y;
    private float ave_z;

    public GameObject spine;
    public GameObject neck;

    /*可動範囲*/
    public float pitch_front = 40f; //前に傾ける角度
    public float pitch_back = 20f; //後ろに傾ける角度
    public float roll_right = 20f; //右にかしげる角度
    public float roll_left = 20f; //左にかしげる角度


    void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;

        /*平滑化のためのキューの初期化*/
        data_listX = new List<float>();
        data_listY = new List<float>();
        data_listZ = new List<float>();
        data_num = 25;


    }

    void Update()
    {
        

        if (data_listX.Count >= data_num)
        {
            data_listX.RemoveAt(insert_index);
            data_listX.Insert(insert_index, accel.x);
            data_listY.RemoveAt(insert_index);
            data_listY.Insert(insert_index, accel.y);
            data_listZ.RemoveAt(insert_index);
            data_listZ.Insert(insert_index, accel.z);
            insert_index++;
            if (insert_index == data_num) { insert_index = 0; }
        }
        else
        {
            data_listX.Add(accel.x);
            data_listY.Add(accel.y);
            data_listZ.Add(accel.z);
        }

        ave_x = data_listX.Average();
        ave_y = data_listY.Average();
        ave_z = data_listZ.Average() / 8.0f;
        Vector3 shake = new Vector3(ave_z, 0, ave_x);

        var aim = cam.transform.position - this.transform.position;
        var look = Quaternion.LookRotation(aim, shake);
        //this.transform.localRotation = look;

        RotateHead();

    }

    void RotateHead()
    {

        /*現在のspineの回転値（基準値）*/
        Vector3 angle = spine.transform.eulerAngles;

        /*うなづきの回転*/
        float angleZ = Mathf.Atan2(ave_x * 3f, ave_y) * 180 / Mathf.PI+90;
        //範囲制限
        if ((angleZ < -pitch_front && angleZ >= -90f) || angleZ <= 270f && angleZ >= 180f){angleZ = -pitch_front; }
        else if (angleZ > pitch_back && angleZ < 180f){angleZ = pitch_back;}

        /*横揺れの回転*/
        float angleX = Mathf.Atan2(ave_z, ave_x) * 180 / Mathf.PI - 180;
        //範囲制限
        if (angleX < -roll_right && angleX > -180f) { angleX = -roll_right; }
        else if (angleX > -(360-roll_left) && angleX < -180f) { angleX = -(360 - roll_left); } 

        /*回転値の設定*/
        Vector3 bow_angle = new Vector3(angle.x+angleX, angle.y, angle.z+angleZ);
        neck.transform.eulerAngles = bow_angle;
    }

    void OnDataReceived(string message)
    {
        var data = message.Split(
                new string[] { "\t" }, System.StringSplitOptions.None);
        if (data.Length < 3) return;

        try
        {
            var ax = float.Parse(data[0]);
            var az = float.Parse(data[1]);
            var ay = float.Parse(data[2]);
            accel = new Vector3(ax, ay, az);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}
