using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────
// データ定義
// ─────────────────────────────────────────────

/// <summary>判定する部位の種類</summary>
public enum BodyPart
{
    Head,
    RightUpperArm,
    RightForeArm,
    LeftUpperArm,
    LeftForeArm,
}

/// <summary>1部位に対する角度条件</summary>
[Serializable]
public class PartCondition
{
    public BodyPart part;

    [Tooltip("この部位のforward方向と比較するワールド方向")]
    public Vector3 referenceDirection = Vector3.forward;

    [Tooltip("referenceDirection との角度差がこの値以下なら条件成立")]
    public float maxAngleDeg = 45f;

    [Tooltip("trueにすると「角度差がmaxAngleDeg以上」に反転（静止判定などに使用）")]
    public bool invert = false;
}

/// <summary>静止判定条件（ジャイロ由来の角速度を使う）</summary>
[Serializable]
public class StillCondition
{
    public BodyPart part;

    [Tooltip("この値(deg/s)以下なら「静止している」と判定")]
    public float maxAngularVelocityDegPerSec = 10f;

    [Tooltip("静止を継続しなければならない秒数")]
    public float requiredDurationSec = 0.5f;
}

/// <summary>1つの魔法に対する判定定義（ScriptableObjectでも可）</summary>
[Serializable]
public class SpellDefinition
{
    public string spellName;

    [Tooltip("すべて成立すると魔法が発動する角度条件リスト")]
    public List<PartCondition> partConditions = new();

    [Tooltip("静止が必要な場合に設定。空なら静止判定なし")]
    public List<StillCondition> stillConditions = new();

    [Tooltip("発動後の再使用不可時間（秒）")]
    public float cooldownSec = 2f;

    [HideInInspector] public float lastFiredTime = -999f;
    [HideInInspector] public bool isCoolingDown => Time.time < lastFiredTime + cooldownSec;
}

// ─────────────────────────────────────────────
// 角速度を追跡するヘルパー
// ─────────────────────────────────────────────

/// <summary>Transform の角速度(deg/s)をフレームごとに計算して保持する</summary>
public class AngularVelocityTracker
{
    private readonly BodyPart _part;
    private Quaternion _prevRotation;
    private float _angularVelocity;   // deg/s
    private float _stillTimer;        // 静止継続秒数
    private bool _initialized;

    public float AngularVelocity => _angularVelocity;
    public float StillDuration => _stillTimer;

    public AngularVelocityTracker(BodyPart part) => _part = part;

    public void Update(Quaternion currentRotation, float dt)
    {
        if (!_initialized)
        {
            _prevRotation = currentRotation;
            _initialized = true;
            return;
        }

        // フレーム間の回転差 → 角速度(deg/s)
        float angleDelta = Quaternion.Angle(_prevRotation, currentRotation);
        _angularVelocity = dt > 0f ? angleDelta / dt : 0f;
        _prevRotation = currentRotation;

        // 静止継続タイマー
        if (_angularVelocity < 15f)   // 15 deg/s 以下を「動いていない」とみなす
            _stillTimer += dt;
        else
            _stillTimer = 0f;
    }
}

// ─────────────────────────────────────────────
// メインの判定クラス
// ─────────────────────────────────────────────

public class MagicPoseDetector : MonoBehaviour
{
    [Header("アバター")]
    [SerializeField] private Animator _animator;

    [Header("魔法定義リスト")]
    [SerializeField] private List<SpellDefinition> _spells = new();

    [Header("デバッグ")]
    [SerializeField] private bool _showGizmos = true;

    // イベント: 魔法名を引数に受け取る
    public event Action<string> OnSpellFired;

    // 部位 → Transform キャッシュ
    private Dictionary<BodyPart, Transform> _boneCache = new();

    // 部位 → 角速度トラッカー
    private Dictionary<BodyPart, AngularVelocityTracker> _trackers = new();

