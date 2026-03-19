using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StWin : MonoBehaviour
{
    [Header("检测目标")]
    [Tooltip("要检测是否处于材质C的物体渲染器。")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("目标材质C。")]
    [SerializeField] private Material materialC;

    [Tooltip("渲染器材质数组中的槽位（一般为0）。")]
    [SerializeField] private int materialIndex = 0;

    [Header("计时与场景")]
    [Tooltip("处于材质C时需要累计达到的秒数。")]
    [Min(0f)]
    [SerializeField] private float requiredSeconds = 2f;

    [Header("计时可视化（可选）")]
    [Tooltip("进度条（0~1）。把 UI Slider 拖进来即可。")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("文字显示（例如：1.23 / 2.00s）。把 UI Text 拖进来即可。")]
    [SerializeField] private Text progressText;

    [Tooltip("满足条件时加载的场景名（在 Build Settings 中添加）。")]
    [SerializeField] private string nextSceneName = "";

    [Tooltip("满足条件时加载的场景索引（若 ≥0 则优先使用，否则使用 nextSceneName）。")]
    [SerializeField] private int nextSceneBuildIndex = -1;

    [Tooltip("若离开材质C，是否将累计计时清零。")]
    [SerializeField] private bool resetTimerWhenNotC = true;

    [Tooltip("调试用：打印累计时间。")]
    [SerializeField] private bool logTimer = false;

    private float _elapsed;
    private bool _loaded;

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (_loaded) return;
        if (targetRenderer == null || materialC == null) return;

        var mats = targetRenderer.sharedMaterials;
        bool isC =
            mats != null &&
            mats.Length > 0 &&
            materialIndex >= 0 &&
            materialIndex < mats.Length &&
            mats[materialIndex] == materialC;

        if (isC)
        {
            _elapsed += Time.deltaTime;
            if (logTimer) Debug.Log($"{nameof(StWin)}：累计 {_elapsed:F2}/{requiredSeconds:F2}s");

            UpdateVisuals();

            if (_elapsed >= requiredSeconds)
            {
                _loaded = true;
                if (nextSceneBuildIndex >= 0)
                    SceneManager.LoadScene(nextSceneBuildIndex);
                else if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
                else
                {
                    Debug.LogWarning($"{nameof(StWin)}：未设置 nextSceneName 或 nextSceneBuildIndex，无法切换场景。");
                    _loaded = false;
                }
            }
        }
        else
        {
            if (resetTimerWhenNotC) _elapsed = 0f;
            UpdateVisuals();
        }
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
