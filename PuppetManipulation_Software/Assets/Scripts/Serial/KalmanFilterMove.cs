using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

/// <summary>
/// SerialHandlerからセンサーデータを受け取り、カルマンフィルタを適用してGameObjectを動かす
/// </summary>
public class KalmanFilterMove : MonoBehaviour
{
    // --- Unityエディタから設定 ---
    [Header("連携コンポーネント")]
    public SerialHandler serialHandler; // SerialHandlerをドラッグ&ドロップ
    public TMP_Text text;
    public GameObject targetObject;     // 動かしたいオブジェクト(Cubeなど)をドラッグ&ドロップ

    [Header("調整パラメータ")]
    public float smoothSpeed = 10.0f;   // 回転のスムージング速度

    // --- スレッド間のデータ同期用 ---
    private readonly object _lock = new object(); // ロック用のオブジェクト
    private Vector3 rawAcc = Vector3.zero;
    private Vector3 rawGyro = Vector3.zero;
    private bool isDataReady = false;

    // --- 時間管理 ---
    private double preTime;

    // --- ジャイロ積分値 ---
    private double xDegSum = 0.0, yDegSum = 0.0;
    private double preXdegS = 0.0, preYdegS = 0.0;

    // --- センサーオフセット (Pythonコードから引用) ---
    private double gyroOffX = 0.121;
    private double gyroOffY = -0.171;

    // --- カルマンフィルタ パラメータ ---
    private SimpleMatrix matA, matB, matBu, matQ, matC, u;
    private double nR;
    private SimpleMatrix xhat_k, yhat_k, xnP, ynP;

    // --- Cube回転用 ---
    private Quaternion targetRotation;

    void Start()
    {
        // 1. SerialHandlerのイベントに、データ受信時の処理を登録
        if (serialHandler == null)
        {
            Debug.LogError("SerialHandlerが設定されていません。インスペクターから設定してください。");
            enabled = false;
            return;
        }
        serialHandler.OnDataReceived += OnSerialDataReceived;

        // 2. 時間の初期化
        preTime = Time.realtimeSinceStartupAsDouble;

        // 3. カルマンフィルタ パラメータの初期化
        double a = 0.75;
        matA = new SimpleMatrix(new double[,] { { a, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } });
        matB = new SimpleMatrix(new double[,] { { 1 - a, 0 }, { 0, 0 }, { 0, 1 } });
        matBu = new SimpleMatrix(new double[,] { { 0 } });
        matQ = new SimpleMatrix(new double[,] { { 60, 0 }, { 0, 60 } });
        nR = 1e-4;
        matC = new SimpleMatrix(new double[,] { { 1, 1, -1 } });
        u = new SimpleMatrix(new double[,] { { 0 } });
        xhat_k = new SimpleMatrix(3, 1);
        yhat_k = new SimpleMatrix(3, 1);
        xnP = SimpleMatrix.Identity(3) * 1000.0;
        ynP = SimpleMatrix.Identity(3) * 1000.0;

        // 4. targetObjectの初期回転を保存
        if (targetObject != null)
        {
            targetRotation = targetObject.transform.rotation;
        }
    }

