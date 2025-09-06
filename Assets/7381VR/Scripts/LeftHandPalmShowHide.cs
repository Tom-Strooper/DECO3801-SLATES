using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

public class LeftHandPalmShowHide : MonoBehaviour
{
    [Header("Assign your Book root")]
    public GameObject bookRoot;

    [Header("Palm-up thresholds (hysteresis)")]
    [Range(0f,1f)] public float palmUpDotToOpen = 0.65f; // 更严格
    [Range(0f,1f)] public float palmUpDotToClose = 0.50f; // 略宽松

    [Header("Hold times & cooldown")]
    public float openHoldSeconds  = 0.35f; // 出现需更久
    public float closeHoldSeconds = 0.25f; // 隐藏稍短
    public float cooldownSeconds  = 0.60f; // 切换后冷却

    [Header("Fist / open-hand detection")]
    [Range(0.3f,1.5f)] public float fistThresholdFactor = 0.90f;
    [Range(0.3f,1.5f)] public float openHandFactor     = 1.05f; // 张开手的“更大”阈值

    [Header("Motion gating")]
    public float maxHandSpeed = 0.20f; // m/s，超过认为在移动，不触发

    XRHandSubsystem handSubsystem;
    bool isShown = false;
    float lastToggleTime = -10f;
    float openStart = -1f, closeStart = -1f;




    // 手速估计
    Vector3 prevWristPos; float prevTime; bool hasPrev;

    void Start()
    {
        if (bookRoot != null) bookRoot.SetActive(false);
        TryGetHandSubsystem();
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running) { TryGetHandSubsystem(); return; }

        var left = handSubsystem.leftHand;
        if (!left.isTracked) { ResetTimers(); return; }

        if (!TryGetPose(left, XRHandJointID.Palm, out var palmPose) ||
            !TryGetPose(left, XRHandJointID.Wrist, out var wristPose) ||
            !TryGetPose(left, XRHandJointID.IndexMetacarpal, out var indexMeta) ||
            !TryGetPose(left, XRHandJointID.LittleMetacarpal, out var littleMeta))
        { ResetTimers(); return; }

        // 掌心法线
        Vector3 normal = Vector3.Cross(indexMeta.position - wristPose.position,
                                       littleMeta.position - wristPose.position).normalized;

        // 手速（基于腕关节）
        float speed = EstimateSpeed(wristPose.position);

        // 手宽 & 张开/握拳
        float handWidth = Vector3.Distance(indexMeta.position, littleMeta.position);
        float avgTipToPalm = AverageTipDistanceToPalm(left, palmPose.position);
        bool fist = avgTipToPalm > 0 && handWidth > 0 && (avgTipToPalm < handWidth * fistThresholdFactor);
        bool openHand = avgTipToPalm > 0 && handWidth > 0 && (avgTipToPalm > handWidth * openHandFactor);

        float now = Time.time;
        bool inCooldown = (now - lastToggleTime) < cooldownSeconds;

        if (!isShown)
        {
            bool palmUpStrict = Vector3.Dot(normal, Vector3.up) > palmUpDotToOpen;

            if (!inCooldown && palmUpStrict && openHand && speed <= maxHandSpeed)
            {
                if (openStart < 0f) openStart = now;
                if (now - openStart >= openHoldSeconds)
                {
                    SetShown(true);
                    lastToggleTime = now;
                    ResetTimers();
                }
            }
            else openStart = -1f;
        }
        else
        {
            bool palmUpLoose = Vector3.Dot(normal, Vector3.up) > palmUpDotToClose;

            if (!inCooldown && palmUpLoose && fist && speed <= maxHandSpeed)
            {
                if (closeStart < 0f) closeStart = now;
                if (now - closeStart >= closeHoldSeconds)
                {
                    SetShown(false);
                    lastToggleTime = now;
                    ResetTimers();
                }
            }
            else closeStart = -1f;
        }
    }

    void SetShown(bool on)
    {
        isShown = on;
        if (bookRoot != null) bookRoot.SetActive(on);
    }

    void ResetTimers() { openStart = -1f; closeStart = -1f; hasPrev = false; }

    float EstimateSpeed(Vector3 wristPos)
    {
        float t = Time.time;
        if (!hasPrev) { prevWristPos = wristPos; prevTime = t; hasPrev = true; return 0f; }
        float dt = Mathf.Max(1e-4f, t - prevTime);
        float speed = Vector3.Distance(wristPos, prevWristPos) / dt;
        prevWristPos = wristPos; prevTime = t;
        return speed;
    }

    bool TryGetPose(XRHand hand, XRHandJointID id, out Pose pose)
    {
        var j = hand.GetJoint(id);
        return j.TryGetPose(out pose);
    }

    float AverageTipDistanceToPalm(XRHand hand, Vector3 palmPos)
    {
        XRHandJointID[] tips = {
            XRHandJointID.ThumbTip, XRHandJointID.IndexTip, XRHandJointID.MiddleTip,
            XRHandJointID.RingTip, XRHandJointID.LittleTip
        };
        int count = 0; float sum = 0f;
        foreach (var id in tips)
        {
            var j = hand.GetJoint(id);
            if (j.TryGetPose(out var p)) { sum += Vector3.Distance(p.position, palmPos); count++; }
        }
        return count >= 3 ? sum / count : -1f;
    }

    void TryGetHandSubsystem()
    {
        var list = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(list);
        if (list.Count > 0) handSubsystem = list[0];
    }
}
