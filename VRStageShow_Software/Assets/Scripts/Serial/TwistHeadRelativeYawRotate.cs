using System;
using UnityEngine;

public class TwistHeadRelativeYawRotate : MonoBehaviour
{
    [Header("必須設定")]
    //public SerialHandler serialHandler;
    public UDPHandler udpHandler;
    public Transform targetObject;       // RotationConstraint の Source にしているターゲット
    public Transform avatarBodyObject;   // アバター胴体（向き基準）

    [Header("ポート設定")]
    public int torsoPort = 1;  // ぬいぐるみ胴体
    public int headPort = 2;  // ぬいぐるみ頭部

    [Header("Quaternion軸マッピング（胴体）(1=x, 2=y, 3=z, 負の数で反転)")]
    public int torsoQuatX = 1;
    public int torsoQuatY = 2;
    public int torsoQuatZ = 3;

    [Header("Quaternion軸マッピング（頭部）(1=x, 2=y, 3=z, 負の数で反転)")]
    public int headQuatX = -2;
    public int headQuatY = -1;
    public int headQuatZ = -3;

    [Header("動きの滑らかさ")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.2f;

    [Header("適用方法")]
    [Tooltip("true=targetObject.rotation(ワールド)を直接更新 / false=localRotationを更新")]
    public bool applyWorldRotation = true;

    [Header("Twist フィルタ")]
    [Range(1f, 30f)]
    public float twistCutoffHz = 6f;

    private Quaternion _filteredTwist = Quaternion.identity;


    // --- 受信状態 ---
    private readonly object _lock = new object();

    private bool _torsoReady = false;
    private bool _headReady = false;

    private Quaternion _torsoRaw = Quaternion.identity;
    private Quaternion _headRaw = Quaternion.identity;

    private Quaternion _torsoInit = Quaternion.identity;
    private Quaternion _headInit = Quaternion.identity;

    void Start()
    {
        if (udpHandler != null)
        {
            //serialHandler.OnDataReceived += OnDataReceived;
            udpHandler.OnDataReceived += OnDataReceived;
        }
        else
        {
            Debug.LogError("TwistHeadRelativeYawRotate: SerialHandlerが未設定です");
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (udpHandler != null)
        {
            udpHandler.OnDataReceived -= OnDataReceived;
        }
    }

    void LateUpdate()
    {
        if (targetObject == null || avatarBodyObject == null) return;

        Quaternion torsoRaw, headRaw, torsoInit, headInit;
        bool torsoReady, headReady;

        lock (_lock)
        {
            torsoRaw = _torsoRaw;
            headRaw = _headRaw;
            torsoInit = _torsoInit;
            headInit = _headInit;
            torsoReady = _torsoReady;
            headReady = _headReady;
        }

        if (!torsoReady || !headReady) return;

        // 1) 初期姿勢からの差分（胴体・頭）
        Quaternion qTorso = Quaternion.Inverse(torsoInit) * torsoRaw;
        Quaternion qHead = Quaternion.Inverse(headInit) * headRaw;

        qTorso = NormalizeSafe(qTorso);
        qHead = NormalizeSafe(qHead);

        // 2) 胴体基準の頭部回転（これが本命）
        Quaternion qRel = Quaternion.Inverse(qTorso) * qHead;
        qRel = NormalizeSafe(qRel);

        // 3) 相対回転から「Yaw成分だけ」を安定に抽出（forwardを水平投影）
        Quaternion relTwist = ExtractTwist(qRel, Vector3.up);                 // 胴体基準Upまわり
        float alpha = 1f - Mathf.Exp(-2f * Mathf.PI * twistCutoffHz * Time.deltaTime);

        _filteredTwist = Quaternion.Slerp(
            _filteredTwist,
            relTwist,
            alpha
        );


        Quaternion bodyYaw = ExtractTwist(avatarBodyObject.rotation, Vector3.up); // world Upまわり（アバター）
        Quaternion worldTarget = bodyYaw * _filteredTwist;



        // 6) 適用（Constraint Sourceなので targetObject を回す）
        if (applyWorldRotation)
        {
            targetObject.rotation = Quaternion.Slerp(
                targetObject.rotation,
                worldTarget,
                smoothSpeed
            );
        }
        else
        {
            // localRotationにしたい場合：親の回転を打ち消してローカルへ変換
            Quaternion parentRot = (targetObject.parent != null) ? targetObject.parent.rotation : Quaternion.identity;
            Quaternion localTarget = Quaternion.Inverse(parentRot) * worldTarget;

            targetObject.localRotation = Quaternion.Slerp(
                targetObject.localRotation,
                localTarget,
                smoothSpeed
            );
        }
    }

    // ---------- 受信処理 ----------
    void OnDataReceived(string message)
    {
        try
        {
            // "port,w,x,y,z"
            string[] v = message.Split(',');
            if (v.Length < 5) return;

            int port = int.Parse(v[0]);
            float w_in = float.Parse(v[1]);
            float x_in = float.Parse(v[2]);
            float y_in = float.Parse(v[3]);
            float z_in = float.Parse(v[4]);

            // 正規化チェック（ざっくり）
            float mag2 = w_in * w_in + x_in * x_in + y_in * y_in + z_in * z_in;
            if (mag2 < 0.85f || mag2 > 1.15f) return;

            Quaternion raw = new Quaternion(x_in, y_in, z_in, w_in);

            if (port == torsoPort)
            {
                Quaternion mapped = MapQuat(raw, torsoQuatX, torsoQuatY, torsoQuatZ);
                mapped = NormalizeSafe(mapped);

                lock (_lock)
                {
                    _torsoRaw = mapped;
                    if (!_torsoReady)
                    {
                        _torsoInit = mapped;
                        _torsoReady = true;
                        // Debug.Log("Torso init set");
                    }
                }
            }
            else if (port == headPort)
            {
                Quaternion mapped = MapQuat(raw, headQuatX, headQuatY, headQuatZ);
                mapped = NormalizeSafe(mapped);

                lock (_lock)
                {
                    _headRaw = mapped;
                    if (!_headReady)
                    {
                        _headInit = mapped;
                        _headReady = true;
                        // Debug.Log("Head init set");
                    }
                }
            }
        }
        catch (Exception)
        {
            // 無視（ログがうるさくなるので必要ならWarningに）
        }
    }

    // ---------- ユーティリティ ----------
    private static Quaternion ExtractYawOnly(Quaternion q)
    {
        Vector3 f = q * Vector3.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 1e-8f) return Quaternion.identity;
        return Quaternion.LookRotation(f.normalized, Vector3.up);
    }

    private static Quaternion ExtractTwist(Quaternion q, Vector3 axis)
    {
        axis.Normalize();

        // q のベクトル部を axis に射影して twist を作る（swing-twist分解の定番簡易式）
        Vector3 r = new Vector3(q.x, q.y, q.z);
        Vector3 proj = Vector3.Project(r, axis);

        Quaternion twist = new Quaternion(proj.x, proj.y, proj.z, q.w);
        twist = NormalizeSafe(twist);

        // 連続性確保（符号反転で急に跳ねるのを抑える）
        if (twist.w < 0f)
            twist = new Quaternion(-twist.x, -twist.y, -twist.z, -twist.w);

        return twist;
    }


    private static Quaternion NormalizeSafe(Quaternion q)
    {
        float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (m < 1e-8f) return Quaternion.identity;
        return new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m);
    }

    private static Quaternion MapQuat(Quaternion raw, int mapX, int mapY, int mapZ)
    {
        float x = GetMappedAxis(mapX, raw);
        float y = GetMappedAxis(mapY, raw);
        float z = GetMappedAxis(mapZ, raw);
        float w = raw.w; // まずはそのまま（必要なら符号調整を検討）
        return new Quaternion(x, y, z, w);
    }

    private static float GetMappedAxis(int mapping, Quaternion q)
    {
        int axis = Mathf.Abs(mapping);
        float val = 0f;
        switch (axis)
        {
            case 1: val = q.x; break;
            case 2: val = q.y; break;
            case 3: val = q.z; break;
            default: val = 0f; break;
        }
        return (mapping < 0) ? -val : val;
    }
}