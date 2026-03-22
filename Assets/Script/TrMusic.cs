using System.Collections;
using UnityEngine;

/// <summary>
/// 进入场景时：将指定物体在若干秒内平滑移动到目标位置，并播放一段音乐。
/// 将本脚本挂在任意场景物体上，在 Inspector 中指定要移动的物体、目标与音频即可。
/// </summary>
public class TrMusic : MonoBehaviour
{
    [Header("移动")]
    [Tooltip("要缓慢移动的物体（其 Transform.position 会被修改）。")]
    [SerializeField] private Transform objectToMove;

    [Tooltip("若指定，则移动到该 Transform 的世界坐标；否则使用下面的 targetWorldPosition。")]
    [SerializeField] private Transform destination;

    [Tooltip("当 destination 为空时使用的目标世界坐标。")]
    [SerializeField] private Vector3 targetWorldPosition;

    [Tooltip("从起点移动到终点所用时间（秒），数值越大越慢。")]
    [Min(0.01f)]
    [SerializeField] private float moveDuration = 3f;

    [Tooltip("是否使用 SmoothStep 缓动（先慢后慢）；关闭则为匀速线性插值。")]
    [SerializeField] private bool useSmoothStep = true;

    [Header("音乐")]
    [Tooltip("进入场景时播放的音频片段。")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("用于播放的 AudioSource。留空则在本物体上自动获取或添加一个。")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("是否循环播放。")]
    [SerializeField] private bool loopMusic = false;

    private void Start()
    {
        if (objectToMove == null)
        {
            Debug.LogWarning($"{nameof(TrMusic)}：未指定 objectToMove，跳过移动与音乐。");
            return;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.loop = loopMusic;
            audioSource.Play();
        }

        StartCoroutine(MoveRoutine());
    }

    private Vector3 GetTargetPosition()
    {
        return destination != null ? destination.position : targetWorldPosition;
    }

    private IEnumerator MoveRoutine()
    {
        Vector3 start = objectToMove.position;
        Vector3 end = GetTargetPosition();
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / moveDuration);
            if (useSmoothStep)
                u = u * u * (3f - 2f * u);

            objectToMove.position = Vector3.Lerp(start, end, u);
            yield return null;
        }

        objectToMove.position = end;
    }
}
