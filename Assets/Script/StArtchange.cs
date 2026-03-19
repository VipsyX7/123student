using UnityEngine;
using UnityEngine.InputSystem;

public class StArtchange : MonoBehaviour
{
    [Header("目标物体与材质")]
    [Tooltip("要切换材质的渲染器（例如 SpriteRenderer / MeshRenderer）。")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("默认材质（松开空格时使用）。")]
    [SerializeField] private Material materialA;

    [Tooltip("按住空格时使用的材质。")]
    [SerializeField] private Material materialB;

    [Tooltip("替换渲染器材质数组中的哪个槽位（0 通常为第一个）。")]
    [SerializeField] private int materialIndex = 0;

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || targetRenderer == null) return;
        if (materialA == null || materialB == null) return;

        var mats = targetRenderer.materials;
        if (mats == null || mats.Length == 0 || materialIndex < 0 || materialIndex >= mats.Length) return;

        Material target = kb.spaceKey.isPressed ? materialB : materialA;
        if (mats[materialIndex] != target)
        {
            mats[materialIndex] = target;
            targetRenderer.materials = mats;
        }
    }
}
