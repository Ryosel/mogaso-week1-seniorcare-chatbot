using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NaverClova : MonoBehaviour
{
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

    // 사용할 언어(Kor)를 맨 뒤에 붙임
    string requestBody = File.ReadAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTest.json");

    //자동연결
    string url = "https://07u1a1xxup.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830";

    //수동연결
    //string url = "https://07u1a1xxup.apigw.ntruss.com/SeniorCareApp/verson1/";
    //string url = "https://07u1a1xxup.apigw.ntruss.com/custom/v1";  

    //Key
    string secretKey1 = "c2lHSkx5TWhkbnhoT0JpV0hJcmdYcW5tcG5YT3NIblc=";

    void Start()
    {
        Debug.Log(requestBody);
    }

    void Update()
    {
        StartCoroutine(Post(url));
    }

    // HMAC 생성 함수
    private string GenerateHMAC(string key, string requestBody)
    {
        // 키 생성
        byte[] hmac_key = Encoding.UTF8.GetBytes(key);
        Debug.Log("key : " + key);
        Debug.Log("hmac_key : " + hmac_key);

        // timestamp 생성
        var timeStamp = DateTime.UtcNow;
        var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
        var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

        // HMAC-SHA256 객체 생성
        using (HMACSHA256 sha = new HMACSHA256(hmac_key)) //hmac_key를 사용하여 HMACSHA256 클래스의 새 인스턴스를 초기화
        {
            Debug.Log("sha : " + sha);

            // 본문 생성
            // 한글이 포함될 경우 글이 깨지는 경우가 생기기 때문에 payload를 base64로 변환 후 암호화를 진행한다.
            // 타임스탬프와 본문의 내용을 합하여 사용하는 경우가 일반적이다.
            // 타임스탬프 값을 이용해 호출, 응답 시간의 차이를 구해 invalid를 하거나 accepted를 하는 방식으로 사용가능하다.
            // 예시에서는 (본문 + 타임스탬프)이지만, 구글링을 통해 찾아보면 (본문 + "^" + 타임스탬프) 등의 방법을 취한다.
            //var bytes = Encoding.UTF8.GetBytes(payload + hmac_timeStamp);
            byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
            Debug.Log("request body : " + requestBody);
            Debug.Log("bytes : " + bytes);
            //string base64 = Convert.ToBase64String(bytes);
            //byte[] message = Encoding.UTF8.GetBytes(base64);

            // 암호화
            byte[] signatureHeader = sha.ComputeHash(bytes);
            Debug.Log("signature header : " + signatureHeader);
            // base64로 convert(byte[] -> string)
            return Convert.ToBase64String(signatureHeader);
        }
    }

    private IEnumerator Post(string url) //해당 POST 요청을 보낸 후 기다렸다가 response를 받아야하기 때문에 Coroutine을 사용
    {
        //// request 생성
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        //CLOVA Chatbot 도메인 빌더에서 생성한 SecretKey(string -> byte -> Base64 string)
        //string secretKey1 = "cGxhdGpoRFphdEZQQUlCdnBzeWxRcUFEcXpJcXRxaFU=";
        //byte[] secretKey2 = Encoding.UTF8.GetBytes(secretKey1);

        //print(secretKey1);
        //print(secretKey2);
        //Console.WriteLine(ToReadableByteArray(secretKey2));
        //byte[] secretKeyBytes = secretKey.getBytes();
        //byte[] secretkey = new Byte[64];

        //HmacSHA256 Algorithm
        //string base64 = Convert.ToBase64String(abc);
        //HMACSHA256 hmac = new HMACSHA256(secretkey);


        //byte[] bytesToEncode = Encoding.UTF8.GetBytes(requestBody);
        //string encodedText = Convert.ToBase64String(bytesToEncode); //Base64로 인코딩

        //HMACSHA256 hmac = new HMACSHA256(secretKey2);//byte 형식의 secretkey로 Init
        //string signatureHeader = Convert.ToBase64String(hmac);

        //byte[] str2 = Encoding.UTF8.GetBytes(requestBody);
        //byte[] signature = Encoding.UTF8.GetBytes(requestBody);

        string signatureBody = GenerateHMAC(secretKey1, requestBody);
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
            JsonData jsonData = JsonUtility.FromJson<JsonData>(message);
            jsonData.printJsonData();

            // Voice Server responded: 인식결과
            Debug.Log("ClovaChatbot Server responded: " + jsonData.events);
        }
    }
}
