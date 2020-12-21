using RUCP.Handler;
using RUCP.Network;
using RUCP.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSynchronizer : MonoBehaviour
{
    public static PlayerSynchronizer Instance { get; private set; }
    private Vector3 lastSentPosition = Vector3.zero;

    private void Awake()
    {
        Instance = this;
        enabled = false;
    }

    

    private void Update()
    {
        //Если игрок переместился на расстояние больше указоной от точки предыдущей отправки 
        if (Vector3.Distance(lastSentPosition, transform.position) > 0.1f)
        {
            //Отпровляем свою позицию на сервер
            SyncPosition();
        }
    }

    private void SyncPosition()
    {
        lastSentPosition = transform.position;
        //Отправка своей позиции на сервер
        Packet packet = Packet.Create(Channel.Discard);
        packet.WriteType(Types.CharacterMove);
        packet.Write(lastSentPosition);
        NetworkManager.Send(packet);

    }

   
}
