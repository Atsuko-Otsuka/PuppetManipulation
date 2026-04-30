using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

using System;

public class KeyLog
{
    public KeyLog() { }
    public KeyLog(string _key, string _timeStamp, float _elapsedTime) {
        key = _key;
        timeStamp = _timeStamp;
        elapsedTime = _elapsedTime;
    }

    public string key;
    public string timeStamp;
    public float elapsedTime;
};

public class SceneManagerScript : MonoBehaviour
{
    //private static float elapsedTime;
    //private static float initialTime;
    private static float initialTime;
    private static float elapsedTime;
    private bool isCountTime;
    //private AudioSource[] sounds;
    //private static List<string> keyLog;
    private static string keyLogPath;
    static public SceneManagerScript sceneManager;
    private static List<KeyLog> keylog;

    void Awake()
    {
        // ref: https://www.hanachiru-blog.com/entry/2018/09/26/010232
        if (sceneManager == null)
        {
            sceneManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        string year = System.DateTime.Now.Year.ToString();
        string month = System.DateTime.Now.Month.ToString();
        string day = System.DateTime.Now.Day.ToString();
        string hour = System.DateTime.Now.Hour.ToString();
        string minutes = System.DateTime.Now.Minute.ToString();
        string second = System.DateTime.Now.Second.ToString();

        string csvFileName = year + "_" + month + "_" + day + "_" + hour + "_" + minutes + "_" + second + ".csv";

        Debug.Log(csvFileName);

        #if UNITY_EDITOR
            keyLogPath = Application.dataPath + "/KeyLog/" + csvFileName;
        #elif UNITY_STANDALONE_OSX
            keyLogPath = Application.dataPath + "/../../" + csvFileName;
        #elif UNITY_STANDALONE_WIN
            keyLogPath = Application.dataPath + "/../../" + csvFileName;
        #endif
        keylog = new List<KeyLog>();
        //isCountTime = false;
        keylog.Add(new KeyLog("START TIME",GetTimeStamp(),0));
        //sounds = GetComponents<AudioSource>();
        initialTime = Time.time;

        // カーソル非表示
        Cursor.visible = false;
        // カーソルの位置を画面中央にロックする
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ref: https://qiita.com/pilkul/items/6351a967372541d92718
        if (Input.anyKeyDown)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {
                    KeyLog k = new KeyLog();
                    // 入力されたKeyをListに追加
                    string codeStr = code.ToString();
                    k.key = codeStr;
                    k.timeStamp = GetTimeStamp();
                    k.elapsedTime = elapsedTime;
                    keylog.Add(k);

                    switch(code)
                    {
                        // Scene 遷移
                        case KeyCode.Q:
                            SceneManager.LoadScene("Question");
                            break;
                        case KeyCode.I:
                            SceneManager.LoadScene("initial");
                            break;
                        case KeyCode.F:
                            SceneManager.LoadScene("Final");
                            break;

                        // アプリケーションの終了
                        case KeyCode.Escape:
                            //  ref: https://web-dev.hatenablog.com/entry/unity/quit-game
                            Debug.Log(elapsedTime);
                            #if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
                            #elif UNITY_STANDALONE
                                UnityEngine.Application.Quit();
                            #endif
                                //  Application.Quit();
                            break;

                        // 時間計測開始
                        //case KeyCode.T:
                            //isCountTime = true;
                            //initialTime = Time.time;
                            //break;

                        /*
                        // 音声再生
                        case KeyCode.Y:
                            sounds[0].Play();
                            break;
                        case KeyCode.H:
                            sounds[1].Play();
                            break;
                        case KeyCode.U:
                            sounds[2].Play();
                            break;
                        case KeyCode.J:
                            sounds[3].Play();
                            break;
                        case KeyCode.Z:
                            sounds[4].Play();
                            break;
                        case KeyCode.X:
                            sounds[5].Play();
                            break;
                        case KeyCode.C:
                            sounds[6].Play();
                            break;
                        case KeyCode.V:
                            sounds[7].Play();
                            break;
                        case KeyCode.B:
                            sounds[8].Play();
                            break;
                        case KeyCode.N:
                            sounds[9].Play();
                            break;
                        case KeyCode.M:
                            sounds[10].Play();
                            break;
                            */

                        // 画面を暗転 + csv出力
                        case KeyCode.Return:
                            isCountTime = false;
                            Debug.Log(elapsedTime);
                            WriteDataToSCV(keylog);
                            break;
                    }
                    break;
                }
            }
        }

        //// 時間計測
        //if (isCountTime)
        //{
        //    elapsedTime = Time.time - initialTime;
        //}
        elapsedTime = Time.time - initialTime;
    }

    string GetTimeStamp() {
        string nowTime;
        string H = System.DateTime.Now.Hour.ToString();
        string M = System.DateTime.Now.Minute.ToString();
        string S = System.DateTime.Now.Second.ToString();

        nowTime = H + ":" + M + ":" + S;
        return nowTime;
    }


    // 経過時間と入力されたkeyのログを出力
    // ref: https://note.mu/macgyverthink/n/na29bc525fc95
    void WriteDataToSCV(List<KeyLog> keylist)
    {
        StreamWriter sw = new StreamWriter(keyLogPath, false);
        sw.WriteLine("Pressed Key Log,Time Stamp,Elapsed Time");
        foreach (var key in keylist)
        {
            Debug.Log(key);
            sw.WriteLine(key.key + "," + key.timeStamp + "," + key.elapsedTime.ToString());
        }
        sw.Flush();
        sw.Close();
    }
}
