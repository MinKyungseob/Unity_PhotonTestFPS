using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;
    [SerializeField] Item[] items;

    private int itemIndex;
    private int previousItemIndex = -1;

    float verticalLookRatation;
    bool isGrounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    Rigidbody rb;
    PhotonView PV;

    private const float maxHealth = 100f;
    private float currentHealth = maxHealth;

    PlayerManager playerManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    void Update()
    {
        if (!PV.IsMine)
            return;
        Look();
        Move();
        Jump();

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
        //press 1, 2 .. to switch item
        //휠을 굴려서 아이템 무기 변경하는 형태이지만. 이때의 경우, 0번과 최대번호를 연결 한 느낌으로, 0에서 1 만큼 줄이면 다시 최대에서 부터 줄일 수 있는 형태로
        //Cycle 형태로 
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <=0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex-1);
            }

        } 
        
        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
        }

        if (transform.position.y < -10.0f)
        {
            Die();
        }
        
        /*//0번 보다 아래로 내린다 해도 무기가 바뀌지 않게끔. 반대로도 맞음.
         if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <=0)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex-1);
            }
        }*/
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRatation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRatation = Mathf.Clamp(verticalLookRatation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRatation;
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount,
            moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed),
            ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up*jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if(_index == previousItemIndex)
            return;
        itemIndex = _index;
        
        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        //sending properties after network
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("ItemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Receiving Property data
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _isGrounded)
    {
        isGrounded = _isGrounded;
    }

    private void FixedUpdate()
    {
        if(!PV.IsMine) 
            return;
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if(!PV.IsMine)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log("Took damage : " + damage);
    }

    void Die()
    {
        playerManager.Die();
    }
}
