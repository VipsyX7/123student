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

    [Header("B/C/D 各自音乐（按住空格显示对应渲染器时播放）")]
    [Tooltip("显示 B 时播放的音频。")]
    [SerializeField] private AudioClip musicB;

    [Tooltip("显示 C 时播放的音频。")]
    [SerializeField] private AudioClip musicC;

    [Tooltip("显示 D 时播放的音频。")]
    [SerializeField] private AudioClip musicD;

    [Tooltip("用于播放的 AudioSource。留空则在本物体上自动获取或添加。")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("B/C/D 音乐是否循环，直到松开空格回到 A。")]
    [SerializeField] private bool loopBcdMusic = true;

    /// <summary>0=B, 1=C, 2=D，在空格按下的那一帧选定，按住期间不变。</summary>
    private int _pickedBcdIndex;

    /// <summary>-1 表示当前为 A（或未在播 B/C/D 专用曲）；0/1/2 对应 B/C/D。</summary>
    private int _lastBcdMusicIndex = -1;

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

            if (_pickedBcdIndex != _lastBcdMusicIndex)
            {
                PlayMusicForBcdIndex(_pickedBcdIndex);
                _lastBcdMusicIndex = _pickedBcdIndex;
            }
        }
        else
        {
            rendererA.enabled = true;
            rendererB.enabled = false;
            rendererC.enabled = false;
            rendererD.enabled = false;

            StopBcdMusic();
            _lastBcdMusicIndex = -1;
        }
    }

    private void PlayMusicForBcdIndex(int index)
    {
        AudioClip clip = index switch
        {
            0 => musicB,
            1 => musicC,
            2 => musicD,
            _ => null
        };

        if (clip == null)
        {
            StopBcdMusic();
            return;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (audioSource.isPlaying && audioSource.clip == clip)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loopBcdMusic;
        audioSource.Play();
    }

    private void StopBcdMusic()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying)
            audioSource.Stop();
        audioSource.clip = null;
    }
}
