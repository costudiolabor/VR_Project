using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMan : NetworkManager {
    
    public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
    {
        public Vector2 vector2; //нельзя использовать Property
    }
    
    public void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message)
    {
        GameObject go = Instantiate(playerPrefab, message.vector2, Quaternion.identity); //локально на сервере создаем gameObject
        NetworkServer.AddPlayerForConnection(conn, go); //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн
    }
    
    bool playerSpawned;

    public void ActivatePlayerSpawn()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 10f;
        pos = Camera.main.ScreenToWorldPoint(pos);

        PosMessage m = new PosMessage() { vector2 = pos }; //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
        NetworkClient.Send(m); //отправка сообщения на сервер с координатами спавна
        playerSpawned = true;
    }
    
    bool playerConnected;

    public override void OnClientConnect()
    {
        //base.OnClientConnect(conn);
        base.OnClientConnect();
        playerConnected = true;
    }

    
    public override void Update()
    {
        base.Update();
        
        if (Input.GetKeyDown(KeyCode.Mouse0) && !playerSpawned && playerConnected)
        {
            ActivatePlayerSpawn();
        }
    }
    
    
}