using UnityEngine;

namespace Scene
{
    public class EnemyController : MonoBehaviour
    {
        public sbyte randomWalkMethod;//1����2����
        private sbyte towardDirection;//1����-1����
        public float enemySpeed;
        public float offset;//���߶�Զ
        private Vector3 localVector3;//������transform��Ϊta���ݵ����������Ͳ���ֵ
        private void Start()
        {
            towardDirection = 1;
            localVector3 = transform.position;
        }
        private void FixedUpdate()
        {
            if (randomWalkMethod == 1)//������
                WalkMethod(ref towardDirection, Vector3.right);
            else if (randomWalkMethod == 2)//������
                WalkMethod(ref towardDirection, Vector3.up);
        }
        private void WalkMethod(ref sbyte num, Vector3 vector)//�������߷���
        {
            if (num == 1)//����/����
            {
                ChangeWalkDirection(vector, num);
                transform.position += vector * enemySpeed * Time.deltaTime * num;
                if (vector == Vector3.right && transform.position.x >= localVector3.x + offset)
                    num = -1;
                else if (vector == Vector3.up && transform.position.y >= localVector3.y + offset)
                    num = -1;
            }
            else//����/����
            {
                ChangeWalkDirection(vector, num);
                transform.position += vector * enemySpeed * Time.deltaTime * num;
                if (vector == Vector3.right && transform.position.x <= localVector3.x - offset)
                    num = 1;
                else if (vector == Vector3.up && transform.position.y <= localVector3.y - offset)
                    num = 1;
            }
        }
        //public static bool operator >=(Vector3 v1,Vector3 v2)//����������������Ž�������Լ�д���Բ��������޹�������
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
        //private Vector3 ChangeDirection(float offset)//��ƫ������������vector3��ʵ��vector��float����
        //{
        //    if (randomWalkMethod == 1)
        //        return new Vector3(offset, 0, 0);
        //    else
        //        return new Vector3(0, offset, 0);
        //}
        private void ChangeWalkDirection(Vector3 vector, sbyte num)//���������������߷���
        {
            if (vector == Vector3.right)
                GetComponent<Animator>().SetFloat("X", num);
            else
                GetComponent<Animator>().SetFloat("Y", num);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player") && !GetComponent<Animator>().GetBool("IsFixed")&&collision.collider.GetComponent<PolygonCollider2D>().enabled==true)//��������������Լ�û���޺��Ҳ����޵�ʱ����
            {
                collision.gameObject.GetComponent<PlayerController>().GetDemaged();//����
            }
        }
    }
}