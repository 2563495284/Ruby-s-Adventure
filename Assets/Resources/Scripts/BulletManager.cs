using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scene
{
    public class BulletManager : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy") && !collision.GetComponent<Animator>().GetBool("IsFixed"))
            {
                collision.GetComponent<Animator>().SetBool("IsFixed", true);
                collision.GetComponent<EnemyController>().randomWalkMethod = 0;
                collision.GetComponent<AudioSource>().clip = Resources.Load("Audio/Hit for enemy 1") as AudioClip;
                collision.GetComponent<AudioSource>().loop = false;
                collision.GetComponent<AudioSource>().Play();
                PlayerController.Player.ControlPlay(PlayerController.Player.CreateNewGO(Resources.Load("Audio/Robot Fixed") as AudioClip));
                collision.GetComponentsInChildren<ParticleSystem>()[0].Stop();
                collision.GetComponentsInChildren<ParticleSystem>()[1].Play();
                GetDemaged.fixedEnemyNum++;
                Destroy(gameObject);
                return;
            }
            if(collision.GetType().ToString()== "UnityEngine.BoxCollider2D"&&!collision.CompareTag("Player"))
            {
                if (collision.GetComponent<BoxCollider2D>().isTrigger == false)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            Destroy(gameObject, 3);
        }
    }
}