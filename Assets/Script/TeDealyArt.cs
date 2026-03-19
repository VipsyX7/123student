using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class TeDelayAction : MonoBehaviour
{
    [Header("目标物体（A）与材质")]
    [Tooltip("物体A：要切换材质的渲染器（例如 SpriteRenderer / MeshRenderer）。")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("物体A的材质A（初始材质 / 回退材质）。不填则运行时自动记录当前材质为A。")]
    [SerializeField] private Material materialA;

    [Tooltip("物体A的材质B（延时后切换到此材质）。")]
    [SerializeField] private Material materialB;

    [Tooltip("物体A替换渲染器材质数组中的哪个槽位（0 通常对应第一个材质）。")]
    [SerializeField] private int materialIndex = 0;

    [Header("联动与场景")]
    [Tooltip("用于判断的另一个物体：当其处于材质C时才进入下一场景，否则物体A恢复为材质A。")]
    [SerializeField] private Renderer otherObjectRenderer;

    [Tooltip("另一个物体必须处于的材质C。")]
    [SerializeField] private Material materialC;

    [Tooltip("另一个物体渲染器的材质槽位（用于判断是否为材质 C）。")]
    [SerializeField] private int otherMaterialIndex = 0;

    [Tooltip("满足条件时加载的场景名（在 Build Settings 中添加）。")]
    [SerializeField] private string nextSceneName = "";

    [Tooltip("满足条件时加载的场景索引（若 ≥0 则优先使用，否则使用 nextSceneName）。")]
    [SerializeField] private int nextSceneBuildIndex = -1;

    [Header("触发规则")]
    [Tooltip("如果在等待期间再次按数字键，则是否取消前一次等待（用最后一次输入为准）。")]
    [SerializeField] private bool cancelPrevious = true;

    [Tooltip("切到材质B后停留多久再检测/回退。设为 0 表示至少停留一帧。")]
    [SerializeField] private float stayOnBSeconds = 0f;

    [Tooltip("调试用：在 Console 打印触发的数字键。")]
    [SerializeField] private bool logKeyTrigger = false;

    private Coroutine _running;
    private InputAction[] _numpadActions;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // 若未手动指定材质A，则启动时记录当前材质作为材质A（回退用）
        if (materialA == null && targetRenderer != null)
        {
            var mats = targetRenderer.sharedMaterials;
            if (mats != null && mats.Length > 0 && materialIndex >= 0 && materialIndex < mats.Length)
                materialA = mats[materialIndex];
        }
    }

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        EnsureNumpadActions();
        for (int i = 0; i < _numpadActions.Length; i++)
        {
            // 对 Button 来说 started 在“按下瞬间”更稳定
            _numpadActions[i].started += OnNumpadStarted;
            _numpadActions[i].Enable();
        }
    }

    private void OnDisable()
    {
        if (_numpadActions == null) return;
        for (int i = 0; i < _numpadActions.Length; i++)
        {
            _numpadActions[i].started -= OnNumpadStarted;
            _numpadActions[i].Disable();
        }
    }

    private void EnsureNumpadActions()
    {
        if (_numpadActions != null) return;
        _numpadActions = new InputAction[10];
        for (int i = 0; i <= 9; i++)
        {
            var action = new InputAction(name: $"Num{i}", type: InputActionType.Button);
            action.AddBinding($"<Keyboard>/numpad{i}");
            action.AddBinding($"<Keyboard>/digit{i}");
            _numpadActions[i] = action;
        }
    }

    private void OnNumpadStarted(InputAction.CallbackContext context)
    {
        if (_numpadActions == null) return;
        int seconds = Array.IndexOf(_numpadActions, context.action);
        if (seconds >= 0)
        {
            if (logKeyTrigger) Debug.Log($"{nameof(TeDelayAction)}：触发数字键 {seconds}");
            Trigger(seconds);
        }
    }

    private void Trigger(int seconds)
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{nameof(TeDelayAction)}：未设置 targetRenderer。");
            return;
        }

        if (materialB == null)
        {
            Debug.LogWarning($"{nameof(TeDelayAction)}：未设置 materialB。");
            return;
        }

        if (cancelPrevious && _running != null)
            StopCoroutine(_running);

        _running = StartCoroutine(RunSequence(seconds));
    }

    private System.Collections.IEnumerator RunSequence(int seconds)
    {
        if (seconds > 0)
            yield return new WaitForSeconds(seconds);

        // 1) 物体A：材质A -> 材质B
        var mats = targetRenderer.materials;
        if (mats == null || mats.Length == 0)
        {
            Debug.LogWarning($"{nameof(TeDelayAction)}：targetRenderer 没有材质。");
            _running = null;
            yield break;
        }

        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
            Debug.LogWarning($"{nameof(TeDelayAction)}：materialIndex={materialIndex} 超出材质数量范围（数量={mats.Length}）。");
            _running = null;
            yield break;
        }

        mats[materialIndex] = materialB;
        targetRenderer.materials = mats;

        // 让材质B至少显示一帧（或指定时长），否则立即回退会看起来像“没切换”
        if (stayOnBSeconds > 0f)
            yield return new WaitForSeconds(stayOnBSeconds);
        else
            yield return null;

        // 2) 检测另一物体是否为材质C
        bool otherIsOnMaterialC = false;
        if (otherObjectRenderer != null && materialC != null)
        {
            var otherMats = otherObjectRenderer.sharedMaterials;
            if (otherMats != null && otherMaterialIndex >= 0 && otherMaterialIndex < otherMats.Length)
                otherIsOnMaterialC = otherMats[otherMaterialIndex] == materialC;
        }

        // 3) 若为C：进入下一场景；否则：物体A从B切回A
        if (otherIsOnMaterialC)
        {
            if (nextSceneBuildIndex >= 0)
                SceneManager.LoadScene(nextSceneBuildIndex);
            else if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                Debug.LogWarning($"{nameof(TeDelayAction)}：未设置 nextSceneName 或 nextSceneBuildIndex，无法切换场景。");
        }
        else
        {
            if (materialA != null)
            {
                mats[materialIndex] = materialA;
                targetRenderer.materials = mats;
            }
        }

        _running = null;
    }
}
