using UnityEngine;

namespace RingLib;

internal class Animator : MonoBehaviour
{
    public bool Finished;
    private UnityEngine.Animator animator;
    private Dictionary<string, float> clipLengths = [];
    private string currentAnimation;
    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<UnityEngine.Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            clipLengths[clip.name] = clip.isLooping ? float.MaxValue : clip.length;
        }
        audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (currentAnimation == null)
        {
            return;
        }
        if (clipLengths[currentAnimation] == float.MaxValue)
        {
            return;
        }
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1)
        {
            Finished = true;
        }
    }

    public float PlayAnimation(string clipName)
    {
        if (!clipLengths.ContainsKey(clipName))
        {
            Log.LogError(GetType().Name, "Animation not found");
            return float.MaxValue;
        }
        Finished = false;
        currentAnimation = clipName;
        animator.Play(clipName, -1, 0);
        return clipLengths[clipName];
    }

    protected void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
