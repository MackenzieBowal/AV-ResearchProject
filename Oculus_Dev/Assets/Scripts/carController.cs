using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Threading;
using static OVRBoundary;

public class carController : MonoBehaviour
{
    //Animators for coachman
    public Animator coachmanAnimator;
    public Animator mechCoachmanAnimator;
    public Animator capsuleCoachman1Animator;
    public Animator capsuleCoachman2Animator;
    //Animators for hand gestures
    public Animator handAnimator;
    public Animator handBatonAnimator;
    // All gestures, coachman are achieved by animation. So I use Animator to manage them.
    private Animator activatedAnimator;
    //the suffix of different coachman name
    private string[] coachmanTag = {"", "_Mech", "_Cap1","_Cap2"};
    //some magic numbers used to make coachman popup.
    private float[] popDis = {0.575f, 0.52f, 0.65f, 0.58f};

    //private AnimatorStateInfo animatorInfo;
    public GameObject centerEye;
    public GameObject[] coachmen = new GameObject[4];
    // located propertities of facial expression design
    private GameObject[] lips = new GameObject[3];
    private GameObject rightEye;
    private GameObject leftEye;
    private Quaternion rEyeRot;
    private Quaternion lEyeRot;
    private string[] armAnimations = { "Go Ahead", "Still Palm Out", "Forearm Wave", "Finger Point", "Motion Downward", "One Finger Wait" };
    private string[] batonArmAnimations = { "Sweep Sideways", "Baton Circles" };
    private GameObject hand;
    private GameObject handBaton;
    //menu
    private GameObject menu;
    private GameObject quitButton;
    //velocity controls the speed of vehicle, the original speed is 35km/h
    private Vector3 velocity = new Vector3(0.0f, 0.0f, 350/36f);
    //faceVelocity controls the speed of face showup, using localposition
    private Vector3 faceVelocity = new Vector3(0.0f, 0.0f, 0.219f);
    //It will take 3 second for the vehicle to totally stop, hence the deceleration is 3.3 m/s^2
    private Vector3 deceleration = new Vector3(0.0f, 0.0f, - 3.24f);
    //The total number of all tasks for one participant
    //private const int TOTAL_TASK_NUM = 44;
    private const int MODE_NUM = 22;
    //The task that will be showed next
    private int taskNum = -1;
    private int[] randomOrder;
    private int userID = -1;
    private int presumeCross = 0;
    private int coachmanType = -1;
    private float startTime;
    private float stopTime;
    private float TimeInSeconds;
    private Vector3 Rotation;
    private Vector3 Position;
    private Vector3[] boundaryPoints;
    //network and DB
    private MongoClient client = new MongoClient("mongodb+srv://WeiWei:AnthroWME-db@cluster0.u0268.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> posCollection;
    IMongoCollection<BsonDocument> rotCollection;
    IMongoCollection<BsonDocument> presumeCrossCollection;
    IMongoCollection<BsonDocument> userCollection;
    private bool animationFlag = false;
    //isStart is decided by the menu button:Start task
    private bool isStart = false;
    private bool threadFlag = true;
    private bool isSlowDown = false;

    private bool isEyeTrack = false;
    private bool isRestart = false;
    private bool isEndTask = false;
    private bool isBlocked = false;
    void Start()
    {
        /*
            get all the gameObject.
        */
        Debug.Log("starts: start");
        menu = GameObject.Find("Menu");
        quitButton = GameObject.Find("Menu/Quit");
        leftEye = GameObject.Find("Waymo/LeftEye");
        rightEye = GameObject.Find("Waymo/RightEye");
        lips[0] = GameObject.Find("Waymo/NormalLip");
        lips[1] = GameObject.Find("Waymo/PositiveLip");
        lips[2] = GameObject.Find("Waymo/NegativeLip");
        hand = GameObject.Find("Waymo/Arm");
        handBaton = GameObject.Find("Waymo/ArmBaton");
        // set the quitButton unvisiable
        quitButton.SetActive(false);

        //boundary points
        boundaryPoints = GameObject.Find("BoundaryCheck").GetComponent<GuardianScripts>().getBoundaryInfo();
        BoundaryCheck();
        //database and collections that will be used
        database = client.GetDatabase("myFirstDatabase");
        posCollection = database.GetCollection<BsonDocument>("crossinfops");
        rotCollection = database.GetCollection<BsonDocument>("crossinfors");
        presumeCrossCollection = database.GetCollection<BsonDocument>("presumecrossinfos");
        userCollection = database.GetCollection<BsonDocument>("userinfos");
        //Get and update the userID
        GetAndUpdateUserID();
        SetAnimator();
        Debug.Log("start function is over");
    }

    /*
        The update of rotation and position and time is done in following func.
        It also checks if the task is done.
    */
    private void Update()
    {
        Rotation = centerEye.transform.eulerAngles;
        Position = centerEye.transform.position;
        //if crossed, end task
        if (isStart&&centerEye.transform.position.x < -322)
            EndCrossing();
        //if time is out, end task(the pedestrians didn't make decisions)
        else if (isSlowDown&&Time.realtimeSinceStartup - stopTime > 11)
            EndCrossing();
    }

    /*
        The vehicle movement was done in FixedUpdate. The vehicle will start at z = -21, start to
        slowdown at z = -16 and stop at z = -1. The pos of intersection is z = 0;
    */
    void FixedUpdate()
    {   
        TimeInSeconds = Time.realtimeSinceStartup - startTime; 
        float deltaTime = Time.deltaTime;
        if(isStart)
        {  
            //moving AV
            if(velocity.z >= 0)
                this.transform.Translate(velocity * deltaTime);
            //Slow down
            if (!isSlowDown&&this.transform.position.z > -16 )
            {
                //show interfaces
                MoveInterfaces(true,deltaTime);
                velocity = velocity + deceleration * deltaTime;
                if (!animationFlag && velocity.z <= 0)
                {
                    stopTime = Time.realtimeSinceStartup;
                    //play animation
                    animationFlag = true;
                    isSlowDown = true;
                    PlayAnimation();
                }
            }
            if(isEyeTrack)
            {
                rightEye.transform.rotation = Quaternion.Slerp(rightEye.transform.rotation, Quaternion.LookRotation(centerEye.transform.position - rightEye.transform.position), 1.0f * Time.deltaTime);
                leftEye.transform.rotation = Quaternion.Slerp(leftEye.transform.rotation, Quaternion.LookRotation(centerEye.transform.position - leftEye.transform.position), 1.0f * Time.deltaTime);
            }
        }
        if(isRestart)
        {
            if(velocity.z <= 350/36)
            {
                velocity = velocity - deceleration * deltaTime;
                //hide interfaces
                MoveInterfaces(false, deltaTime);
            }
            this.transform.Translate(velocity * deltaTime);
            if(!isEndTask&&this.transform.position.z > 19)
                EndTask();
            isRestart = this.transform.position.z < 49;
        }
        if(isBlocked&&(centerEye.transform.position.x > -320 || centerEye.transform.position.x < -322))
        {
            if (centerEye.transform.position.x < -322)
                presumeCross +=1;
            isBlocked = false;
            isRestart = true;
            threadFlag = false;
        }
    }

    public void MoveInterfaces(bool isPop, float deltaTime)
    {
        int pos = randomOrder[taskNum] % MODE_NUM;
        float weight = isPop? 1: -1;
        string armAniName = isPop? "Open": "Close";
        string batonArmAniName = isPop? "OpenBaton": "CloseBaton";
        //pop up
        if(coachmanType <= 3 && coachmanType >= 0)
            {
                TriggerSeeArounnd();
                coachmen[coachmanType].transform.Translate(new Vector3(0f, weight*popDis[coachmanType] / 3.0f, 0f) * deltaTime);
            }
        //hand gestures
        if ( pos >= 16 && pos <= MODE_NUM - 2)
            activatedAnimator.Play(armAniName);
        else if (pos == MODE_NUM-1 || pos == 0)
            activatedAnimator.Play(batonArmAniName);
        else if (pos >= 10 && pos <= 15)
            {
                rightEye.transform.Translate(weight*faceVelocity * deltaTime, Space.World);
                leftEye.transform.Translate(weight*faceVelocity * deltaTime, Space.World);
                lips[0].transform.Translate(weight*faceVelocity * deltaTime, Space.World);
                lips[1].transform.Translate(weight*faceVelocity * deltaTime, Space.World);
                lips[2].transform.Translate(weight*faceVelocity * deltaTime, Space.World);
            }
    }

    /*
        play animation according to randomOrder 
    */
    public void PlayAnimation()
    {
        int pos = randomOrder[taskNum] % MODE_NUM;
        if (1 < pos&&pos < 10)
        {
            //coachman design
            int isNegative = pos % 2 ;
            TriggerCoachmanAnimation(isNegative);
        }
        else if(10 <= pos&&pos < 16)
        {
            //facial expression design
            if (13 <= pos&&pos < 16)
                isEyeTrack = true;
            else
                isEyeTrack = false;
        }
        //no baton hand gestures
        else if (pos >= 16 && pos <= MODE_NUM-2)
        {
            activatedAnimator.Play(armAnimations[pos - 16]);
        }
        else if (pos == MODE_NUM-1)
        {
            activatedAnimator.Play(batonArmAnimations[0]);
        }
        else
            activatedAnimator.Play(batonArmAnimations[1]);
    }
    public void StartTask()
    {
        taskNum++;
        //Debug.Log(taskNum);
        if (taskNum >= 2*MODE_NUM)
            {
                Debug.Log("#####Experiment is over");
                quitButton.SetActive(true);
            }
        else
        {
            SetAnimator();
            menu.SetActive(false);
            isStart = true;
            animationFlag = false;
            isSlowDown = false;
            isBlocked = false;
            isRestart = false;
            isEndTask = false;
            velocity = new Vector3(0.0f, 0.0f, 350/36f);
            startTime = Time.realtimeSinceStartup;
            this.transform.localPosition = new Vector3(-321.4f, 71.7f, -21.0f);
            SendDataToDB();
        }
    }

    public async void Quit()
    {
        /*Will not write data in local file any more
            string filepath = GetAndroidExternalFilesDir();
            Debug.Log(filepath);
            WriteToCSV(filepath + "/TTD.txt");
            Debug.Log("*****Quit");
            Debug.Log(dataList);
        */
        if (userID!=-1)
        {var crossData = new BsonDocument { { "userID", userID }, { "presumeCross", presumeCross} };
        await presumeCrossCollection.InsertOneAsync(crossData);}
        Application.Quit();
    }
    
    
    public void TriggerSeeArounnd()
    {
        activatedAnimator.Play("SeeAround" + coachmanTag[coachmanType]);
        activatedAnimator.Update(0);
        GetAnimatorInfo();
    }
    
    //pop up the coachman at the slowdown process
    public void TriggerCoachmanAnimation(int isNegative)
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
         GetAnimatorInfo();
    }

