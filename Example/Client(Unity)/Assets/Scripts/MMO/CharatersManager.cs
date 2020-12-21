using RUCP.Handler;
using RUCP.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharatersManager : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    private Dictionary<int, Character> characters = new Dictionary<int, Character>();
   

    private void Awake()
    {
        //Привязка типа пакета к методу для его обработки
        HandlersStorage.RegisterHandler(Types.CharacterCreate, CharacterCreate);
        HandlersStorage.RegisterHandler(Types.CharacterMove, CharacterMove);
        HandlersStorage.RegisterHandler(Types.CharacterDestroy, CharacterDestroy);
    }

    //Создание нового персонажа
    private void CharacterCreate(Packet packet)
    {
        //ИД создаваемого персонажа
        int id = packet.ReadInt();
        //Если персонаж еще не создан
        if (!characters.ContainsKey(id))
        {
            string name = packet.ReadString();
            packet.Read(out Vector3 position);//Чтения позиции в которой будет создан персонаж
            //Создание персонажа
            GameObject characterOBJ = Instantiate(characterPrefab, position, Quaternion.identity);
            Character character = characterOBJ.GetComponent<Character>();
            character.Name = name;
            characters.Add(id, character);
        }
    }
    //Удаление персонажа
    private void CharacterDestroy(Packet packet)
    {
        //Чтения ИД персонажа
        int id = packet.ReadInt();
        if(characters.TryGetValue(id, out Character character))
        {
            Destroy(character.gameObject);
            characters.Remove(id);
        }
    }
    //Получение новой позиции персонажа
    private void CharacterMove(Packet packet)
    {
        //Чтения ИД персонажа
        int id = packet.ReadInt();
        if (characters.TryGetValue(id, out Character character))
        {
            //Задать новую позицию
            packet.Read(out Vector3 position);
            character.Move(position);
        }
    }

    private void OnDestroy()
    {
        //Описываем методы при уничтожении обьекта
        HandlersStorage.UnregisterHandler(Types.CharacterCreate);
        HandlersStorage.UnregisterHandler(Types.CharacterMove);
        HandlersStorage.UnregisterHandler(Types.CharacterDestroy);
    }
}
