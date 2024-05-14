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

    private float NormalizedTime()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(currentAnimation))
        {
            return stateInfo.normalizedTime;
        }
        return 0;
    }

    public void Update()
    {
        if (currentAnimation == null || clipLength[currentAnimation] == float.MaxValue)
        {
            return;
        }
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (NormalizedTime() >= 1)
        {
            finished = true;
        }
    }

    public float ClipLength(string clipName)
    {
        return clipLength[clipName];
    }

    public IEnumerator<Transition> PlayAnimation(string clipName, Action<float> updater = null)
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
                    updater(NormalizedTime());
                }
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