    public void SendDataToDB()
    {
        threadFlag = true;
        Thread thread = new Thread(new ThreadStart(this.timer));
        thread.Start();
    }
    public async void SaveCrossInfoToDB()
    {
        var posData = new BsonDocument { { "userID", userID }, { "taskID", randomOrder[taskNum] }, { "x", Position.x }, { "y", Position.y },{ "z", Position.z }, { "timeInSeconds", TimeInSeconds } };
        var rotData = new BsonDocument { { "userID", userID }, { "taskID", randomOrder[taskNum] }, { "rotationX", Rotation.x }, { "rotationY", Rotation.y }, { "rotationZ", Rotation.z }, { "timeInSeconds", TimeInSeconds } };
        await posCollection.InsertOneAsync(posData);
        await rotCollection.InsertOneAsync(rotData);
    }

    public async void GetAndUpdateUserID()
    {
        var userInfo = userCollection.FindAsync(new BsonDocument());
        var userInfoAwaited = await userInfo;
        var userIDJson = userInfoAwaited.ToList()[0].ToString();
        int startPos = userIDJson.IndexOf("),", 0) + 2;
        int beginPos = userIDJson.IndexOf(": ", startPos);
        string userId = "";
        for (int j = beginPos + 2; j < userIDJson.Length - 2; j++)
            userId = userId + userIDJson[j];
        userID = int.Parse(userId);
        var filter = Builders<BsonDocument>.Filter.Eq("userID", userID);
        var update = Builders<BsonDocument>.Update.Set("userID", userID + 1);
        await userCollection.UpdateOneAsync(filter, update);
        //Generate the order for each participant and save it to database
        //randomOrder = GetRandomList(TOTAL_TASK_NUM);
        randomOrder = GetCounterBalancedOrder();
        //SaveOrderInfoToDB();
    }
    
