using RUCP.Handler;
using RUCP.Network;
using RUCP.Packets;
using UnityEngine;
using UnityEngine.UI;

public class EnterName : MonoBehaviour
{
    public static EnterName Instance { get; private set; }
    [SerializeField] InputField inputName;
    [SerializeField] Button button;
    private Text buttonText;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        //Привязка типа пакета к методу для его обработки
        HandlersStorage.RegisterHandler(Types.EnterWorld, EnterWorld);

        canvas = GetComponent<Canvas>();
        buttonText = button.GetComponentInChildren<Text>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Enter);
    }

    public void On()
    {
        canvas.enabled = true;
    }
    public void Off()
    {
        canvas.enabled = false;
    }
    //Обработка пакета при успешном подключении к игровому миру
    private void EnterWorld(Packet packet)
    {
        //Чтения позиции игрокан на сервере
        packet.Read(out Vector3 position);
        //Задать позицию игрока на клиенте
        PlayerSynchronizer.Instance.transform.position = position;
        //Включить отправку позиции игрока с клиента на сервер
        PlayerSynchronizer.Instance.enabled = true;
        //Скрыть панель ввода имени
        Off();
    }

    public void Enter()
    {
        if(inputName.text.Length == 0 || inputName.text.Length > 30)
        {
            print("имя должно содержать не менее 1 и не более 30 символов");
            return;
        }
        button.interactable = false;
        inputName.interactable = false;
        buttonText.text = "Подключение";

        //Отправить запрос на поключения к игровому миру
        Packet packet = Packet.Create(Channel.Reliable);
        packet.WriteType(Types.EnterWorld);
        packet.WriteString(inputName.text);
        NetworkManager.Send(packet);
    }
    private void OnDestroy()
    {
        //Описываем методы при уничтожении обьекта
        HandlersStorage.UnregisterHandler(Types.EnterWorld);
    }
}
