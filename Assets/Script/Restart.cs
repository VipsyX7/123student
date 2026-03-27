using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 按下 R 键重新开始：默认重载当前场景，并恢复 Time.timeScale。
/// 将脚本挂在任意常驻物体上（例如 DontDestroy 的管理器或玩家物体）。
/// </summary>
public class Restart : MonoBehaviour
{
    [Tooltip("若为 true，重载当前正在玩的场景；若为 false，则加载下面指定的 buildIndex。")]
    [SerializeField] private bool reloadActiveScene = false;

    [Tooltip("当 reloadActiveScene 为 false 时使用：要加载的场景在 Build Settings 里的索引。")]
    [SerializeField] private int restartSceneBuildIndex = 0;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!kb.rKey.wasPressedThisFrame)
            return;

        Time.timeScale = 1f;

        if (reloadActiveScene)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(restartSceneBuildIndex);
    }
}
