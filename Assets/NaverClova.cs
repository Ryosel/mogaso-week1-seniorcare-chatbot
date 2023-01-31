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

    //API Gateway �ڵ�/��������(Domain 8712)
    //string url = "https://o1s61nqdxh.apigw.ntruss.com/custom/v1/"; //error(result) code : 300(URL Not Found)
    string url = "https://o1s61nqdxh.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830"; //error(result) code : 99

    //Key
    string secretKey1 = "ZmZoS3lhdmllUmFlRmNPcnJWcENZTVdjUFVNcFNwUE0=";
    string requestBody2 = "";

    // timestamp ����
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

    // HMAC ���� �Լ�
    private string GenerateHMAC(string key, string payload)
    {
        // Ű ����
        var hmac_key = Encoding.UTF8.GetBytes(key);

        //// timestamp ����
        //var timeStamp = DateTime.UtcNow;
        //var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
        //var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

        //JsonData jsonData2 = new JsonData();
        //jsonData2 = JsonUtility.FromJson<JsonData>(requestBody);
        //jsonData2.printJsonData();
        //jsonData2.timestamp = timeStamp;
        ////File.WriteAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json", JsonUtility.ToJson(jsonData2));

        // HMAC-SHA256 ��ü ����
        using (HMACSHA256 sha = new HMACSHA256(hmac_key))
        {
            // ���� ����
            // �ѱ��� ���Ե� ��� ���� ������ ��찡 ����� ������ payload�� base64�� ��ȯ �� ��ȣȭ�� �����Ѵ�.
            // Ÿ�ӽ������� ������ ������ ���Ͽ� ����ϴ� ��찡 �Ϲ����̴�.
            // Ÿ�ӽ����� ���� �̿��� ȣ��, ���� �ð��� ���̸� ���� invalid�� �ϰų� accepted�� �ϴ� ������� ��밡���ϴ�.
            // ���ÿ����� (���� + Ÿ�ӽ�����)������, ���۸��� ���� ã�ƺ��� (���� + "^" + Ÿ�ӽ�����) ���� ����� ���Ѵ�.
            var bytes = Encoding.UTF8.GetBytes(payload + "\n" + hmac_timeStamp);
            string base64 = Convert.ToBase64String(bytes);
            var message = Encoding.UTF8.GetBytes(base64);

            // ��ȣȭ
            var hash = sha.ComputeHash(message);

            // base64 ������
            return Convert.ToBase64String(hash);
        }
    }

    private IEnumerator Post(string url) //�ش� POST ��û�� ���� �� ��ٷȴٰ� response�� �޾ƾ��ϱ� ������ Coroutine�� ���
    {
        //// request ����
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        string signatureBody = GenerateHMAC(secretKey1, requestBody2);
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
            JsonData jsonData3 = JsonUtility.FromJson<JsonData>(message);
            jsonData3.printJsonData();
            File.WriteAllText("C:/Users/vit00/Documents/GitHub/SeniorCareAIChatbot/Assets/ChatbotTestVer.json", JsonUtility.ToJson(jsonData3));

            // Voice Server responded: �νİ��
            Debug.Log("ClovaChatbot Server responded: " + jsonData3.events);
        }
    }
}
