// BookPageDragInteractor.cs (v4: Ray命中点驱动)
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BookPageDragInteractor : MonoBehaviour
{
    [Header("XRSimpleInteractable（填在外侧把手上）")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable leftInteractable;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable rightInteractable;

    [Header("可视脚本（仍然转Pivot）")]
    public BookFlipVisual flipVisual;

    [Header("坐标基准（通常拖 Book）")]
    public Transform bookRoot;

    [Header("拖动映射")]
    public float dragDistance = 0.18f;              // 单页宽度≈书脊到外缘的本地X距离

    [Header("翻页判定（角度优先）")]
    public bool useAngleThreshold = true;
    [Range(0f, 180f)] public float commitAngleDeg = 120f;
    [Range(0.2f, 0.9f)] public float commitProgress = 0.5f;

    [Header("手感")]
    [Range(0f, 0.2f)] public float minLiftOnSelect = 0.04f;

    bool isDragging = false;
    bool dragIsRightPage = false;
    UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    Vector3 startLocalPoint; // 起始“参考点”（Ray=命中点，Direct=手位置）
    float startProgress;

    void OnEnable()
    {
        if (rightInteractable)
        {
            rightInteractable.selectEntered.AddListener(a => BeginDrag(a, true, flipVisual ? flipVisual.rightProgress : 0f));
            rightInteractable.selectExited.AddListener(a => EndDrag(true, flipVisual ? flipVisual.rightProgress : 0f));
        }
        if (leftInteractable)
        {
            leftInteractable.selectEntered.AddListener(a => BeginDrag(a, false, flipVisual ? flipVisual.leftProgress : 0f));
            leftInteractable.selectExited.AddListener(a => EndDrag(false, flipVisual ? flipVisual.leftProgress : 0f));
        }
    }
    void OnDisable()
    {
        if (rightInteractable)
        {
            rightInteractable.selectEntered.RemoveAllListeners();
            rightInteractable.selectExited.RemoveAllListeners();
        }
        if (leftInteractable)
        {
            leftInteractable.selectEntered.RemoveAllListeners();
            leftInteractable.selectExited.RemoveAllListeners();
        }
    }

    void Update()
    {
        if (!isDragging || flipVisual == null || currentInteractor == null) return;

        Vector3 curWorld = GetInteractorPointWorld(currentInteractor, out bool ok);
        if (!ok) return;

        Vector3 curLocal = ToBookLocal(curWorld);
        float dx = curLocal.x - startLocalPoint.x;

        float dir = dragIsRightPage ? -1f : 1f; // 右页向左拖为正；左页向右拖为正
        float delta = (dx / Mathf.Max(0.001f, dragDistance)) * dir;

        float p = Mathf.Clamp01(startProgress + delta);
        flipVisual.SetDragProgress(dragIsRightPage, p);
    }

    void BeginDrag(SelectEnterEventArgs args, bool isRight, float currentProgress)
    {
        if (!flipVisual || !bookRoot) return;

        dragIsRightPage = isRight;
        isDragging = true;
        currentInteractor = args.interactorObject;

        Vector3 world = GetInteractorPointWorld(currentInteractor, out bool ok);
        if (!ok) return;

        startLocalPoint = ToBookLocal(world);
        startProgress = Mathf.Max(currentProgress, minLiftOnSelect);
        flipVisual.SetDragProgress(dragIsRightPage, startProgress);
    }

    void EndDrag(bool isRight, float progressNow)
    {
        isDragging = false;
        currentInteractor = null;

        // 用可视脚本里的最大翻页角度做一致的判定
        float maxAngle = (flipVisual != null) ? flipVisual.maxFlipAngleDeg : 180f;
        float angleNow = Mathf.Clamp01(progressNow) * maxAngle;

        bool commit = useAngleThreshold
            ? (angleNow >= commitAngleDeg)      // 角度阈值（建议 ≤ maxAngle）
            : (progressNow >= commitProgress);  // 备用：进度阈值

        if (flipVisual == null) return;
        flipVisual.AnimateTo(isRight, commit ? 1f : 0f);
    }


    // ★ 核心：拿“交互点”世界坐标——Ray 用命中点，Direct 用Attach/Transform
    Vector3 GetInteractorPointWorld(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor inter, out bool ok)
    {
        ok = false;
        // Ray交互：命中点
        if (inter is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray)
        {
            if (ray.TryGetHitInfo(out var pos, out _, out _, out bool valid) && valid)
            {
                ok = true; return pos;
            }
        }
        // 近距交互：用Attach Transform或自身transform
        var t = inter?.GetAttachTransform(null);
        if (t != null) { ok = true; return t.position; }
        if (inter is Component c) { ok = true; return c.transform.position; }
        return Vector3.zero;
    }

    Vector3 ToBookLocal(Vector3 worldPos)
        => bookRoot ? bookRoot.InverseTransformPoint(worldPos) : worldPos;
}
