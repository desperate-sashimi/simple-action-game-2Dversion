using UnityEngine;
using DG.Tweening;

public class GhostTrail : MonoBehaviour
{
    private PlayerController move;
    private Enemy enemy;
    private SpriteRenderer sr;
    public Transform ghostsParent;
    public Color trailColor;
    public Color fadeColor;
    public float ghostInterval;
    public float fadeTime;

    private void Start()
    {
        move = FindObjectOfType<PlayerController>();
        enemy = FindObjectOfType<Enemy>();

        sr = GetComponent<SpriteRenderer>();
    }

    // instead of using animation, using dotween could use the sprite render of the player
    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < ghostsParent.childCount; i++)
        {
            Transform currentGhost = ghostsParent.GetChild(i);

            s.AppendCallback(()=> currentGhost.position = move.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = move.transform.localScale.x > 0?true:false);
            s.AppendCallback(()=>currentGhost.GetComponent<SpriteRenderer>().sprite = move.GetComponent<SpriteRenderer>().sprite);


            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(trailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(ghostInterval);
        }
    }


    public void ShowGhostEnemy(){
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < ghostsParent.childCount; i++)
        {
            Transform currentGhost = ghostsParent.GetChild(i);

            s.AppendCallback(()=> currentGhost.position = enemy.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = enemy.transform.localScale.x > 0?true:false);
            s.AppendCallback(()=>currentGhost.GetComponent<SpriteRenderer>().sprite = enemy.GetComponent<SpriteRenderer>().sprite);


            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(trailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(ghostInterval);
        }

    }

    public void FadeSprite(Transform current)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill(); // kill the tweens that use this object as a reference
        current.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime); // ghost fade in fadeTime
        
    }

}
