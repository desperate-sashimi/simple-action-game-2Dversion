using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackSense : MonoBehaviour
{
    private static AttackSense instance;
    public static AttackSense Instance
    {
        get
        {
            if (instance == null)
                instance = Transform.FindObjectOfType<AttackSense>();
            return instance;
        }
    }
    private bool isShake; // is shaking the camera

    public void HitPause(int duration)
    {
        StartCoroutine(Pause(duration));
    }

    IEnumerator Pause(int duration)
    {
        float pauseTime = duration / 60f;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(pauseTime);
        Time.timeScale = 0.8f;
    }

    IEnumerator ConPause(int duration){
        float pauseTime = duration / 60f;
        DOVirtual.Float(0.1f,0.8f,pauseTime - 0.1f,timeScaleChanger).SetEase(Ease.InOutSine);
        yield return new WaitForSecondsRealtime(pauseTime - 0.05f);
        Time.timeScale = 1;
        
    }
    public void ContinousPause(int duration){
        StartCoroutine(ConPause(duration));
    }

    private void timeScaleChanger(float time){
        Time.timeScale = time;
    }

    public void CameraShake(float duration, float strength)
    {
        if (!isShake)
            StartCoroutine(Shake(duration, strength));
    }

    IEnumerator Shake(float duration, float strength)
    {
        isShake = true;
        Transform camera = Camera.main.transform;
        Vector3 startPosition = camera.position;

        while (duration > 0)
        {
            camera.position = Random.insideUnitSphere * strength + startPosition;
            duration -= Time.deltaTime;
            yield return null; // update within frame
        }
        camera.position = startPosition;
        isShake = false;
    }
}
