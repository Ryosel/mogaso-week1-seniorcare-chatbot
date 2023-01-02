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

    // 사용할 언어(Kor)를 맨 뒤에 붙임
    string url = "https://07u1a1xxup.apigw.ntruss.com/custom/v1/8712/35e51b8deb90593be336d670812252f3f179ed1eca5dcb8504f3d460f7212830";

    private IEnumerator Post(string url, byte[] data) //해당 POST 요청을 보낸 후 기다렸다가 response를 받아야하기 때문에 Coroutine을 사용
    {
        // request 생성
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // request header 설정
        request.SetRequestHeader("X-NCP-CHATBOT_SIGNATURE", "signature of request body"); //챗봇에게 요청을 보내는 메시지
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
            ClovaChatbot clovaChatbot = JsonUtility.FromJson<ClovaChatbot>(message);

            // Voice Server responded: 인식결과
            Debug.Log("ClovaChatbot Server responded: " + clovaChatbot.text);
        }
    }
}
