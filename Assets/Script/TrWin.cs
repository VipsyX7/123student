using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrWin : MonoBehaviour
{
    [Header("计时与场景")]
    [Tooltip("游戏开始后需要等待的秒数，到点后切换场景。")]
    [Min(0f)]
    [SerializeField] private float requiredSeconds = 2f;

    [Tooltip("切换场景索引（若 >=0 则优先使用）。")]
    [SerializeField] private int nextSceneBuildIndex = -1;

    [Tooltip("切换场景名（若 nextSceneBuildIndex < 0 则使用）。")]
    [SerializeField] private string nextSceneName = "";

    [Header("计时可视化（可选）")]
    [Tooltip("进度条（0~1）。把 UI Slider 拖进来即可。")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("文字显示（例如：1.23 / 2.00s）。把 UI Text 拖进来即可。")]
    [SerializeField] private Text progressText;

    private bool _loaded;
    private float _elapsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_loaded) return;
        _elapsed = 0f;
        StartCoroutine(RunTimerAndLoad());
    }

    private System.Collections.IEnumerator RunTimerAndLoad()
    {
        // 使用逐帧计时，以便实时更新进度可视化。
        if (requiredSeconds > 0f)
        {
            while (_elapsed < requiredSeconds)
            {
                _elapsed += Time.deltaTime;
                UpdateVisuals();
                yield return null;
            }
        }
        else
        {
            _elapsed = requiredSeconds;
            UpdateVisuals();
        }

        if (_loaded) yield break;
        _loaded = true;

        if (nextSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
            yield break;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        Debug.LogWarning($"{nameof(TrWin)}：未设置 nextSceneBuildIndex 或 nextSceneName，无法切换场景。");
        _loaded = false;
    }

    private void UpdateVisuals()
    {
        float denom = Mathf.Max(requiredSeconds, 0.0001f);
        float t = Mathf.Clamp01(_elapsed / denom);

        if (progressSlider != null)
            progressSlider.value = t;

        if (progressText != null)
            progressText.text = $"{_elapsed:F2} / {requiredSeconds:F2}s";
    }
}
