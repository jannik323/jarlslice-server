using System;
using System.Threading;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;

internal enum ClientToServerId : ushort {
    JoinPlayer = 1,
    PlayerPosition = 2,
    TextMessage = 3,
    SceneChange = 4,
    NameChange = 5,
    LevelFinish = 6,
    ColorChange = 7,
    RequestCustomLevel = 8,
    LEObjectCreate = 9,
    LEObjectEdit = 10,
    LEObjectDelete = 11,
}

internal enum ServerToPlayerId : ushort {
    JoinPlayer = 1,
    PlayerPosition = 2,
    TextMessage = 3,
    SceneChange = 4,
    NameChange = 5,
    ColorChange = 6,
    HostSettings = 7,
    CustomLevel = 9,
    CustomLevelLeave = 10,
    LEObjectCreate = 11,
    LEObjectEdit = 12,
    LEObjectDelete = 13,

}

public enum HostSetting : ushort {
    canPlayerChangeColor = 0,
    canPlayerCollide,
}

public class NetworkManager {

    static void Main(string[] args) {
        NetworkManager ntwrk = new NetworkManager();
        if (args == null) {
            Console.WriteLine("args is null");
        } else {
            // Step 2: print length, and loop over all arguments.
            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "cCollide":
                        ntwrk.canPlayerCollide = true;
                        break;
                    case "cColor":
                        ntwrk.canPlayerColorSelect = true;
                        break;
                    case "msgF":
                        ntwrk.msgonLevelFinish = true;
                        break;
                    case "msgT":
                        ntwrk.msgOnTouch = true;
                        break;
                }
            }
        }
        ntwrk.start();
    }
    public static NetworkManager Singleton { get; private set; }

    public Server Server { get; private set; }

    private ushort port = 33333;
    private ushort maxPlayers = 20;

    private ushort CurrentTick = 0;
    private ushort sendRate = 10;
    public int maxUsernameLength { get; private set; } = 20;
    public int maxTextMessageLength { get; private set; } = 100;

    public bool msgOnTouch = false;
    public bool msgonLevelFinish = false;
    public bool canPlayerColorSelect = false;
    public bool canPlayerCollide= false;
    public NetworkManager() {
        Singleton = this;
        Server = new Server();
    }
    public void start() {
        Console.Out.WriteLine("Starting Server with following config:");
        Console.Out.WriteLine("");
        Console.Out.WriteLine("Player Color Select: " + canPlayerColorSelect.ToString());
        Console.Out.WriteLine("Player Collision: " + canPlayerCollide.ToString());
        Console.Out.WriteLine("Msg On Touch: " + msgOnTouch.ToString());
        Console.Out.WriteLine("Msg On Finish: " + msgonLevelFinish.ToString());

        Server.Start(port, maxPlayers);
        Console.Out.WriteLine("");
        Console.Out.WriteLine("Server started!");

        Server.ClientDisconnected += Server_ClientDisconnected;
        
        RiptideLogger.Initialize(Console.Out.WriteLine, true);
        NetUpdate();
    }

    private void Server_ClientDisconnected(object sender, ClientDisconnectedEventArgs e) {
        Player.Remove(e.Id);
    }

    void NetUpdate() {
        while (true) {
            Server.Tick();
            if (CurrentTick % sendRate == 0) {
                SendPositions();
            }
            CurrentTick++;
        }
    }
    private void SendPositions() {
        foreach (Player player in Player.list.Values) {
            Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToPlayerId.PlayerPosition);
            message.AddUShort(player.Id);
            message.AddVector3(player.transform.position);
            message.AddVector3(player.transform.rotation);
            Server.SendToAll(message, player.Id);
        }
    }

    public void SendHostSetting(HostSetting hostSetting, bool value) {
        switch (hostSetting) {
            case HostSetting.canPlayerChangeColor:
                canPlayerColorSelect = value;
                break;
            case HostSetting.canPlayerCollide:
                canPlayerCollide = value;
                break;
        }
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.HostSettings);
        message.AddUShort(((ushort)hostSetting));
        message.AddBool(value);
        Server.SendToAll(message);
    }

    public void PlayerCollision(string username1, string username2) {
        if (!msgOnTouch) return;
        SendTextMessage(username1 + " has touched " + username2, true);
    }
    private void SendTextMessage(string textmessage, bool system = false) {
        Server.SendToAll(Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.TextMessage).AddString(textmessage).AddBool(system));
    }

    [MessageHandler((ushort)ClientToServerId.LevelFinish)]
    private static void PlayerLevelFinish(ushort fromClientId, Message message) {
        if (!NetworkManager.Singleton.msgonLevelFinish) return;
        string level = message.GetString();
        float time = message.GetFloat();
        NetworkManager.Singleton.SendTextMessage(Player.getUserName(fromClientId) + " has finished " + level + " in: " + TimeSpan.FromSeconds(time).ToString(@"mm\:ss\.fff"), true);
    }

    [MessageHandler((ushort)ClientToServerId.JoinPlayer)]
    private static void PlayerSpawn(ushort fromClientId, Message message) {
        ushort Id = message.GetUShort();
        string username = message.GetString();
        if (username.Length > NetworkManager.Singleton.maxUsernameLength) {
            username = username.Substring(0, NetworkManager.Singleton.maxUsernameLength);
        }
        NetworkManager.Singleton.SendTextMessage(username + " has joined", true);

        Message newmessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.JoinPlayer);
        newmessage.AddUShort(Id);
        newmessage.AddString(username);
        newmessage.AddString("Menu"); // hardcoded lollol
        newmessage.AddColor(new Color(.7f, .5f, 0, .5f));
        NetworkManager.Singleton.Server.SendToAll(newmessage, fromClientId);

        foreach (Player player in Player.list.Values) {
            Message jmessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.JoinPlayer);
            jmessage.AddUShort(player.Id);
            jmessage.AddString(player.username);
            jmessage.AddString(player.scene);
            jmessage.AddColor(player.color);
            NetworkManager.Singleton.Server.Send(jmessage, fromClientId);
        }

        Player.PlayerJoin(Id, username, "Menu", new Color(.7f, .5f, 0, .5f));

        NetworkManager.Singleton.SendHostSetting(HostSetting.canPlayerChangeColor, NetworkManager.Singleton.canPlayerColorSelect);
        NetworkManager.Singleton.SendHostSetting(HostSetting.canPlayerCollide, NetworkManager.Singleton.canPlayerCollide);
    }

    [MessageHandler((ushort)ClientToServerId.NameChange)]
    private static void PlayerNameChange(ushort fromClientId, Message message) {
        string username = message.GetString();
        if (username.Length > NetworkManager.Singleton.maxUsernameLength) {
            username = username.Substring(0, NetworkManager.Singleton.maxUsernameLength);
        }
        Message newmessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.NameChange);
        newmessage.AddUShort(fromClientId);
        newmessage.AddString(username);
        Player.getUser(fromClientId).setUserName(username);
        NetworkManager.Singleton.Server.SendToAll(newmessage);
        NetworkManager.Singleton.SendTextMessage(Player.getUserName(fromClientId) + " has changed their name to " + username, true);
    }

    [MessageHandler((ushort)ClientToServerId.PlayerPosition)]
    private static void PlayerPosition(ushort fromClientId, Message message) {
        Vector3 pos = message.GetVector3();
        Vector3 rot = message.GetVector3();
        Player.getUser(fromClientId).setTransform(pos,rot);
    }

    [MessageHandler((ushort)ClientToServerId.TextMessage)]
    private static void PlayerTextMessage(ushort fromClientId, Message message) {
        string user = message.GetString();
        string text = message.GetString();
        if (text.Length >= NetworkManager.Singleton.maxTextMessageLength) {
            text = text.Substring(0, NetworkManager.Singleton.maxTextMessageLength);
        };
        NetworkManager.Singleton.SendTextMessage(user + ": " + text);
    }

    [MessageHandler((ushort)ClientToServerId.SceneChange)]
    private static void PlayerSceneChange(ushort fromClientId, Message message) {
        Message newmessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.SceneChange);
        newmessage.AddUShort(fromClientId);
        string scene = message.GetString();
        newmessage.AddString(scene);
        Player.getUser(fromClientId).setScene(scene);
        NetworkManager.Singleton.Server.SendToAll(newmessage, fromClientId);
    }

    [MessageHandler((ushort)ClientToServerId.ColorChange)]
    private static void PlayerColorChange(ushort fromClientId, Message message) {
        if (!NetworkManager.Singleton.canPlayerColorSelect) return;
        Message newmessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToPlayerId.ColorChange);
        newmessage.AddUShort(fromClientId);
        Color color = message.GetColor();
        newmessage.AddColor(color);
        Player.getUser(fromClientId).setColor(color);
        NetworkManager.Singleton.Server.SendToAll(newmessage, fromClientId);
    }
}

