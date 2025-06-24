using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public class NpcGreetPlayer : MonoBehaviour
{
    // audio source for greeting sound
    [SerializeField] private AudioClip[] _greetingAudioClips;
    [SerializeField] private float _greetingDelay = 10f;
    [SerializeField, ReadOnly] private bool _canPlayGreeting = true;
    
    private AudioSource _audioSource;
    private Coroutine _coroutine;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canPlayGreeting) return;
        if (_greetingAudioClips.Length == 0) return;
        if (!other.CompareTag("Player")) return;
        
        // play the greeting sound
        _audioSource.clip = _greetingAudioClips[Random.Range(0, _greetingAudioClips.Length)];
        _audioSource.Play();
        
        // add a delay before the next greeting can be played
        _canPlayGreeting = false;
        _coroutine = StartCoroutine(DelayGreetReset());
    }
    
    private IEnumerator DelayGreetReset()
    {
        yield return new WaitForSeconds(_greetingDelay);
        _canPlayGreeting = true;
    }
}