    public void timer()
    {
        //Thread.CurrentThread.IsBackground = true;
        while (threadFlag)
        {
            Thread.CurrentThread.Join(100);
            SaveCrossInfoToDB();
        }
    }

    public void BoundaryCheck()
    {
        //get the boundary info
        // float x1 = 0f, y1 = 0f, z1 = 0f,x2 = 0f, y2 = 0f, z2 = 0f;
        float maxDis = 0f;
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            // if (boundaryPoints[i].x > x1)
            //     x1 = boundaryPoints[i].x;
            // if (boundaryPoints[i].x < x2)
            //     x2 = boundaryPoints[i].x;
            // if (boundaryPoints[i].y > y1)
            //     y1 = boundaryPoints[i].y;
            // if (boundaryPoints[i].y < y2)
            //     y2 = boundaryPoints[i].y;
            // if (boundaryPoints[i].z > z1)
            //     z1 = boundaryPoints[i].z;
            // if (boundaryPoints[i].z < z2)
            //     z2 = boundaryPoints[i].z;
            float temp = (float)(Math.Pow(boundaryPoints[i].x, 2) + Math.Pow(boundaryPoints[i].z, 2));
            if (maxDis < temp)
                maxDis = temp;
        }
        if (maxDis < (2.5 * 2.5))
        {
            Quit();
        }
    }

    public int[] GetRandomList(int maxnum)
    {
        System.Random ran = new System.Random();
        int[] ans = new int[maxnum];
        for (int i = 1; i < maxnum+1; i++)
            ans[i-1] = i;
        while(maxnum>=1)
        {
            int temp = ran.Next(0, maxnum);
            int te = ans[temp];
            ans[temp] = ans[maxnum - 1];
            ans[maxnum - 1] = te;
            maxnum--;
        }
        return ans;
    }

    public int[] GetCounterBalancedOrder()
    {
        //most stupid hard code, because I don't wanna read file :(
        int[,] balancedOrder = 
        {{1,22,2,21,3,20,4,19,5,18,6,17,7,16,8,15,9,14,10,13,11,12,2,1,3,22,4,21,5,20,6,19,7,18,8,17,9,16,10,15,11,14,12,13},
        {3,2,4,1,5,22,6,21,7,20,8,19,9,18,10,17,11,16,12,15,13,14,4,3,5,2,6,1,7,22,8,21,9,20,10,19,11,18,12,17,13,16,14,15},
        {5,4,6,3,7,2,8,1,9,22,10,21,11,20,12,19,13,18,14,17,15,16,6,5,7,4,8,3,9,2,10,1,11,22,12,21,13,20,14,19,15,18,16,17},
        {7,6,8,5,9,4,10,3,11,2,12,1,13,22,14,21,15,20,16,19,17,18,8,7,9,6,10,5,11,4,12,3,13,2,14,1,15,22,16,21,17,20,18,19},
        {9,8,10,7,11,6,12,5,13,4,14,3,15,2,16,1,17,22,18,21,19,20,10,9,11,8,12,7,13,6,14,5,15,4,16,3,17,2,18,1,19,22,20,21},
        {11,10,12,9,13,8,14,7,15,6,16,5,17,4,18,3,19,2,20,1,21,22,12,11,13,10,14,9,15,8,16,7,17,6,18,5,19,4,20,3,21,2,22,1},
        {13,12,14,11,15,10,16,9,17,8,18,7,19,6,20,5,21,4,22,3,1,2,14,13,15,12,16,11,17,10,18,9,19,8,20,7,21,6,22,5,1,4,2,3},
        {15,14,16,13,17,12,18,11,19,10,20,9,21,8,22,7,1,6,2,5,3,4,16,15,17,14,18,13,19,12,20,11,21,10,22,9,1,8,2,7,3,6,4,5},
        {17,16,18,15,19,14,20,13,21,12,22,11,1,10,2,9,3,8,4,7,5,6,18,17,19,16,20,15,21,14,22,13,1,12,2,11,3,10,4,9,5,8,6,7},
        {19,18,20,17,21,16,22,15,1,14,2,13,3,12,4,11,5,10,6,9,7,8,20,19,21,18,22,17,1,16,2,15,3,14,4,13,5,12,6,11,7,10,8,9},
        {21,20,22,19,1,18,2,17,3,16,4,15,5,14,6,13,7,12,8,11,9,10,22,21,1,20,2,19,3,18,4,17,5,16,6,15,7,14,8,13,9,12,10,11}
        };
        int[] ans = new int[MODE_NUM*2];
        for(int i = 0; i <  MODE_NUM*2; i++)
        {
            ans[i] = balancedOrder[userID-1,i];
        }
        return ans;
    }

    public void SetAnimator()
    {
        int pos = randomOrder[taskNum] % MODE_NUM;
        Debug.Log("pos is :" + pos);
        rightEye.SetActive(pos >= 10 && pos <= 15);
        leftEye.SetActive(pos >= 10 && pos <= 15);
        lips[0].SetActive(pos == 10 || pos == 13);
        lips[1].SetActive(pos == 11 || pos == 14);
        lips[2].SetActive(pos == 12 || pos == 15);
        hand.SetActive(pos >= 16 && pos <= MODE_NUM -2 );
        handBaton.SetActive(pos == 0 || pos == MODE_NUM-1);

        //disable all unchosen coachman
        coachmanType = pos / 2 - 1;
        for(int i = 0; i < 4; i++)
        {
            coachmen[i].SetActive(coachmanType == i);
        }
        if (coachmanType == 0)
            activatedAnimator = coachmanAnimator;
        else if (coachmanType == 1)
            activatedAnimator = mechCoachmanAnimator;
        else if (coachmanType == 2)
            activatedAnimator = capsuleCoachman1Animator;
        else if (coachmanType == 3)
            activatedAnimator = capsuleCoachman2Animator;
        else if (pos >= 16 && pos <= MODE_NUM-2)
        {
            activatedAnimator = handAnimator;
            activatedAnimator.Play("Start");
        }
        else if (pos == MODE_NUM -1 || pos == 0)
        {
            activatedAnimator = handBatonAnimator;
            activatedAnimator.Play("StartBaton");
        }


    }

    // time is over or crossing is over
    public void EndCrossing()
    {
        isStart = false;
        isSlowDown = false;
        // road is blocked by pedestrians
        if (centerEye.transform.position.x > -322 && centerEye.transform.position.x < -320)
        {
            isBlocked = true;
        }
        else
        {
            isRestart = true;
            threadFlag = false;
        }
    }

    public void EndTask()
    {
        menu.SetActive(true);
        isEndTask = true;
        isEyeTrack = false;
        coachmen[0].transform.localPosition = new Vector3(0.51f, -1.35f,-0.4f);
        coachmen[1].transform.localPosition = new Vector3(0.5f, -2f,-0.45f);
        coachmen[2].transform.localPosition = new Vector3(0.5f, -1.7f,-0.42f);
        coachmen[3].transform.localPosition = new Vector3(0.5f, -1.38f,-0.42f);
        lips[0].transform.localPosition = new Vector3(-0.006f, 0.35f,-0.95f);
        lips[1].transform.localPosition = new Vector3(0.01f, 0.28f,-0.95f);
        lips[2].transform.localPosition = new Vector3(-0.012f, 0.438f,-0.995f);
        rightEye.transform.localPosition = new Vector3(-0.51f, 0.65f, -0.7f);
        leftEye.transform.localPosition = new Vector3(0.51f, 0.65f, -0.7f);
        rightEye.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        leftEye.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
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
    void GetAnimatorInfo()
    {
        string name = activatedAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        Debug.Log("The name of the animation that is played now:" + name);
        float length = activatedAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        Debug.Log("The length of the animation that is played now:" + length);
    }
}