    // ─── Unity ライフサイクル ───────────────────

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        BuildBoneCache();
        BuildTrackers();
        SetupDefaultSpells();
    }

    private void Update()
    {
        UpdateTrackers();
        CheckAllSpells();
    }

    // ─── 初期化 ────────────────────────────────

    private void BuildBoneCache()
    {
        var mapping = new Dictionary<BodyPart, HumanBodyBones>
        {
            { BodyPart.Head,          HumanBodyBones.Head              },
            { BodyPart.RightUpperArm, HumanBodyBones.RightUpperArm     },
            { BodyPart.RightForeArm,  HumanBodyBones.RightLowerArm     },
            { BodyPart.LeftUpperArm,  HumanBodyBones.LeftUpperArm      },
            { BodyPart.LeftForeArm,   HumanBodyBones.LeftLowerArm      },
        };

        foreach (var (part, bone) in mapping)
        {
            var t = _animator.GetBoneTransform(bone);
            if (t != null)
                _boneCache[part] = t;
            else
                Debug.LogWarning($"[MagicPoseDetector] ボーンが見つかりません: {bone}");
        }
    }

    private void BuildTrackers()
    {
        foreach (BodyPart part in Enum.GetValues(typeof(BodyPart)))
            _trackers[part] = new AngularVelocityTracker(part);
    }

    /// <summary>
    /// Inspector で定義しない場合のデフォルト魔法セット。
    /// Inspector で _spells に要素を追加するとこちらは実行されない。
    /// </summary>
    private void SetupDefaultSpells()
    {
        if (_spells.Count > 0) return;

        // ── 炎の矢：右上腕を前に向ける ──────────────────────────────────
        _spells.Add(new SpellDefinition
        {
            spellName = "炎の矢",
            cooldownSec = 2f,
            partConditions = new List<PartCondition>
            {
                new() {
                    part               = BodyPart.RightUpperArm,
                    referenceDirection = Vector3.forward,
                    maxAngleDeg        = 40f,
                },
            },
        });

        // ── 雷撃：右上腕を真上に向ける ─────────────────────────────────
        _spells.Add(new SpellDefinition
        {
            spellName = "雷撃",
            cooldownSec = 3f,
            partConditions = new List<PartCondition>
            {
                new() {
                    part               = BodyPart.RightUpperArm,
                    referenceDirection = Vector3.up,
                    maxAngleDeg        = 35f,
                },
            },
        });

        // ── 氷の嵐：両上腕を左右に広げる（複合判定） ───────────────────
        _spells.Add(new SpellDefinition
        {
            spellName = "氷の嵐",
            cooldownSec = 5f,
            partConditions = new List<PartCondition>
            {
                new() {
                    part               = BodyPart.RightUpperArm,
                    referenceDirection = Vector3.right,   // 右腕は右方向
                    maxAngleDeg        = 40f,
                },
                new() {
                    part               = BodyPart.LeftUpperArm,
                    referenceDirection = Vector3.left,    // 左腕は左方向
                    maxAngleDeg        = 40f,
                },
            },
        });

        // ── 守護の盾：右前腕を水平に構えて静止 ────────────────────────
        _spells.Add(new SpellDefinition
        {
            spellName = "守護の盾",
            cooldownSec = 4f,
            partConditions = new List<PartCondition>
            {
                new() {
                    part               = BodyPart.RightForeArm,
                    referenceDirection = Vector3.forward,
                    maxAngleDeg        = 30f,
                },
            },
            stillConditions = new List<StillCondition>
            {
                new() {
                    part                       = BodyPart.RightForeArm,
                    maxAngularVelocityDegPerSec = 10f,
                    requiredDurationSec         = 1.5f,    // 1.5秒静止
                },
            },
        });

        // ── 回復の光：頭を下げながら両前腕を内側に引き寄せる（3部位複合）
        _spells.Add(new SpellDefinition
        {
            spellName = "回復の光",
            cooldownSec = 6f,
            partConditions = new List<PartCondition>
            {
                new() {
                    part               = BodyPart.Head,
                    referenceDirection = Vector3.down,    // 頭を下に傾ける
                    maxAngleDeg        = 40f,
                },
                new() {
                    part               = BodyPart.RightForeArm,
                    referenceDirection = Vector3.left,    // 右前腕を内側へ
                    maxAngleDeg        = 45f,
                },
                new() {
                    part               = BodyPart.LeftForeArm,
                    referenceDirection = Vector3.right,   // 左前腕を内側へ
                    maxAngleDeg        = 45f,
                },
            },
        });
    }

    // ─── 毎フレームの更新 ──────────────────────

    private void UpdateTrackers()
    {
        float dt = Time.deltaTime;
        foreach (var (part, bone) in _boneCache)
            _trackers[part].Update(bone.rotation, dt);
    }

    // ─── 判定メインループ ──────────────────────

    private void CheckAllSpells()
    {
        foreach (var spell in _spells)
        {
            if (spell.isCoolingDown) continue;
            if (EvaluateSpell(spell))
                FireSpell(spell);
        }
    }

    /// <summary>
    /// スペル定義のすべての条件を評価する。
    /// すべて true のとき true を返す。
    /// </summary>
    private bool EvaluateSpell(SpellDefinition spell)
    {
        // --- 角度条件 ---
        foreach (var cond in spell.partConditions)
        {
            if (!_boneCache.TryGetValue(cond.part, out var bone)) return false;

            float angle = Vector3.Angle(bone.forward, cond.referenceDirection);
            bool pass = angle <= cond.maxAngleDeg;
            if (cond.invert) pass = !pass;
            if (!pass) return false;
        }

        // --- 静止条件 ---
        foreach (var cond in spell.stillConditions)
        {
            if (!_trackers.TryGetValue(cond.part, out var tracker)) return false;

            bool pass = tracker.AngularVelocity <= cond.maxAngularVelocityDegPerSec
                     && tracker.StillDuration >= cond.requiredDurationSec;
            if (!pass) return false;
        }

        return true;
    }

    private void FireSpell(SpellDefinition spell)
    {
        spell.lastFiredTime = Time.time;
        Debug.Log($"[魔法発動] {spell.spellName}");
        OnSpellFired?.Invoke(spell.spellName);
    }

    // ─── デバッグ用ギズモ ──────────────────────

    private void OnDrawGizmos()
    {
        if (!_showGizmos || _boneCache == null) return;

        foreach (var (part, bone) in _boneCache)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(bone.position, bone.forward * 0.15f);
        }
    }

    // ─── 外部から判定状況を取得するユーティリティ ──

    /// <summary>
    /// UIやデバッグ表示向け。
    /// 各部位の角度条件が何%満たされているかを返す（0〜1）。
    /// </summary>
    public float GetSpellProgress(string spellName)
    {
        var spell = _spells.Find(s => s.spellName == spellName);
        if (spell == null) return 0f;

        int total = spell.partConditions.Count + spell.stillConditions.Count;
        if (total == 0) return 0f;

        int passed = 0;

        foreach (var cond in spell.partConditions)
        {
            if (!_boneCache.TryGetValue(cond.part, out var bone)) continue;
            float angle = Vector3.Angle(bone.forward, cond.referenceDirection);
            bool pass = (angle <= cond.maxAngleDeg);
            if (cond.invert) pass = !pass;
            if (pass) passed++;
        }

        foreach (var cond in spell.stillConditions)
        {
            if (!_trackers.TryGetValue(cond.part, out var tracker)) continue;
            if (tracker.AngularVelocity <= cond.maxAngularVelocityDegPerSec
             && tracker.StillDuration >= cond.requiredDurationSec)
                passed++;
        }

        return (float)passed / total;
    }
}