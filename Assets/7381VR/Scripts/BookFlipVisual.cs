using System.Collections;
using UnityEngine;

public class BookFlipVisual : MonoBehaviour
{
    [Header("Pivots（指向 LeftPagePivot / RightPagePivot）")]
    public Transform leftPagePivot;
    public Transform rightPagePivot;

    [Header("Page Renderers（把四个 plane 的 Renderer 拖进来）")]
    public Renderer leftFront;   // 当前摊开的左页正面
    public Renderer leftBack;    // 左页背面（=上一页右侧）
    public Renderer rightFront;  // 当前摊开的右页正面
    public Renderer rightBack;   // 右页背面（=下一页左侧）

    [Header("All Pages（从 0 开始，0=左，1=右，2=左，3=右……）")]
    public Texture2D[] pages;

    [Tooltip("当前摊开的左页索引（建议为偶数）。脚本会自动修正为偶数。")]
    public int leftPageIndex = 0;

    [Header("Flip Progress（0-1，由交互脚本驱动）")]
    [Range(0f, 1f)] public float leftProgress;   // 左页向右翻 0→1
    [Range(0f, 1f)] public float rightProgress;  // 右页向左翻 0→1

    [Header("Animation")]
    public float snapSpeed = 10f;                // 松手后的补间速度
    public Vector3 flipAxis = Vector3.up;        // 绕Y轴翻


    [Header("Book Space (用来确定书的'正面'方向)")]
    public Transform bookRoot;                 // 一般拖 Book
    public bool forceFlipToFront = true;       // 强制只在书的“正面半空间”翻
    public bool invertRightAngle = false;      // 右页角度反向开关（万一你模型轴相反）
    public bool invertLeftAngle = false;      // 左页角度反向开关
    public bool invertFrontDir = false;      // 正面方向反向（book.forward 反了时勾上）


    [Header("Flip Angle")]
    [Range(0f, 179f)] public float maxFlipAngleDeg = 120f;   // 你想要的最大跨度


    [Header("Lift/Offset（抬页弧度）")]
    public float liftUp = 0.006f;         // 垂直抬高（米）
    public float liftForward = 0.020f;         // 朝“书的正面”抬出（米）


    [Header("Optional Under Pages (static planes, not under pivots)")]
    public Renderer leftUnder;   // 显示 L-2
    public Renderer rightUnder;  // 显示 L+3


    // 初始姿态（防止别处改动导致累积误差时可用于还原；目前未使用）
    Quaternion _leftInitRot, _rightInitRot;

    void Awake()
    {
        // 确保左页索引为偶数
        if ((leftPageIndex & 1) == 1) leftPageIndex -= 1;
    }

    Vector3 _leftInitPos, _rightInitPos;

    void Start()
    {
        if (leftPagePivot)
        {
            _leftInitPos = leftPagePivot.localPosition;
            _leftInitRot = leftPagePivot.localRotation;
        }
        if (rightPagePivot)
        {
            _rightInitPos = rightPagePivot.localPosition;
            _rightInitRot = rightPagePivot.localRotation;
        }
        AssignSpread(leftPageIndex);
    }

    System.Collections.IEnumerator CoDemoFlip(bool right)
    {
        // 翻到1，再回0
        yield return AnimateTo(right, 1f);
        yield return new WaitForSeconds(0.4f);
        yield return AnimateTo(right, 0f);
    }

    void LateUpdate()
    {
        // 本地坐标系里的“正面方向”和“向上方向”
        // 注意：我们要给 localPosition 赋值，所以需要把 world 的 forward/up 转成“父物体的局部方向”
        Vector3 frontL = Vector3.forward;
        Vector3 frontR = Vector3.forward;
        Vector3 upL = Vector3.up;
        Vector3 upR = Vector3.up;

        if (leftPagePivot && leftPagePivot.parent && bookRoot)
        {
            frontL = leftPagePivot.parent.InverseTransformDirection(bookRoot.forward).normalized;
            upL = leftPagePivot.parent.InverseTransformDirection(bookRoot.up).normalized;
        }
        if (rightPagePivot && rightPagePivot.parent && bookRoot)
        {
            frontR = rightPagePivot.parent.InverseTransformDirection(bookRoot.forward).normalized;
            upR = rightPagePivot.parent.InverseTransformDirection(bookRoot.up).normalized;
        }

        if (invertFrontDir) { frontL = -frontL; frontR = -frontR; }

        // ---- 右页：0→1 映射到 0→-180（可反向） ----
        if (rightPagePivot)
        {
            float angR = Mathf.Lerp(0f, -maxFlipAngleDeg, rightProgress);
            if (invertRightAngle) angR = -angR;

            float s = Mathf.Sin(Mathf.PI * Mathf.Clamp01(rightProgress)); // 0→1→0 的抬起权重

            // 只在“书的正面半空间”抬起：前向位移始终非负
            float forwardDisp = forceFlipToFront ? Mathf.Abs(s) * liftForward : s * liftForward;

            rightPagePivot.localRotation = _rightInitRot * Quaternion.AngleAxis(angR, Vector3.up);
            rightPagePivot.localPosition = _rightInitPos
                                         + upR * (s * liftUp)
                                         + frontR * forwardDisp;
        }

        // ---- 左页：0→1 映射到 0→+180（可反向） ----
        if (leftPagePivot)
        {
            float angL = Mathf.Lerp(0f, +maxFlipAngleDeg, leftProgress);
            if (invertLeftAngle) angL = -angL;

            float s = Mathf.Sin(Mathf.PI * Mathf.Clamp01(leftProgress));
            float forwardDisp = forceFlipToFront ? Mathf.Abs(s) * liftForward : s * liftForward;

            leftPagePivot.localRotation = _leftInitRot * Quaternion.AngleAxis(angL, Vector3.up);
            leftPagePivot.localPosition = _leftInitPos
                                        + upL * (s * liftUp)
                                        + frontL * forwardDisp;
        }
    }


