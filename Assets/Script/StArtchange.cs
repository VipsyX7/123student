using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class StArtchange : MonoBehaviour
{
    [Header("渲染器：松开空格为 A，按住空格在 B/C/D 中随机其一")]
    [Tooltip("松开空格时启用的渲染器。")]
    [FormerlySerializedAs("targetRenderer")]
    [SerializeField] private Renderer rendererA;

    [Tooltip("按住空格时可能随机选中的渲染器之一。")]
    [SerializeField] private Renderer rendererB;

    [SerializeField] private Renderer rendererC;

    [SerializeField] private Renderer rendererD;

    /// <summary>0=B, 1=C, 2=D，在空格按下的那一帧选定，按住期间不变。</summary>
    private int _pickedBcdIndex;

    private void Reset()
    {
        rendererA = GetComponent<Renderer>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (rendererA == null || rendererB == null || rendererC == null || rendererD == null) return;

        if (kb.spaceKey.wasPressedThisFrame)
            _pickedBcdIndex = Random.Range(0, 3);

        if (kb.spaceKey.isPressed)
        {
            rendererA.enabled = false;
            rendererB.enabled = _pickedBcdIndex == 0;
            rendererC.enabled = _pickedBcdIndex == 1;
            rendererD.enabled = _pickedBcdIndex == 2;
        }
        else
        {
            rendererA.enabled = true;
            rendererB.enabled = false;
            rendererC.enabled = false;
            rendererD.enabled = false;
        }
    }
}
