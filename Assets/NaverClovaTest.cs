using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NaverClovaTest : MonoBehaviour
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
    string url = "https://t8vrg6872n.apigw.ntruss.com/custom/v1/";

    //Key
    string secretKey1 = "ZmZoS3lhdmllUmFlRmNPcnJWcENZTVdjUFVNcFNwUE0=";

    void Start()
    {
        Debug.Log(requestBody);
        Debug.Log(url);
        Debug.Log(secretKey1);
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

        // timestamp ����
        var timeStamp = DateTime.UtcNow;
        var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
        var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

        // HMAC-SHA256 ��ü ����
        using (HMACSHA256 sha = new HMACSHA256(hmac_key))
        {
            // ���� ����
            // �ѱ��� ���Ե� ��� ���� ������ ��찡 ����� ������ payload�� base64�� ��ȯ �� ��ȣȭ�� �����Ѵ�.
            // Ÿ�ӽ������� ������ ������ ���Ͽ� ����ϴ� ��찡 �Ϲ����̴�.
            // Ÿ�ӽ����� ���� �̿��� ȣ��, ���� �ð��� ���̸� ���� invalid�� �ϰų� accepted�� �ϴ� ������� ��밡���ϴ�.
            // ���ÿ����� (���� + Ÿ�ӽ�����)������, ���۸��� ���� ã�ƺ��� (���� + "^" + Ÿ�ӽ�����) ���� ����� ���Ѵ�.
            var bytes = Encoding.UTF8.GetBytes(payload + hmac_timeStamp);
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