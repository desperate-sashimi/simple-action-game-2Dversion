using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class EnemyDetector : MonoBehaviour
{
    [Header("List")]
    public List<GameObject> EnemyDetectedList;

    private void OnTriggerEnter2D(Collider2D col){
        if(col.CompareTag("Enemy")){
            EnemyDetectedList.Add(col.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if(other.CompareTag("Enemy")){
            EnemyDetectedList.Remove(other.gameObject);
        }
    }

}
