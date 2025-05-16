
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Photon.Realtime;
using Hastable =ExitGames.Client.Photon.Hashtable;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killCountText;
    public TMP_Text deathCountText;

    private Player player; 
    public void Initialize(Player player)
    {
        this.player = player;
        usernameText.text = player.NickName;
        UpdateStats();
    }

    void UpdateStats()
    {
        if (player.CustomProperties.TryGetValue("killCount", out object killCount))
        {
            killCountText.text = killCount.ToString();
        }
        if (player.CustomProperties.TryGetValue("deathCount", out object deathCount))
        {
            killCountText.text = deathCount.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hastable changedProps)
    {
        if (targetPlayer == player)
        {
            if (changedProps.ContainsKey("killCount") ||changedProps.ContainsKey("deathCount"))
            {
                UpdateStats();
            }
        }
        
    }
}
