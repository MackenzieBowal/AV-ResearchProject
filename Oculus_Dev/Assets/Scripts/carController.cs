using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Threading;

public class carController : MonoBehaviour
{
    //Animators for coachman
    public Animator coachmanAnimator;
    public Animator mechCoachmanAnimator;
    public Animator capsuleCoachman1Animator;
    public Animator capsuleCoachman2Animator;
    //Animators for hand gestures-TBA
    public Animator handAnimator;
    // All gestures, ccoachman movements are achieved by animation. So I use Animator to manage them.
    private Animator activatedAnimator;
    /*
        coachmantype decides which coachman is activated:
        1:human-like coachman(Latifa), 2:mechCoachman(Robot Kyle), 3:CapsuleCoachman1, 4:CapsuleCoachman2
    */

    //private AnimatorStateInfo animatorInfo;

    public GameObject OVRCamera;
    private GameObject coachman;
    private GameObject mechCoachman;
    private GameObject capsuleCoachman1;
    private GameObject capsuleCoachman2;

    private Transform zebraLine;

    private GameObject city;
    // located propertities of facial expression design
    private GameObject[] lips = new GameObject[3];
    private GameObject rightEye;
    private GameObject leftEye;
    private GameObject hand;
    //menu
    private GameObject menu;
    //movement controls the speed of vehicle, the original speed is 40km/h,or 35km/h
    private Vector3 movement = new Vector3(0.0f, 0.0f, 0.2f);
    private string[] coachmanTag = {"", "_Mech", "_Cap1","_Cap2"};
    //The total number of all tasks for one participant
    private const int TOTAL_TASK_NUM = 46;
    //The task that will be showed next
    private int taskNum = -1;
    private float TTD = 0;
    private bool animationFlag = false;
    //isStart is decided by the menu button:Start task
    private bool isStart = false;
    private int[] randomOrder;
    private int userID;
    private float startTime;
    private double slowDownTime = 0.5f;
    //network and DB
    MongoClient client = new MongoClient("mongodb+srv://WeiWei:AnthroWME-db@cluster0.u0268.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> posCollection;
    IMongoCollection<BsonDocument> rotCollection;
    IMongoCollection<BsonDocument> userCollection;
    bool threadFlag = true;
    void Start()
    {
        startTime = Time.realtimeSinceStartup;

        menu = GameObject.Find("Menu");

        coachman = GameObject.Find("CoachManAV/Coachman");
        mechCoachman = GameObject.Find("CoachManAV/MechCoachman");
        capsuleCoachman1 = GameObject.Find("CoachManAV/CapsuleCoachman1");
        capsuleCoachman2 = GameObject.Find("CoachManAV/CapsuleCoachman2");
        // located the zebraline
        Transform props = city.transform.Find("props");
        zebraLine = props.transform.Find("Street 8 Prefab (6)");

        leftEye = GameObject.Find("Waymo/LeftEye");
        rightEye = GameObject.Find("Waymo/RightEye");
        lips[0] = GameObject.Find("Waymo/NormalLip");
        lips[1] = GameObject.Find("Waymo/PositiveLip");
        lips[2] = GameObject.Find("Waymo/NegativeLip");
        //animationClip = activatedAnimator.runtimeAnimatorController.animationClips;

        //database and collections that will be used
        database = client.GetDatabase("myFirstDatabase");
        posCollection = database.GetCollection<BsonDocument>("crossinfops");
        rotCollection = database.GetCollection<BsonDocument>("crossinfors");
        userCollection = database.GetCollection<BsonDocument>("userinfo");
        //Get the userID
        GetUserIDFromDB();
        //After get the userID, update it(plus 1) and save the update to DB
        UpdateUserID();
        //Generate the order for each participant
        randomOrder = GetRandomList(TOTAL_TASK_NUM);

        SetAnimator();
        //start to record data and send it to DB
        SendDataToDB();
    }

    private void Update()
    {
        //if crossed, end task
        if (OVRCamera.transform.localPosition.x < -328)
            EndTask();
        //if time is out, end task(the pedestrians didn't make decisions)
        else if (Time.realtimeSinceStartup - startTime > 33)
            EndTask();
    }
    void FixedUpdate()
    {        
        if(isStart)
        {  
            //moving AV
            this.transform.Translate(movement);
            //Get the square length of two object vectors
            float sqrLenght = (zebraLine.position - this.transform.position).sqrMagnitude;
            //Slow down
            if (sqrLenght < 100 * 100 )
                {
                //slow down, the slowdown function is Quadratic function : speed(m/s) = 14/9t^2 - 28/3t + 14, which make sure the car will stop at 3rd second, align with Chang.et.al' work
                slowDownTime += 0.02f;
                movement.z = (float)((14 / 9) *Math.Pow(slowDownTime, 2) - (28/3)* slowDownTime + 14);
                //TBA: pop up
                if (!animationFlag&& movement.z == 0)
                {
                    //play animation
                    animationFlag = true;
                    PlayAnimation();
                }
            }
            //animatorInfo = activatedAnimator.GetCurrentAnimatorStateInfo(0);
            //normalizedTime decides if the animation ends or not. if normalizedTime > 1, it's ends
            //if ((animatorInfo.normalizedTime >= 1.0f) && (animatorInfo.IsName("Negative" + coachmanTag[coachmanType])))
            //TriggerSeeArounnd();
        }
    }

    // play animation according to randomOrder
    
    public void PlayAnimation()
    {
        int pos = randomOrder[taskNum] % 23;
        if (1 < pos&&pos < 10)
        {
            //coachman design
            int coachmanType = pos / 2 - 1;
            int isNegative = pos % 2 ;
            TriggerCoachmanAnimation(coachmanType, isNegative);
        }
        else if(10 <= pos&&pos < 16)
        {
            //facial expression design
            EyeTrack(pos > 12);
        }
        else if (pos >= 16)
        {
            // do the hand gesture
        }
    }
    public void StartTask()
    {
        taskNum++;
        if (taskNum >= TOTAL_TASK_NUM)
            Debug.Log("Experiment is over");
            //show experiment over tips
        else
        {
            menu.SetActive(false);
            isStart = true;
            animationFlag = false;
            movement.z = 0.05f;
            this.transform.Translate(new Vector3(0.0f, 0.0f, -20.0f));
        }
    }

    public void Quit()
    {
        /*Will not write data in local file any more
            string filepath = GetAndroidExternalFilesDir();
            Debug.Log(filepath);
            WriteToCSV(filepath + "/TTD.txt");
            Debug.Log("*****Quit");
            Debug.Log(dataList);
        */
        Application.Quit();
    }
    
    /*
    public void TriggerSeeArounnd()
    {
        Debug.Log("see around");
        //activatedAnimator.SetTrigger("SeeAroundTrigger");
        activatedAnimator.Play("SeeAround" + coachmanTag[coachmanType]);   //播放动画
        activatedAnimator.Update(0);
        if (coachmanType == 2)
        {
            activatedAnimator.Play("Default"); 
            activatedAnimator.Update(1);
        }
    }
    */

    public void TriggerCoachmanAnimation(int coachmanType, int isNegative)
    {
        if(isNegative == 0)
            activatedAnimator.Play("Positive" + coachmanTag[coachmanType]);
        else
            activatedAnimator.Play("Negative" + coachmanTag[coachmanType]);  
        activatedAnimator.Update(0);
        if (coachmanType == 2)
        {
            if (isNegative == 0)
                activatedAnimator.Play("Happy");
            else
                activatedAnimator.Play("Angry"); 
            activatedAnimator.Update(1);      
        }
    }

    public void SendDataToDB()
    {
        threadFlag = true;
        Thread thread = new Thread(new ThreadStart(this.timer));
        thread.Start();
    }
    public async void SaveCrossInfoToDB()
    {
        Vector3 rotation = OVRCamera.transform.localEulerAngles;
        Vector3 position = OVRCamera.transform.localPosition;
        float timeInSeconds = Time.realtimeSinceStartup - startTime;
        var posData = new BsonDocument { { "userID", userID }, { "x", position.x }, { "z", position.z }, { "timeInSeconds", timeInSeconds } };
        var rotData = new BsonDocument { { "userID", userID }, { "rotationX", rotation.x }, { "rotationY", rotation.y }, { "rotationZ", rotation.z }, { "timeInSeconds", timeInSeconds } };
        await posCollection.InsertOneAsync(posData);
        await rotCollection.InsertOneAsync(rotData);
    }
    public async void GetUserIDFromDB()
    {
        var userInfo = userCollection.FindAsync(new BsonDocument());
        var userInfoAwaited = await userInfo;

        Debug.Log("received info from DB");
        var userIDJson = userInfoAwaited.ToList()[0].ToString();

        int startPos = userIDJson.IndexOf("),", 0) + 2;
        int beginPos = userIDJson.IndexOf(" :", startPos);
        int endPos = userIDJson.IndexOf(",", startPos);
        string userId = "";
        for (int j = beginPos + 2; j < endPos; j++)
            userId = userId + userIDJson[j];
        userID = int.Parse(userId);
    }

    public async void UpdateUserID()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("userID", userID);
        var update = Builders<BsonDocument>.Update.Set("userID", userID + 1);
        userCollection.UpdateOne(filter, update);
    }
    public void timer()
    {
        //Thread.CurrentThread.IsBackground = true;
        while (threadFlag)
        {
            Thread.CurrentThread.Join(10000);
            SaveCrossInfoToDB();
        }
    }

