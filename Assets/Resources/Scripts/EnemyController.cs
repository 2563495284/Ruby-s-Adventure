using UnityEngine;

namespace Scene
{
    public class EnemyController : MonoBehaviour
    {
        public sbyte randomWalkMethod;//1左右2上下
        private sbyte towardDirection;//1正向-1负向
        public float enemySpeed;
        public float offset;//能走多远
        private Vector3 localVector3;//不能用transform因为ta传递的是引用类型不是值
        private void Start()
        {
            towardDirection = 1;
            localVector3 = transform.position;
        }
        private void FixedUpdate()
        {
            if (randomWalkMethod == 1)//左右走
                WalkMethod(ref towardDirection, Vector3.right);
            else if (randomWalkMethod == 2)//上下走
                WalkMethod(ref towardDirection, Vector3.up);
        }
        private void WalkMethod(ref sbyte num, Vector3 vector)//敌人行走方法
        {
            if (num == 1)//右走/上走
            {
                ChangeWalkDirection(vector, num);
                transform.position += vector * enemySpeed * Time.deltaTime * num;
                if (vector == Vector3.right && transform.position.x >= localVector3.x + offset)
                    num = -1;
                else if (vector == Vector3.up && transform.position.y >= localVector3.y + offset)
                    num = -1;
            }
            else//左走/下走
            {
                ChangeWalkDirection(vector, num);
                transform.position += vector * enemySpeed * Time.deltaTime * num;
                if (vector == Vector3.right && transform.position.x <= localVector3.x - offset)
                    num = 1;
                else if (vector == Vector3.up && transform.position.y <= localVector3.y - offset)
                    num = 1;
            }
        }
        //public static bool operator >=(Vector3 v1,Vector3 v2)//想用运算符重载来着结果发现自己写不对查了资料无果放弃了
        //{
        //    bool state = false;
        //    if (v1.x >= v2.x || v1.y >= v2.y)
        //        state = true;
        //    return state;
        //}
        //public static bool operator <=(Vector3 v1, Vector3 v2)
        //{
        //    bool state = false;
        //    if (v1.x <= v2.x || v1.y <= v2.y)
        //        state = true;
        //    return state;
        //}
        //private Vector3 ChangeDirection(float offset)//传偏移量进来，出vector3，实现vector和float运算
        //{
        //    if (randomWalkMethod == 1)
        //        return new Vector3(offset, 0, 0);
        //    else
        //        return new Vector3(0, offset, 0);
        //}
        private void ChangeWalkDirection(Vector3 vector, sbyte num)//决定左右上下行走方向
        {
            if (vector == Vector3.right)
                GetComponent<Animator>().SetFloat("X", num);
            else
                GetComponent<Animator>().SetFloat("Y", num);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player") && !GetComponent<Animator>().GetBool("IsFixed")&&collision.collider.GetComponent<PolygonCollider2D>().enabled==true)//敌人碰到玩家且自己没被修好且不在无敌时间内
            {
                collision.gameObject.GetComponent<PlayerController>().GetDemaged();//受伤
            }
        }
    }
}