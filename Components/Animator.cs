using RingLib.StateMachine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RingLib.Components;

internal class Animator : MonoBehaviour
{
    private UnityEngine.Animator animator;
    private Dictionary<string, float> clipLength = [];

    private string currentAnimation;
    private bool finished;

    private AudioSource audioSource;

    protected virtual void AnimatorStart() { }

    private void Start()
    {
        animator = GetComponent<UnityEngine.Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            clipLength[clip.name] = clip.isLooping ? float.MaxValue : clip.length;
        }
        audioSource = GetComponent<AudioSource>();
        AnimatorStart();
    }

    private float NormalizedTime()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(currentAnimation))
        {
            return Mathf.Min(1, stateInfo.normalizedTime);
        }
        return 0;
    }

    private void Update()
    {
        if (currentAnimation == null || clipLength[currentAnimation] == float.MaxValue)
        {
            return;
        }
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (NormalizedTime() == 1)
        {
            finished = true;
        }
    }

    public IEnumerator<Transition> PlayAnimation(string clipName, Func<float, Transition> updater = null)
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
                if (updater != null)
                {
                    var tansition = updater(NormalizedTime());
                    if (tansition == null)
                    {
                        break;
                    }
                    yield return tansition;
                }
                yield return new NoTransition();
            }
        }
        return routine();
    }

    public float ClipLength(string clipName)
    {
        return clipLength[clipName];
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Log.LogError(GetType().Name, "Clip not found");
            return;
        }
        audioSource.PlayOneShot(clip);
    }
}
