using UnityEngine.UI;
using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class PlayerController : MonoBehaviour
    {
        private const float devideSpeed = 0.04f;//行走速度
        private const float highSpeed = 0.08f;//跑步速度
        private float playerSpeed = 0;//当前速度
        private readonly float playerMaxHP = 30;//最大HP
        private float playerPresentHP;//当前HP
        public float hurtNum;//伤害数值
        public float recoveredNum;//恢复数值
        public float rayDistance;//射线距离
        private readonly float acceleration = 0.03f;//走路加速度
        private readonly float invincibleTime = 0.99f;//无敌时间
        private GameObject mask;//mask遮罩，设置血条长度
        private Vector2 maskVector2 = new Vector2(322.74f, 90.08f);//maskUI原本长宽
        private Vector3 offset = new Vector3(0.5f, 0.5f, 0);//发射子弹偏移量
        private Vector3 force = new Vector3(10, 10, 0);//子弹受力大小
        private Vector3 respawnPosition;//重生位置
        private Quaternion respawnQuaternion;//重生角度
        private Animator playerAnimator;//玩家的动画机
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
            if (Input.GetMouseButtonDown(0) && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))//受伤时不能射击和接任务
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
            //if (h != 0)//单向移动
            //    v = 0;
            //else
            //    h = 0;
            if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Launch") && !IsNeedReborn())//不射击且不用重生时可移动
            {
                ChangeDirection(h, v);
                playerAnimator.SetFloat("Speed", playerSpeed);
                transform.Translate(Vector2.right * h * playerSpeed);
                transform.Translate(Vector2.up * v * playerSpeed);
            }
            Reborn(IsNeedReborn());//需要重生时就重生
        }
        private RaycastHit2D Ray()//Ruby专有射线检测
        {
            float x = GetComponent<Animator>().GetFloat("Look X");
            float y = GetComponent<Animator>().GetFloat("Look Y");
            Vector3 offset;//offset是根据BoxCollider2D大小和Ruby的位置算出来的，这样就可以用一个距离而且不会检测不到NPC
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
        private void MissionComplete()//完成任务的操作
        {
            RaycastHit2D ray = Ray();
            if (ray.collider != null && ray.collider.CompareTag("NPC") && Scene.GetDemaged.fixedEnemyNum == Scene.GetDemaged.needToFixedNum)//射线检测到了NPC且已经修好的机器人和应该要修的机器人数量相同
            {
                Scene.GetDemaged.isFinishMission = true;
                foreach (GameObject temp in Resources.FindObjectsOfTypeAll(typeof(GameObject)))//找隐藏对话框
                {
                    if (temp.activeSelf == false && temp.name == "DialogCanvas")
                    {
                        temp.SetActive(true);
                        temp.transform.GetChild(1).GetComponent<Text>().text = "干得好，对了Ruby天冷了进来我的♂米奇妙妙屋♂暖和暖和。";
                        ControlPlay(CreateNewGO(Resources.Load("Audio/Quest Complete") as AudioClip));
                        StartCoroutine("StartDialogCount", temp);
                        uIManagerment.GameOver();
                    }
                }
            }
        }
        private void GetMission()//获取任务
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
                        temp.transform.GetChild(1).GetComponent<Text>().text = "Ruby，帮爷修下机器人。";
                        StartCoroutine("StartDialogCount", temp);
                    }
                }
            }
        }
        public void RecoverHP(GameObject food)//加血
        {
            if (playerPresentHP == playerMaxHP)//满血就返回
                return;
            else
            {
                if (playerPresentHP + recoveredNum > playerMaxHP)//恢复血量超过最大值，加到满血
                {
                    playerPresentHP = playerMaxHP;
                    mask.GetComponent<RectTransform>().sizeDelta = maskVector2;
                }
                else
                {
                    playerPresentHP += recoveredNum;//恢复当前恢复量
                    mask.GetComponent<RectTransform>().sizeDelta = new Vector2(maskVector2.x * (playerPresentHP / playerMaxHP), maskVector2.y);
                }
                food.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
                ControlPlay(CreateNewGO(Resources.Load<AudioClip>("Audio/Collectable")));
                Destroy(food, 0.3f);
            }
        }
        private void ChangeDirection(float h, float v)//动画机转向
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
        private void ChangeSpeed(float h, float v)//加减速
        {
            if (h != 0 || v != 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))//按下shift加到最高速
                {
                    if (playerSpeed >= highSpeed)
                        return;
                    playerSpeed += acceleration;
                }
                else
                {
                    if (playerSpeed > devideSpeed)//速度比步行高，减速
                    {
                        playerSpeed -= acceleration;
                        return;
                    }
                    else//加速
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
        public void GetDemaged()//受伤
        {
            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))//如果正在受伤，则不再受伤，否则受伤
                return;
            playerPresentHP -= hurtNum;
            playerAnimator.SetTrigger("Hit");
            mask.GetComponent<RectTransform>().sizeDelta = new Vector2(maskVector2.x * (playerPresentHP / playerMaxHP), maskVector2.y);
            GetComponent<AudioSource>().Play();
            GetComponent<PolygonCollider2D>().enabled = false;
            StartCoroutine("StartInvincible");
        }
        private void Attack()//攻击
        {
            playerAnimator.SetTrigger("Launch");
            GameObject go = Instantiate(Resources.Load("Prefabs/Bullet"), transform.position + ChangeBulletOrOffsetDirection(offset, true), transform.rotation) as GameObject;
            go.GetComponent<Rigidbody2D>().AddForce(ChangeBulletOrOffsetDirection(force, false));
            ControlPlay(CreateNewGO(Resources.Load("Audio/Throw Cog") as AudioClip));
        }
        private bool IsNeedReborn()//是否需要重生
        {
            if (playerPresentHP <= 0)
                return true;
            else
                return false;
        }
        private void Reborn(bool isNeed)//重生方法
        {
            if (isNeed)//需要重生且重生动画播放三次后
            {
                GetComponent<BoxCollider2D>().enabled = false;
                if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3)
                {
                    Instantiate(Resources.Load("Prefabs/Ruby") as GameObject, respawnPosition, respawnQuaternion);
                    Destroy(gameObject);
                }
            }

        }
        public GameObject CreateNewGO(AudioClip clip)//创建新物体承载已销毁物体要求播放的声音
        {
            GameObject go = new GameObject();
            go.transform.position = new Vector3(0, 0, 0);
            go.AddComponent<AudioSource>().clip = clip;
            go.GetComponent<AudioSource>().Play();
            return go;
        }
        public void ControlPlay(GameObject go)//播放完毕后销毁
        {
            Destroy(go, go.GetComponent<AudioSource>().time + 0.5f);
        }
        private Vector3 ChangeBulletOrOffsetDirection(Vector3 temp, bool isOffset)//决定左右上下子弹方向且决定偏移量方向
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