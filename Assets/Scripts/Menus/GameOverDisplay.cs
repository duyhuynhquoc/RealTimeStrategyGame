using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverDisplayParent = null;

    [SerializeField]
    private TMP_Text winnerNamerText = null;

    void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientOnGameOverHandler;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientOnGameOverHandler;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientOnGameOverHandler(string winnerName)
    {
        gameOverDisplayParent.SetActive(true);

        winnerNamerText.text = $"{winnerName} has won!";
    }
}
