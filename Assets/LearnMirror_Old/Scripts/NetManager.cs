using Mirror;
using UnityEngine;

public class NetManager : NetworkManager {
    //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
    public struct PosMessage : NetworkMessage {
        //нельзя использовать Property
        public Vector2 vector2; 
    }

    private void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message) {
        //локально на сервере создаем gameObject
        //GameObject go = Instantiate(playerPrefab, message.vector2, Quaternion.identity); 
        //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
        
        Transform startPos = GetStartPosition();
        _player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, _player); 
    }
    
    public override void OnStartServer() {
        base.OnStartServer();
        //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); 
    }

    private void ActivatePlayerSpawn() {
        Vector3 pos = new Vector3();
        //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
        PosMessage m = new PosMessage() { vector2 = pos }; 
        //отправка сообщения на сервер с координатами спавна
        NetworkClient.Send(m);  
    }

    public override void OnClientConnect() {
        base.OnClientConnect();
        Debug.Log($"<color=yellow>OnClientConnect</color>");
        ActivatePlayerSpawn();
    }
}