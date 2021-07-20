using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading;

public class crossInfoRequest : MonoBehaviour
{

    MongoClient client = new MongoClient("mongodb+srv://WeiWei:AnthroWME-db@cluster0.u0268.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;
    bool flag = true;

    void Start()
    {
        database = client.GetDatabase("myFirstDatabase");
        collection = database.GetCollection<BsonDocument>("userinfos");
    }

    public void GetInfo()
    {
        Debug.Log("button pressed");
        getUserInfo();
        flag = false;
    }

    public void PostInfo()
    {
        SaveCrossInfoToDB(1);
    }

    public async void SaveCrossInfoToDB(int userID)
    {
        //hard code here for now
        var document = new BsonDocument{ {"userID", userID }, {"x", 220}, {"z", 41}, {"timeInSeconds",0} };
        await collection.InsertOneAsync(document);
    }

    public void SendDataToDB()
    {
        flag = true;
        Thread thread = new Thread(new ThreadStart(this.timer));
        thread.Start();
    }

    public async void getUserInfo()
    {
        var allCrossInfo = collection.FindAsync(new BsonDocument());
        var crossInfoAwaited = await allCrossInfo;
        Debug.Log("received info from DB");
        foreach (var crossInfo in crossInfoAwaited.ToList())
        {
            Debug.Log(crossInfo.ToString());
        }
    }

    public async Task<List<CrossInfoP>> GetCrossInfoFromDB()
    {
        var allCrossInfo = collection.FindAsync(new BsonDocument());
        var crossInfoAwaited = await allCrossInfo;

        Debug.Log("received info from DB");
        List<CrossInfoP> crossInfos = new List<CrossInfoP>();
        foreach (var crossInfo in crossInfoAwaited.ToList())
        {
            /*
            The structure of crossInfo is like this(in string version):
            { "_id" : ObjectId("60e7b7d166e2e5721c7c2fea"), 
            "userID" : 1, 
            "x" : 0, 
            "z" : 0, 
            "timeInSeconds" : 10, 
            "createdAt" : ISODate("2021-07-09T02:43:29.937Z"), 
            "updatedAt" : ISODate("2021-07-09T02:43:29.937Z"), 
            "__v" : 0 }
            */
            crossInfos.Add(Deserialize(crossInfo.ToString()));
        }

        return crossInfos;
    }


    private CrossInfoP Deserialize(string rawJson)
    {
        var crossInfo = new CrossInfoP();
        int startPos = rawJson.IndexOf("),", 0) + 2;
        string[] values = new string[4];
        int i = 0;
        int beginPos = 0;
        int endPos = 0;
        while (i < 4)
        {
            beginPos = rawJson.IndexOf(" :", startPos);
            endPos = rawJson.IndexOf(",", startPos);
            if (endPos == -1)
                endPos = rawJson.Length - 1;
            startPos = endPos + 2;
            for(int j = beginPos + 2; j < endPos; j++)
                values[i] = values[i] + rawJson[j];
            //Debug.Log(rawJson.Substring(beginPos + 1, endPos));
            i = i + 1;
        }
        Debug.Log(values[0]);
        Debug.Log(values[1]);
        Debug.Log(values[2]);
        crossInfo.userID = int.Parse(values[0]);
        crossInfo.x = int.Parse(values[1]);
        crossInfo.z = int.Parse(values[2]);
        crossInfo.timeInSeconds = int.Parse(values[3]);
        return crossInfo;
    }
    //inline class
    public class CrossInfoP
    {
        public int userID {get; set;}
        public int x {get; set;}
        public int z {get; set;}
        public int timeInSeconds {get; set;}
    }

    public void timer()
    {
        int userID = 11;
        //Thread.CurrentThread.IsBackground = true;
        while (flag)
        {
            Thread.CurrentThread.Join(10000);
            SaveCrossInfoToDB(userID);
            userID++;
        }
    }
}