    //TBA
    private void EyeTrack(bool isTrack)
    {
        if(isTrack)
        {
            // do the track
        }
    }

    public int[] GetRandomList(int maxnum)
    {
        System.Random ran = new System.Random();
        int[] ans = new int[maxnum];
        for (int i = 1; i < maxnum+1; i++)
            ans[i-1] = i;
        while(maxnum>1)
        {
            int temp = ran.Next(0, maxnum);
            ans[temp] ^= ans[maxnum - 1];
            ans[maxnum - 1] ^= ans[temp];
            ans[temp] ^= ans[maxnum - 1];
            maxnum--;
        }
        return ans;
    }

    public void SetAnimator()
    {
        int pos = randomOrder[taskNum] % 23;
        rightEye.SetActive(pos >= 10 && pos <= 15);
        leftEye.SetActive(pos >= 10 && pos <= 15);
        lips[0].SetActive(pos == 10 || pos == 13);
        lips[1].SetActive(pos == 11 || pos == 14);
        lips[2].SetActive(pos == 12 || pos == 15);

        hand.SetActive(pos >= 16);
        //disable all unchosen coachman
        int animatorType = pos / 2 - 1;
        coachman.SetActive(animatorType == 0);
        mechCoachman.SetActive(animatorType == 1);
        capsuleCoachman1.SetActive(animatorType == 2);
        capsuleCoachman2.SetActive(animatorType == 3);
        if (animatorType == 0)
            activatedAnimator = coachmanAnimator;
        else if (animatorType == 1)
            activatedAnimator = mechCoachmanAnimator;
        else if (animatorType == 2)
            activatedAnimator = capsuleCoachman1Animator;
        else if (animatorType == 3)
            activatedAnimator = capsuleCoachman2Animator;
        else //TBA: hand gesture
            ;
    }

