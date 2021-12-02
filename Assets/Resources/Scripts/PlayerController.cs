using UnityEngine.UI;
using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class PlayerController : MonoBehaviour
    {
        private const float devideSpeed = 0.04f;//�����ٶ�
        private const float highSpeed = 0.08f;//�ܲ��ٶ�
        private float playerSpeed = 0;//��ǰ�ٶ�
        private readonly float playerMaxHP = 30;//���HP
        private float playerPresentHP;//��ǰHP
        public float hurtNum;//�˺���ֵ
        public float recoveredNum;//�ָ���ֵ
        public float rayDistance;//���߾���
        private readonly float acceleration = 0.03f;//��·���ٶ�
        private readonly float invincibleTime = 0.99f;//�޵�ʱ��
        private GameObject mask;//mask���֣�����Ѫ������
        private Vector2 maskVector2 = new Vector2(322.74f, 90.08f);//maskUIԭ������
        private Vector3 offset = new Vector3(0.5f, 0.5f, 0);//�����ӵ�ƫ����
        private Vector3 force = new Vector3(10, 10, 0);//�ӵ�������С
        private Vector3 respawnPosition;//����λ��
        private Quaternion respawnQuaternion;//�����Ƕ�
        private Animator playerAnimator;//��ҵĶ�����
        public UIManagerment uIManagerment;
        public static PlayerController Player { get; private set; }
        IEnumerator StartInvincible()
        {
            yield return new WaitForSeconds(invincibleTime);
            GetComponent<PolygonCollider2D>().enabled = true;
        }
        IEnumerator StartDialogCount(GameObject temp)
        {
            yield return new WaitForSeconds(5f);
            temp.SetActive(false);
        }
        private void Awake()
        {
            Player = this;
        }
        private void Start()
        {
            mask = GameObject.Find("Mask");
            mask.GetComponent<RectTransform>().sizeDelta = maskVector2;
            GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().Follow = gameObject.transform;
            playerPresentHP = playerMaxHP;
            respawnPosition = transform.position;
            respawnQuaternion = transform.rotation;
            playerAnimator = GetComponent<Animator>();
            playerAnimator.SetFloat("Look Y", 1);
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))//����ʱ��������ͽ�����
            {
                if (Ray().collider == null || !Ray().collider.CompareTag("NPC"))
                    if (Scene.GetDemaged.isGetMission && !Scene.GetDemaged.isFinishMission)
                        Attack();
                if (!Scene.GetDemaged.isFinishMission && Scene.GetDemaged.isGetMission)
                    MissionComplete();                                                 
                if (!Scene.GetDemaged.isGetMission)                                   
                    GetMission();                                                      
            }
        }
        private void FixedUpdate()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            ChangeSpeed(h, v);
            //if (h != 0)//�����ƶ�
            //    v = 0;
            //else
            //    h = 0;
            if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Launch") && !IsNeedReborn())//������Ҳ�������ʱ���ƶ�
            {
                ChangeDirection(h, v);
                playerAnimator.SetFloat("Speed", playerSpeed);
                transform.Translate(Vector2.right * h * playerSpeed);
                transform.Translate(Vector2.up * v * playerSpeed);
            }
            Reborn(IsNeedReborn());//��Ҫ����ʱ������
        }
        private RaycastHit2D Ray()//Rubyר�����߼��
        {
            float x = GetComponent<Animator>().GetFloat("Look X");
            float y = GetComponent<Animator>().GetFloat("Look Y");
            Vector3 offset;//offset�Ǹ���BoxCollider2D��С��Ruby��λ��������ģ������Ϳ�����һ��������Ҳ����ⲻ��NPC
            if (y > 0)
                offset = new Vector3(0, 0.5f, 0);
            else if (y < 0)
                offset = new Vector3(0, -0.2f, 0);
            else if (x > 0)
                offset = new Vector3(0.5f, 0, 0);
            else
                offset = new Vector3(-0.5f, 0, 0);
            RaycastHit2D NPC_Ray = Physics2D.Raycast(gameObject.transform.position + offset, new Vector3(x, y, 0), rayDistance, LayerMask.GetMask("Default"));
            return NPC_Ray;
        }
        private void MissionComplete()//�������Ĳ���
        {
            RaycastHit2D ray = Ray();
            if (ray.collider != null && ray.collider.CompareTag("NPC") && Scene.GetDemaged.fixedEnemyNum == Scene.GetDemaged.needToFixedNum)//���߼�⵽��NPC���Ѿ��޺õĻ����˺�Ӧ��Ҫ�޵Ļ�����������ͬ
            {
                Scene.GetDemaged.isFinishMission = true;
                foreach (GameObject temp in Resources.FindObjectsOfTypeAll(typeof(GameObject)))//�����ضԻ���
                {
                    if (temp.activeSelf == false && temp.name == "DialogCanvas")
                    {
                        temp.SetActive(true);
                        temp.transform.GetChild(1).GetComponent<Text>().text = "�ɵúã�����Ruby�����˽����ҵġ����������ݡ�ů��ů�͡�";
                        ControlPlay(CreateNewGO(Resources.Load("Audio/Quest Complete") as AudioClip));
                        StartCoroutine("StartDialogCount", temp);
                        uIManagerment.GameOver();
                    }
                }
            }
        }
        private void GetMission()//��ȡ����
        {
            RaycastHit2D ray = Ray();
            if (ray.collider != null && ray.collider.CompareTag("NPC"))
            {
                Scene.GetDemaged.isGetMission = true;
                foreach (GameObject temp in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    if (temp.activeSelf == false && temp.name == "DialogCanvas")
                    {
                        temp.SetActive(true);
                        temp.transform.GetChild(1).GetComponent<Text>().text = "Ruby����ү���»����ˡ�";
                        StartCoroutine("StartDialogCount", temp);
                    }
                }
            }
        }
        public void RecoverHP(GameObject food)//��Ѫ
        {
            if (playerPresentHP == playerMaxHP)//��Ѫ�ͷ���
                return;
            else
            {
                if (playerPresentHP + recoveredNum > playerMaxHP)//�ָ�Ѫ���������ֵ���ӵ���Ѫ
                {
                    playerPresentHP = playerMaxHP;
                    mask.GetComponent<RectTransform>().sizeDelta = maskVector2;
                }
                else
                {
                    playerPresentHP += recoveredNum;//�ָ���ǰ�ָ���
                    mask.GetComponent<RectTransform>().sizeDelta = new Vector2(maskVector2.x * (playerPresentHP / playerMaxHP), maskVector2.y);
                }
                food.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
                ControlPlay(CreateNewGO(Resources.Load<AudioClip>("Audio/Collectable")));
                Destroy(food, 0.3f);
            }
        }
        private void ChangeDirection(float h, float v)//������ת��
        {
            if (h != 0)
            {
                playerAnimator.SetFloat("Look X", h);
                playerAnimator.SetFloat("Look Y", 0);
            }
            else if (v != 0)
            {
                playerAnimator.SetFloat("Look Y", v);
                playerAnimator.SetFloat("Look X", 0);
            }
        }
        private void ChangeSpeed(float h, float v)//�Ӽ���
        {
            if (h != 0 || v != 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))//����shift�ӵ������
                {
                    if (playerSpeed >= highSpeed)
                        return;
                    playerSpeed += acceleration;
                }
                else
                {
                    if (playerSpeed > devideSpeed)//�ٶȱȲ��иߣ�����
                    {
                        playerSpeed -= acceleration;
                        return;
                    }
                    else//����
                    {
                        if (playerSpeed == devideSpeed)
                            return;
                        playerSpeed += acceleration;
                    }
                }
            }
            else
                playerSpeed = 0;
        }
        public void GetDemaged()//����
        {
            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))//����������ˣ��������ˣ���������
                return;
            playerPresentHP -= hurtNum;
            playerAnimator.SetTrigger("Hit");
            mask.GetComponent<RectTransform>().sizeDelta = new Vector2(maskVector2.x * (playerPresentHP / playerMaxHP), maskVector2.y);
            GetComponent<AudioSource>().Play();
            GetComponent<PolygonCollider2D>().enabled = false;
            StartCoroutine("StartInvincible");
        }
        private void Attack()//����
        {
            playerAnimator.SetTrigger("Launch");
            GameObject go = Instantiate(Resources.Load("Prefabs/Bullet"), transform.position + ChangeBulletOrOffsetDirection(offset, true), transform.rotation) as GameObject;
            go.GetComponent<Rigidbody2D>().AddForce(ChangeBulletOrOffsetDirection(force, false));
            ControlPlay(CreateNewGO(Resources.Load("Audio/Throw Cog") as AudioClip));
        }
        private bool IsNeedReborn()//�Ƿ���Ҫ����
        {
            if (playerPresentHP <= 0)
                return true;
            else
                return false;
        }
        private void Reborn(bool isNeed)//��������
        {
            if (isNeed)//��Ҫ���������������������κ�
            {
                GetComponent<BoxCollider2D>().enabled = false;
                if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3)
                {
                    Instantiate(Resources.Load("Prefabs/Ruby") as GameObject, respawnPosition, respawnQuaternion);
                    Destroy(gameObject);
                }
            }

        }
        public GameObject CreateNewGO(AudioClip clip)//�����������������������Ҫ�󲥷ŵ�����
        {
            GameObject go = new GameObject();
            go.transform.position = new Vector3(0, 0, 0);
            go.AddComponent<AudioSource>().clip = clip;
            go.GetComponent<AudioSource>().Play();
            return go;
        }
        public void ControlPlay(GameObject go)//������Ϻ�����
        {
            Destroy(go, go.GetComponent<AudioSource>().time + 0.5f);
        }
        private Vector3 ChangeBulletOrOffsetDirection(Vector3 temp, bool isOffset)//�������������ӵ������Ҿ���ƫ��������
        {
            if (isOffset)
            {
                if (playerAnimator.GetFloat("Look X") >= 0.01f)
                    return new Vector3(temp.x, temp.y, 0);
                else if (playerAnimator.GetFloat("Look X") <= -0.01f)
                    return new Vector3(-temp.x, temp.y, 0);
                else if (playerAnimator.GetFloat("Look Y") >= 0.01f)
                    return new Vector3(0, temp.y, 0);
                else
                    return new Vector3(0, 0.5f - temp.y, 0);
            }
            else
            {
                if (playerAnimator.GetFloat("Look X") >= 0.01f)
                    return new Vector3(temp.x, 0, 0);
                else if (playerAnimator.GetFloat("Look X") <= -0.01f)
                    return new Vector3(-temp.x, 0, 0);
                else if (playerAnimator.GetFloat("Look Y") >= 0.01f)
                    return new Vector3(0, temp.y, 0);
                else
                    return new Vector3(0, -temp.y, 0);
            }
        }
    }
}