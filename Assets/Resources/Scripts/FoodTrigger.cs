using UnityEngine;

namespace Scene
{
    public class FoodTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")&&collision.GetType().ToString()== "UnityEngine.BoxCollider2D")
            {
                PlayerController.Player.RecoverHP(gameObject);
            }
        }
    }
}