using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NaverClova : MonoBehaviour
{
    [SerializeField]
    public class ClovaChatbot
    {
        public string text;
    }

    // ����� ���(Kor)�� �� �ڿ� ����
    string url = "https://07u1a1xxup.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830";

    private IEnumerator Post(string url, byte[] data) //�ش� POST ��û�� ���� �� ��ٷȴٰ� response�� �޾ƾ��ϱ� ������ Coroutine�� ���
    {
        // request ����
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // request header ����
        request.SetRequestHeader("X-NCP-CHATBOT_SIGNATURE", "signature of request body"); //ê������ ��û�� ������ �޽���
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
            ClovaChatbot clovaChatbot = JsonUtility.FromJson<ClovaChatbot>(message);

            // Voice Server responded: �νİ��
            Debug.Log("ClovaChatbot Server responded: " + clovaChatbot.text);
        }
    }
}
