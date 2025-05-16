using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Linq;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    private PhotonView PV;

    GameObject controller;

    private int killCount;
    private int deathCount;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
        }

    }

    void CreateController()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[]{PV.ViewID});
        //Debug.Log("Instantiated Player Controller");
        //Instantiate my Player Controller
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
        
        
        deathCount++;

        Hashtable hash = new Hashtable();
        hash.Add("deathCount", deathCount);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetKilled()
    {
        PV.RPC(nameof(RPC_GetKilled),PV.Owner);
    }

    [PunRPC]
    void RPC_GetKilled()
    {
        killCount++;

        Hashtable hash = new Hashtable();
        hash.Add("killCount", killCount);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public static PlayerManager Find(Player player)
    {
        //처음 보는 문법. 뒤의 singleOrDefault 공부하기!. 문장상 하나거나 디폴트 상태 라는 말로 혼자일때 지정하는 형태로 보면 될듯.
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner== player);
        
    }
}
 
