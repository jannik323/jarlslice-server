using RiptideNetworking;
using System.Collections.Generic;
using System.Drawing;

public class Player{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public ushort Id { get; protected set; }
    public string username { get; protected set; }
    public string scene { get; protected set; } = "None";
    public Transform transform { get; protected set; }
    public Color color { get; protected set; }

    public static void PlayerJoin(ushort id, string username, string scene, Color color) {
        if (!list.ContainsKey(id)) {
            Player player = new Player {
                transform = new Transform()
            };
            player.Id = id;
            player.color = color;
            player.username = username;
            player.scene = scene;
            list.Add(id, player);
        }
    }

    public static void Remove(ushort id) {
        list.Remove(id);
    }

    public static string getUserName(ushort Id){
        if(list.TryGetValue(Id,out Player player)){
            return player.username;
        }
        return null;
    }

    public static Player getUser(ushort Id) {
        if (list.TryGetValue(Id, out Player player)) {
            return player;
        }
        return null;
    }

    public void setScene(string scene){
        this.scene=scene;
    }

    public void setColor(Color color){
        this.color=color;
    }

    public static int getScenePlayerCount(string scene){
        int count = 0;
        foreach (Player player in list.Values){
            if(player.scene==scene)count++;
        }
        return count;
    }

    public void setUserName(string username){
        this.username=username;
    }

    public void setTransform(Vector3 position,Vector3 rotation) {
        this.transform.position = position;
        this.transform.rotation = rotation;
    }
    public static void clearPlayer(){
        list.Clear();
    }


    
}

public class Transform {
    public Transform() {
        this.position = Vector3.Zero;
        this.rotation = Vector3.Zero;
    }
    public Vector3 position;
    public Vector3 rotation;
}

