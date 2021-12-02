using UnityEngine;

namespace Scene
{
    public class GetDemaged : MonoBehaviour
    {
        internal static ushort fixedEnemyNum;//修好敌人数量
        internal static bool isGetMission = false;//是否接了任务
        internal static bool isFinishMission = false;//是否完成任务
        internal static ushort needToFixedNum = 3;//需要修好敌人数量
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")&&collision.GetComponent<PolygonCollider2D>().enabled==true)//碰到玩家玩家受伤
            {
                collision.GetComponent<PlayerController>().GetDemaged();
            }
        }
    }
}