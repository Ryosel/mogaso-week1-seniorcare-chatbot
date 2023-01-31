using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static NaverClova;

public class NaverClova : MonoBehaviour
{
    [System.Serializable]
    public class JsonData
    {
        //public string version;
        public string userId;
        //public string userIp;
        public long timestamp;
        public string[] bubbles;
        public string events;

        public void printJsonData()
        {
            //Debug.Log("version : " + version);
            Debug.Log("userId : " + userId);
            //Debug.Log("userIp : " + userIp);
            Debug.Log("timestamp : " + timestamp);
            Debug.Log("bubbles : " + bubbles);
            Debug.Log("event : " + events);
        }
    }

    string requestBody = File.ReadAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTest3.json");

    //API Gateway 자동/수동연결(Domain 8712)
    //string url = "https://o1s61nqdxh.apigw.ntruss.com/custom/v1/"; //error(result) code : 300(URL Not Found)
    string url = "https://o1s61nqdxh.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830"; //error(result) code : 99

    //Key
    string secretKey1 = "ZmZoS3lhdmllUmFlRmNPcnJWcENZTVdjUFVNcFNwUE0=";
    string requestBody2 = "";

    // timestamp 생성
    static DateTime timeStamp = DateTime.UtcNow;
    static TimeSpan timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
    long hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

    void Start()
    {
        JsonData jsonData1 = new JsonData();
        jsonData1 = JsonUtility.FromJson<JsonData>(requestBody);
        jsonData1.timestamp = hmac_timeStamp;
        jsonData1.printJsonData();
        File.WriteAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json", JsonUtility.ToJson(jsonData1));
        requestBody2 = File.ReadAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json");

        Debug.Log("requestBody : " + requestBody2);
        Debug.Log("url : " + url);
        Debug.Log("secretKey : " +  secretKey1);
    }

    void Update()
    {
        StartCoroutine(Post(url));
    }

    // HMAC 생성 함수
    private string GenerateHMAC(string key, string payload)
    {
        // 키 생성
        var hmac_key = Encoding.UTF8.GetBytes(key);

        //// timestamp 생성
        //var timeStamp = DateTime.UtcNow;
        //var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
        //var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

        //JsonData jsonData2 = new JsonData();
        //jsonData2 = JsonUtility.FromJson<JsonData>(requestBody);
        //jsonData2.printJsonData();
        //jsonData2.timestamp = timeStamp;
        ////File.WriteAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json", JsonUtility.ToJson(jsonData2));

        // HMAC-SHA256 객체 생성
        using (HMACSHA256 sha = new HMACSHA256(hmac_key))
        {
            // 본문 생성
            // 한글이 포함될 경우 글이 깨지는 경우가 생기기 때문에 payload를 base64로 변환 후 암호화를 진행한다.
            // 타임스탬프와 본문의 내용을 합하여 사용하는 경우가 일반적이다.
            // 타임스탬프 값을 이용해 호출, 응답 시간의 차이를 구해 invalid를 하거나 accepted를 하는 방식으로 사용가능하다.
            // 예시에서는 (본문 + 타임스탬프)이지만, 구글링을 통해 찾아보면 (본문 + "^" + 타임스탬프) 등의 방법을 취한다.
            var bytes = Encoding.UTF8.GetBytes(payload + "\n" + hmac_timeStamp);
            string base64 = Convert.ToBase64String(bytes);
            var message = Encoding.UTF8.GetBytes(base64);

            // 암호화
            var hash = sha.ComputeHash(message);

            // base64 컨버팅
            return Convert.ToBase64String(hash);
        }
    }

    private IEnumerator Post(string url) //해당 POST 요청을 보낸 후 기다렸다가 response를 받아야하기 때문에 Coroutine을 사용
    {
        //// request 생성
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        string signatureBody = GenerateHMAC(secretKey1, requestBody2);
        Debug.Log("signatureBody : " + signatureBody);

        // 챗봇 요청을 위한 request header 설정
        request.SetRequestHeader("X-NCP-CHATBOT_SIGNATURE", signatureBody); //챗봇에게 요청을 보내는 메시지, X-NCP-CHATBOT_SIGNATURE 는 Sign content(Request body)을 Base64 로 인코딩하여 설정
        request.SetRequestHeader("Content-Type", "application/json;UTF-8");

        // request 보낸 후 response를 받을 때까지 대기
        yield return request.SendWebRequest();

        // response가 비어있을 경우에 대한 error 처리
        if (request == null)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // json 형태로 받음 {"text":"인식결과"}
            string message = request.downloadHandler.text;
            Debug.Log(message);
            JsonData jsonData3 = JsonUtility.FromJson<JsonData>(message);
            jsonData3.printJsonData();
            File.WriteAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json", JsonUtility.ToJson(jsonData3));

            // Voice Server responded: 인식결과
            Debug.Log("ClovaChatbot Server responded: " + jsonData3.events);
        }
    }
}
