using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using System;

public class TeDelayAction : MonoBehaviour
{
    [Header("目标物体（A）：切换渲染器")]
    [Tooltip("物体 A 在状态 A（初始 / 回退）时应启用的渲染器。")]
    [SerializeField] private Renderer rendererA;

    [Tooltip("物体 A 在状态 B（延时后切换）时应启用的渲染器。")]
    [SerializeField] private Renderer rendererB;

    [Header("联动与场景")]
    [Tooltip("联动对象上表示「状态 C」的渲染器：启用 (enabled) 即视为处于 C。不为 C 时进入下一场景；为 C 时切回 A/B。")]
    [FormerlySerializedAs("otherConditionRenderer")]
    [SerializeField] private Renderer otherRendererC;

    [Tooltip("满足条件时加载的场景名（在 Build Settings 中添加）。")]
    [SerializeField] private string nextSceneName = "";

    [Tooltip("满足条件时加载的场景索引（若 ≥0 则优先使用，否则使用 nextSceneName）。")]
    [SerializeField] private int nextSceneBuildIndex = -1;

    [Header("触发规则")]
    [Tooltip("如果在等待期间再次按数字键，则是否取消前一次等待（用最后一次输入为准）。")]
    [SerializeField] private bool cancelPrevious = true;

    [Tooltip("切到渲染器 B 后停留多久再检测/回退。设为 0 表示至少停留一帧。")]
    [SerializeField] private float stayOnBSeconds = 0f;

    [Tooltip("调试用：在 Console 打印触发的数字键。")]
    [SerializeField] private bool logKeyTrigger = false;

    [Header("切换到 B 时音频")]
    [Tooltip("切到渲染器 B 时播放的音频片段。")]
    [SerializeField] private AudioClip musicOnB;

    [Tooltip("用于播放的 AudioSource。留空则在本物体上自动获取或添加。")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("是否循环播放。")]
    [SerializeField] private bool loopMusicOnB = false;

    [Tooltip("切到 B 时若正在播放其它片段，是否先 Stop 再播放本段。")]
    [SerializeField] private bool stopAudioBeforePlayOnB = true;

    private Coroutine _running;
    private InputAction[] _numpadActions;

    private void Awake()
    {
        if (rendererA == null || rendererB == null)
        {
            var r = GetComponents<Renderer>();
            if (r != null && r.Length >= 2)
            {
                if (rendererA == null) rendererA = r[0];
                if (rendererB == null) rendererB = r[1];
            }
        }
    }

    private void Reset()
    {
        var r = GetComponents<Renderer>();
        if (r != null && r.Length >= 2)
        {
            rendererA = r[0];
            rendererB = r[1];
        }
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
        if (rendererA == null || rendererB == null)
        {
            Debug.LogWarning($"{nameof(TeDelayAction)}：请设置 rendererA 与 rendererB。");
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

        // 1) 物体 A：显示 B（切换渲染器）
        rendererA.enabled = false;
        rendererB.enabled = true;
        PlayMusicOnSwitchToB();

        // 2) 在切换到 B 的瞬间：联动对象渲染器启用 = 处于 C，否则 = 不为 C
        bool isOtherInStateC = otherRendererC != null && otherRendererC.enabled;

        // 3) 不为 C → 进下一场景；为 C → 停留后切回渲染器 A
        if (!isOtherInStateC)
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
            if (stayOnBSeconds > 0f)
                yield return new WaitForSeconds(stayOnBSeconds);
            else
                yield return null;

            rendererA.enabled = true;
            rendererB.enabled = false;
        }

        _running = null;
    }

    private void PlayMusicOnSwitchToB()
    {
        if (musicOnB == null) return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (stopAudioBeforePlayOnB && audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = musicOnB;
        audioSource.loop = loopMusicOnB;
        audioSource.Play();
    }
}