    //when one task ends
    public void EndTask()
    {
        threadFlag = false;
        isStart = false;
        //save the waiting time&crossing time to DB
        //relocate the pedestrian
        OVRCamera.transform.position = new Vector3(-328, (float)71.4, (float)0.72);
        //show the menu
        menu.SetActive(true);
    }

    //This method will not be called
    /*    public void WriteToCSV(string fileName)
        {
            if ( fileName.Length > 0)
            {
                //FileStream fs = new FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    //write data
                    string dataHeard = string.Empty;
                    dataHeard = "Participant ID,TTD,IsRight";
                    sw.WriteLine(dataHeard);
                    foreach(double time in dataList)
                    {
                        string dataStr = participandID.ToString() + ",";
                        dataStr += time.ToString();
                        sw.WriteLine(dataStr);
                    }
                }

            }
        }*/

    /*Used to access the file system in oculus. But since we are going to do the experiment online,
     * this method will be not called
    */
    private static string GetAndroidExternalFilesDir()
    {
     using (AndroidJavaClass unityPlayer = 
            new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
     {
          using (AndroidJavaObject context = 
                 unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
          {
               // Get all available external file directories (emulated and sdCards)
               AndroidJavaObject[] externalFilesDirectories = 
                                   context.Call<AndroidJavaObject[]>
                                   ("getExternalFilesDirs", (object)null);

               AndroidJavaObject emulated = null;
               AndroidJavaObject sdCard = null;

               for (int i = 0; i < externalFilesDirectories.Length; i++)
               {
                    AndroidJavaObject directory = externalFilesDirectories[i];
                    using (AndroidJavaClass environment = 
                           new AndroidJavaClass("android.os.Environment"))
                    {
                        // Check which one is the emulated and which the sdCard.
                        bool isRemovable = environment.CallStatic<bool>
                                          ("isExternalStorageRemovable", directory);
                        bool isEmulated = environment.CallStatic<bool>
                                          ("isExternalStorageEmulated", directory);
                        if (isEmulated)
                            emulated = directory;
                        else if (isRemovable && isEmulated == false)
                            sdCard = directory;
                    }
               }
               // Return the sdCard if available
               if (sdCard != null)
                    return sdCard.Call<string>("getAbsolutePath");
               else
                    return emulated.Call<string>("getAbsolutePath");
            }
      }
    }
    // void GetAnimatorInfo()
    // {
    //     string name = activatedAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;//获取当前播放动画的名称
    //     Debug.Log("当前播放的动画名为：" + name);
    //     float length = activatedAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;//获取当前动画的时间长度
    //     Debug.Log("播放动画的长度：" + length);
    // }
}
