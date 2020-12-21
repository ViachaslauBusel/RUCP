using RUCP.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    [SerializeField] Button connect;
    [SerializeField] InputField inputIP, inputPort;
    [SerializeField] Text statusTXT;
    private Text connectTXT;
    private NetworkStatus networkStatus = NetworkStatus.CLOSED;
    private Canvas canvas;

    private void Awake()
    {
        
        connectTXT = connect.GetComponentInChildren<Text>();
        canvas = GetComponent<Canvas>();
       StatusUpdate(networkStatus);
    }
    public void Connect()
    {
        connect.interactable = false;
        //Вызов метода поключения к серверу, по ип и порту указанных в InputField
        NetworkManager.Instance.Connection(inputIP.text, int.Parse(inputPort.text));
    }

    private void Update()
    {
        //Проверка не изменился ли статус подключения
        NetworkStatus status = (NetworkManager.Instance.Socket != null) ? NetworkManager.Instance.Socket.NetworkStatus : NetworkStatus.CLOSED;
        if (status != networkStatus)
        {
            networkStatus = status;
            //Отклик на обновление статуса
            StatusUpdate(networkStatus);
        }
    }
    public void Disconnect()
    {
        NetworkManager.Instance.Socket?.Close();
    }
    public void StatusUpdate(NetworkStatus status)
    {
        statusTXT.text = "Status: " + status.ToString();
        switch (status)
        {
            case NetworkStatus.CLOSED://Соеденение закрыто
                connectTXT.text = "Подключиться";
                connect.onClick.RemoveAllListeners();
                connect.onClick.AddListener(() => Connect());
                connect.interactable = true;
                inputIP.interactable = true;
                inputPort.interactable = true;
                canvas.enabled = true;
                break;
            case NetworkStatus.LISTENING://Ожидается соеденения с сервером
                connectTXT.text = "Подключение";
                connect.interactable = false;
                inputIP.interactable = false;
                inputPort.interactable = false;
                break;
            case NetworkStatus.СONNECTED://Подключение успешно
                //connectTXT.text = "Отключиться";
                //connect.onClick.RemoveAllListeners();
                //connect.onClick.AddListener(() => Disconnect());
                //connect.interactable = true;
                //inputIP.interactable = false;
                //inputPort.interactable = false;
                EnterName.Instance.On();//Открыть панель ввода имени
                canvas.enabled = false;
                break;
        }
    }
}