    // =============== 外部调用：交互脚本实时写入进度 ===============
    public void SetDragProgress(bool isRightPage, float progress01)
    {
        if (isRightPage) rightProgress = Mathf.Clamp01(progress01);
        else leftProgress = Mathf.Clamp01(progress01);
    }

    // =============== 外部调用：松手后的补间（到 0 或 1） ===============
    public Coroutine AnimateTo(bool isRightPage, float target01)
    {
        return StartCoroutine(CoAnimate(isRightPage, target01));
    }

    IEnumerator CoAnimate(bool isRightPage, float target)
    {
        if (isRightPage)
        {
            while (Mathf.Abs(rightProgress - target) > 0.001f)
            {
                rightProgress = Mathf.MoveTowards(rightProgress, target, Time.deltaTime * snapSpeed);
                yield return null;
            }
            rightProgress = target;

            if (Mathf.Approximately(target, 1f))
            {
                // 右页落到左侧 => 下一组（+2）
                leftPageIndex = Mathf.Min(GetMaxLeftEvenIndex(), leftPageIndex + 2);
                AssignSpread(leftPageIndex);
                rightProgress = 0f; // 复位
            }
        }
        else
        {
            while (Mathf.Abs(leftProgress - target) > 0.001f)
            {
                leftProgress = Mathf.MoveTowards(leftProgress, target, Time.deltaTime * snapSpeed);
                yield return null;
            }
            leftProgress = target;

            if (Mathf.Approximately(target, 1f))
            {
                // 左页落到右侧 => 上一组（-2）
                leftPageIndex = Mathf.Max(0, leftPageIndex - 2);
                AssignSpread(leftPageIndex);
                leftProgress = 0f; // 复位
            }
        }
    }

    // =============== 贴图分配的唯一入口 ===============
    // spreadLeft = 当前摊开的左页索引（偶数）
    void AssignSpread(int spreadLeft)
    {
        int L = Mathf.Clamp(spreadLeft, 0, GetMaxLeftEvenIndex());
        int R = L + 1;

        // 前面（你正面对着看的两页）
        SetTex(leftFront, GetPageTex(L));
        SetTex(rightFront, GetPageTex(R));

        // 背面（翻动过程中会看到）
        SetTex(leftBack, GetPageTex(L - 1)); // 上一页的右面
        SetTex(rightBack, GetPageTex(L + 2)); // 下一页的左面

        if (leftUnder)
        {
            var tex = GetPageTex(L - 2);
            SetTex(leftUnder, tex);
            leftUnder.enabled = tex != null;  // 越界就隐藏
        }
        if (rightUnder)
        {
            var tex = GetPageTex(L + 3);
            SetTex(rightUnder, tex);
            rightUnder.enabled = tex != null;
        }
    }

    // =============== 工具方法 ===============
    Texture GetPageTex(int index)
    {
        if (pages == null || pages.Length == 0) return null;
        if (index < 0 || index >= pages.Length) return null;
        return pages[index];
    }

    int GetMaxLeftEvenIndex()
    {
        if (pages == null || pages.Length == 0) return 0;
        // 最大可用左页：确保 L 和 L+1 都在范围内
        int maxRight = pages.Length - 1;
        int maxLeft = Mathf.Max(0, maxRight - 1);
        // 向下取偶数
        if ((maxLeft & 1) == 1) maxLeft -= 1;
        return Mathf.Max(0, maxLeft);
    }

    void SetTex(Renderer r, Texture t)
    {
        if (!r) return;
        var m = r.material;            // 实例材质（不影响别的对象）
        if (!m) return;

        // 兼容 URP Lit（_BaseMap）与 Standard（_MainTex）
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
        else if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", t);
        // 如有自定义shader，请在此扩展其它属性名
    }
}
