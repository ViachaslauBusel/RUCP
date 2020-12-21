using RUCP;
using RUCP.Debugger;
using RUCP.Network;
using RUCP.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    //Объект с помощью которого удерживается связь с сервером
    public ServerSocket Socket { get; private set; } = null;

    private DebugBuffer debugBuffer;


    private void Awake()
    {
        Instance = this;
        RUCP.Debug.Start();
        debugBuffer = RUCP.Debug.Object as DebugBuffer;
    }



    public void Connection(string ip, int port)
    {
        //Если открытое соеденение, закрываем его
        Socket?.Close();
        //Создаем новый обькт с ип и портом сервера с которым будет устанавливать соедение
        Socket = new ServerSocket(ip, port);
        //Устанавливаем соеденение, при успешном подклчении Socket.NetworkStatus == NetworkStatus.СONNECTED
        Socket.Connection();
    }

    public static void Send(Packet packet)
    {
        if (Instance.Socket == null ||Instance.Socket.NetworkStatus != NetworkStatus.СONNECTED) return;
        //Отправка пакетов на сервер
        Instance.Socket.Send(packet);
    }

    private void Update()
    {
        string message = debugBuffer?.GetMessage();
        if (!string.IsNullOrEmpty(message))
            UnityEngine.Debug.Log(message);

        var error = debugBuffer?.GetError();
        if (!string.IsNullOrEmpty(error.Value.message))
            UnityEngine.Debug.LogError(error.Value.className + " : " + error.Value.message + " : " + error.Value.stackTrace);


        //Обработка полученных пакетов. Параметр - максимальное количество пакетов для обработки
        Socket?.ProcessPacket(10);
    }



    private void OnDestroy()
    {
        //Закрываем соеденение 
        Socket?.Close();
    }
}
