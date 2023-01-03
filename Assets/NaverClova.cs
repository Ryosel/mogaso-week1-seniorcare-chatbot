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

    // ����� ���(Kor)�� �� �ڿ� ����
    string requestBody = File.ReadAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTest.json");

    //�ڵ�����
    string url = "https://07u1a1xxup.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830";

    //��������
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

    // HMAC ���� �Լ�
    private string GenerateHMAC(string key, string requestBody)
    {
        // Ű ����
        byte[] hmac_key = Encoding.UTF8.GetBytes(key);
        Debug.Log("key : " + key);
        Debug.Log("hmac_key : " + hmac_key);

        // timestamp ����
        var timeStamp = DateTime.UtcNow;
        var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
        var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

        // HMAC-SHA256 ��ü ����
        using (HMACSHA256 sha = new HMACSHA256(hmac_key)) //hmac_key�� ����Ͽ� HMACSHA256 Ŭ������ �� �ν��Ͻ��� �ʱ�ȭ
        {
            Debug.Log("sha : " + sha);

            // ���� ����
            // �ѱ��� ���Ե� ��� ���� ������ ��찡 ����� ������ payload�� base64�� ��ȯ �� ��ȣȭ�� �����Ѵ�.
            // Ÿ�ӽ������� ������ ������ ���Ͽ� ����ϴ� ��찡 �Ϲ����̴�.
            // Ÿ�ӽ����� ���� �̿��� ȣ��, ���� �ð��� ���̸� ���� invalid�� �ϰų� accepted�� �ϴ� ������� ��밡���ϴ�.
            // ���ÿ����� (���� + Ÿ�ӽ�����)������, ���۸��� ���� ã�ƺ��� (���� + "^" + Ÿ�ӽ�����) ���� ����� ���Ѵ�.
            //var bytes = Encoding.UTF8.GetBytes(payload + hmac_timeStamp);
            byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
            Debug.Log("request body : " + requestBody);
            Debug.Log("bytes : " + bytes);
            //string base64 = Convert.ToBase64String(bytes);
            //byte[] message = Encoding.UTF8.GetBytes(base64);

            // ��ȣȭ
            byte[] signatureHeader = sha.ComputeHash(bytes);
            Debug.Log("signature header : " + signatureHeader);
            // base64�� convert(byte[] -> string)
            return Convert.ToBase64String(signatureHeader);
        }
    }

    private IEnumerator Post(string url) //�ش� POST ��û�� ���� �� ��ٷȴٰ� response�� �޾ƾ��ϱ� ������ Coroutine�� ���
    {
        //// request ����
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        //CLOVA Chatbot ������ �������� ������ SecretKey(string -> byte -> Base64 string)
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
        //string encodedText = Convert.ToBase64String(bytesToEncode); //Base64�� ���ڵ�

        //HMACSHA256 hmac = new HMACSHA256(secretKey2);//byte ������ secretkey�� Init
        //string signatureHeader = Convert.ToBase64String(hmac);

        //byte[] str2 = Encoding.UTF8.GetBytes(requestBody);
        //byte[] signature = Encoding.UTF8.GetBytes(requestBody);

        string signatureBody = GenerateHMAC(secretKey1, requestBody);
        Debug.Log("signatureBody : " + signatureBody);

        // ê�� ��û�� ���� request header ����
        request.SetRequestHeader("X-NCP-CHATBOT_SIGNATURE", signatureBody); //ê������ ��û�� ������ �޽���, X-NCP-CHATBOT_SIGNATURE �� Sign content(Request body)�� Base64 �� ���ڵ��Ͽ� ����
        request.SetRequestHeader("Content-Type", "application/json;UTF-8");

        // request ���� �� response�� ���� ������ ���
        yield return request.SendWebRequest();

        // response�� ������� ��쿡 ���� error ó��
        if (request == null)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // json ���·� ���� {"text":"�νİ��"}
            string message = request.downloadHandler.text;
            Debug.Log(message);
            JsonData jsonData = JsonUtility.FromJson<JsonData>(message);
            jsonData.printJsonData();

            // Voice Server responded: �νİ��
            Debug.Log("ClovaChatbot Server responded: " + jsonData.events);
        }
    }
}