    /// <summary>
    /// シリアルデータを受信したときに呼び出される関数
    /// </summary>
    void OnSerialDataReceived(string message)
    {
        try
        {
            string[] values = message.Split(',');
            text.text = "ax:" + values[0] + " " + "ay:" + values[1] + " " + "az:" + values[2] + "\n" + "gx:" + values[3] + " " + "gy:" + values[4] + " " + "gz:" + values[5] + "\n" + "mx:" + values[6] + " " + "my:" + values[7] + " " + "mz:" + values[8] + "\n"; // シリアルの値をテキストに表示


            // ★修正点: 先にデータの個数をチェックする
            // カルマンフィルタは6軸(Acc+Gyro)のデータを使用
            if (values.Length >= 6)
            {
                // UIテキストの更新 (6個または9個のデータに対応)
                if (text != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"ax:{values[0]} ay:{values[1]} az:{values[2]}");
                    sb.AppendLine($"gx:{values[3]} gy:{values[4]} gz:{values[5]}");
                    if (values.Length >= 9)
                    {
                        sb.AppendLine($"mx:{values[6]} my:{values[7]} mz:{values[8]}");
                    }
                    text.text = sb.ToString();
                }

                // データをパースして、共有変数に格納
                var acc = new Vector3(
                    float.Parse(values[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(values[1].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(values[2].Trim(), CultureInfo.InvariantCulture)
                );
                var gyro = new Vector3(
                    float.Parse(values[3].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(values[4].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(values[5].Trim(), CultureInfo.InvariantCulture)
                );

                lock (_lock)
                {
                    rawAcc = acc;
                    rawGyro = gyro;
                    isDataReady = true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[KalmanFilter] Serial Parse Error: {e.Message}");
        }
    }

    void Update()
    {
        Vector3 currentAcc, currentGyro;
        bool hasNewData;

        // lockブロック内で共有データを安全に読み取る
        lock (_lock)
        {
            hasNewData = isDataReady;
            currentAcc = rawAcc;
            currentGyro = rawGyro;
            isDataReady = false; // 読み取ったのでフラグを下ろす
        }

        if (hasNewData)
        {
            // --- ここからカルマンフィルタの計算 ---
            Vector3 gyro = currentGyro;
            Vector3 acc = currentAcc;

            gyro.x += (float)gyroOffX;
            gyro.y += (float)gyroOffY;

            double nowTime = Time.realtimeSinceStartupAsDouble;
            double dT = nowTime - preTime;
            if (dT <= 0) dT = 1.0 / 60.0;

            xDegSum += (preXdegS + gyro.x) / 2.0 * dT;
            yDegSum += (preYdegS + gyro.y) / 2.0 * dT;

            preXdegS = gyro.x;
            preYdegS = gyro.y;
            preTime = nowTime;

            double xDegAcc = Math.Atan2(acc.y, Math.Sqrt(acc.x * acc.x + acc.z * acc.z)) * (180.0 / Math.PI);
            double yDegAcc = Math.Atan2(-acc.x, Math.Sqrt(acc.y * acc.y + acc.z * acc.z)) * (180.0 / Math.PI);

            SimpleMatrix xDeg = new SimpleMatrix(new double[,] { { xDegSum - xDegAcc } });
            SimpleMatrix yDeg = new SimpleMatrix(new double[,] { { yDegSum - yDegAcc } });

            var (xhat_new, P_new_x, _) = KalmanFilterLogic.Filter(matA, matB, matBu, matC, matQ, nR, u, xDeg, xhat_k, xnP);
            xhat_k = xhat_new;
            xnP = P_new_x;

            var (yhat_new, P_new_y, _) = KalmanFilterLogic.Filter(matA, matB, matBu, matC, matQ, nR, u, yDeg, yhat_k, ynP);
            yhat_k = yhat_new;
            ynP = P_new_y;

            double xDeghat1 = xDegSum - xhat_k[0, 0] - xhat_k[1, 0];
            double yDeghat1 = yDegSum - yhat_k[0, 0] - yhat_k[1, 0];

            targetRotation = Quaternion.Euler((float)xDeghat1, (float)yDeghat1, 0f);
        }

        // --- 常にオブジェクトの回転をスムーズに更新 ---
        if (targetObject != null)
        {
            targetObject.transform.rotation = Quaternion.Slerp(
                targetObject.transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }
}


/// <summary>
/// Pythonの kalmanFilter 関数を移植した静的クラス
/// </summary>
public static class KalmanFilterLogic
{
    public static (SimpleMatrix, SimpleMatrix, SimpleMatrix) Filter(SimpleMatrix A, SimpleMatrix B, SimpleMatrix Bu, SimpleMatrix C, SimpleMatrix Q, double R, SimpleMatrix u, SimpleMatrix y, SimpleMatrix xhat, SimpleMatrix P)
    {
        int nn = A.Rows;

        SimpleMatrix xhat_m = A * xhat;

        SimpleMatrix P_m = (A * P * A.Transpose()) + (B * Q * B.Transpose());
        double innovationCov = (C * P_m * C.Transpose())[0, 0] + R;
        SimpleMatrix G = (P_m * C.Transpose()) / innovationCov;
        SimpleMatrix innovation = y - (C * xhat_m);
        SimpleMatrix xhat_new = xhat_m + (G * innovation);
        SimpleMatrix I = SimpleMatrix.Identity(nn);
        SimpleMatrix P_new = (I - (G * C)) * P_m;
        return (xhat_new, P_new, G);
    }
}


/// <summary>
/// NumPyの代わりとなる、最小限の行列計算クラス
/// </summary>
public class SimpleMatrix
{
    public readonly double[,] Data;
    public readonly int Rows;
    public readonly int Cols;
    public SimpleMatrix(int rows, int cols) { Rows = rows; Cols = cols; Data = new double[rows, cols]; }
    public SimpleMatrix(double[,] data) { Data = (double[,])data.Clone(); Rows = data.GetLength(0); Cols = data.GetLength(1); }
    public double this[int row, int col] { get { return Data[row, col]; } set { Data[row, col] = value; } }
    public static SimpleMatrix Identity(int n) { var m = new SimpleMatrix(n, n); for (int i = 0; i < n; i++) m[i, i] = 1.0; return m; }
    public SimpleMatrix Transpose() { var m = new SimpleMatrix(Cols, Rows); for (int r = 0; r < Rows; r++) for (int c = 0; c < Cols; c++) m[c, r] = Data[r, c]; return m; }
    public static SimpleMatrix operator *(SimpleMatrix a, SimpleMatrix b) { if (a.Cols != b.Rows) throw new ArgumentException("Matrix dimensions do not match for multiplication."); var result = new SimpleMatrix(a.Rows, b.Cols); for (int r = 0; r < result.Rows; r++) for (int c = 0; c < result.Cols; c++) { double sum = 0; for (int k = 0; k < a.Cols; k++) sum += a[r, k] * b[k, c]; result[r, c] = sum; } return result; }
    public static SimpleMatrix operator *(SimpleMatrix a, double s) { var m = new SimpleMatrix(a.Rows, a.Cols); for (int r = 0; r < a.Rows; r++) for (int c = 0; c < a.Cols; c++) m[r, c] = a[r, c] * s; return m; }
    public static SimpleMatrix operator /(SimpleMatrix a, double s) { var m = new SimpleMatrix(a.Rows, a.Cols); for (int r = 0; r < a.Rows; r++) for (int c = 0; c < a.Cols; c++) m[r, c] = a[r, c] / s; return m; }
    public static SimpleMatrix operator +(SimpleMatrix a, SimpleMatrix b) { if (a.Rows != b.Rows || a.Cols != b.Cols) throw new ArgumentException("Matrix dimensions must be identical for addition."); var m = new SimpleMatrix(a.Rows, a.Cols); for (int r = 0; r < a.Rows; r++) for (int c = 0; c < a.Cols; c++) m[r, c] = a[r, c] + b[r, c]; return m; }
    public static SimpleMatrix operator -(SimpleMatrix a, SimpleMatrix b) { if (a.Rows != b.Rows || a.Cols != b.Cols) throw new ArgumentException("Matrix dimensions must be identical for subtraction."); var m = new SimpleMatrix(a.Rows, a.Cols); for (int r = 0; r < a.Rows; r++) for (int c = 0; c < a.Cols; c++) m[r, c] = a[r, c] - b[r, c]; return m; }
}
