using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[System.Serializable]
public class JsonData
{
    public string version;
    public string userId;
    public string userIp;
    public long timestamp;
    public string[] bubbles;
    public string events;

    public void printJsonData()
    {
        Debug.Log("version : " + version);
        Debug.Log("userId : " + userId);
        Debug.Log("userIp : " + userIp);
        Debug.Log("timestamp : " + timestamp);
        Debug.Log("bubbles : " + bubbles);
        Debug.Log("event : " + events);
    }
}

public class JsonLoad : MonoBehaviour
{
    // Start is called before the first frame update 
    void Start()
    {
        //File.WriteAllText(Application.dataPath + "/ChatbotTest.json", JsonUtility.ToJson(data));

        // file load 
        //string str = File.ReadAllText(Application.dataPath + "/ChatbotTest.json");
        string str = File.ReadAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTest.json");
        Debug.Log(str);

        //JsonUtility.FromJson()함수를 사용하여 Json 파일의 문자열로부터 객체를 생성
        JsonData jsonData = new JsonData();
        jsonData = JsonUtility.FromJson<JsonData>(str);
        jsonData.printJsonData();
        //Debug.Log(jsonData.data);

    }

    // Update is called once per frame 
    void Update()
    {

    }
}
