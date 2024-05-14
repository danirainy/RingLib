using RingLib.StateMachine;
using UnityEngine;

namespace RingLib.Components;

internal class Animator : MonoBehaviour
{
    private UnityEngine.Animator animator;
    private Dictionary<string, float> clipLength = [];

    private string currentAnimation;
    private bool finished;

    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<UnityEngine.Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            clipLength[clip.name] = clip.isLooping ? float.MaxValue : clip.length;
        }
        audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (currentAnimation == null || clipLength[currentAnimation] == float.MaxValue)
        {
            return;
        }
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1)
        {
            finished = true;
        }
    }

    public float ClipLength(string clipName)
    {
        return clipLength[clipName];
    }

    public IEnumerator<Transition> PlayAnimation(string clipName)
    {
        if (!clipLength.ContainsKey(clipName))
        {
            Log.LogError(GetType().Name, "Animation not found");
            return null;
        }
        finished = false;
        currentAnimation = clipName;
        animator.Play(clipName, -1, 0);

        IEnumerator<Transition> routine()
        {
            while (!finished)
            {
                yield return new CurrentState();
            }
        }
        return routine();
    }

    protected void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
