using UnityEngine;

namespace Scene
{
    public class GetDemaged : MonoBehaviour
    {
        internal static ushort fixedEnemyNum;//�޺õ�������
        internal static bool isGetMission = false;//�Ƿ��������
        internal static bool isFinishMission = false;//�Ƿ��������
        internal static ushort needToFixedNum = 3;//��Ҫ�޺õ�������
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")&&collision.GetComponent<PolygonCollider2D>().enabled==true)//��������������
            {
                collision.GetComponent<PlayerController>().GetDemaged();
            }
        }
    }
}