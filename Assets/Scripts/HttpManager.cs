using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;       // HTTP 통신을 위한 네임 스페이스
using System.Text;      // json, csv같은 문서 형태의 인코딩(UTF-8)을 위한 네임 스페이스
using System;    
using UnityEngine.UI;
using System.IO;
using UnityEditor;

public class HttpManager : MonoBehaviour
{
    public string url;
    public Text text_response;
    public RawImage img_response;
    public Button btn_get;
    public Button btn_getImage;
    public Button btn_getjson;
    public Button btn_postJson;
    public Button btn_postImage;
    public List<InputField> userInputs = new List<InputField>();
    public Toggle freeUser;

    public void Get()
    {
        btn_get.interactable = false;
        StartCoroutine(GetRequest(url));
    }

    // Get 통신 코루틴 함수
    IEnumerator GetRequest(string url)
    {
        // HTTP Get 통신 준비를 한다.
        UnityWebRequest request = UnityWebRequest.Get(url);

        // 서버에 Get 요청을 하고, 응답이 올 때까지 대기한다.
        yield return request.SendWebRequest();

        // 만일, 서버로부터 온 응답이 성공(200)이라면...
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답받은 데이터를 출력한다.
            string response = request.downloadHandler.text;
            print(response);
            text_response.text = response;

        }
        // 그렇지 않다면...(400, 404 etc)
        else
        {
            // 에러 내용을 출력한다.
            print(request.error);
            text_response.text = request.error;
        }
        btn_get.interactable = true;
    }

    public void GetImage()
    {
        btn_getImage.interactable = false;
        StartCoroutine(GetImageRequest(url));
    }

    // 이미지 파일을 Get으로 받는 함수
    IEnumerator GetImageRequest(string url)
    {
        // get(texture) 통신을 준비한다.
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // 서버에 요청을 하고, 응답이 있을 때까지 기다린다.
        yield return request.SendWebRequest();
        // 만일, 응답이 성공이라면...
        if(request.result == UnityWebRequest.Result.Success)
        {
            // 받은 텍스쳐 데이터를 Texture2D 변수에 받아놓는다.
            Texture2D response = DownloadHandlerTexture.GetContent(request);
            
            //Texture2D 데이터를 img_response 의 texture 값으로 넣어둔다.
            img_response.texture = response;

            // text_response에도 성공 코드를 번호를 출력한다.
            text_response.text = "성공 - " + request.responseCode.ToString();

        }
        else
        {
            // 그렇지 않다면...

            // 에러 내용을 text_response 출력한다.
            print(request.error);
            text_response.text = request.error;

        }
        btn_getImage.interactable = true;

    }

    public void GetJson()
    {
        btn_getjson.interactable= false;
        StartCoroutine(GetJsonImageRequest(url));
    }

    IEnumerator GetJsonImageRequest(string url)
    {
        // url로부터 Get으로 요청을 준비한다.
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        // 준비된 요청을 서버에 전달하고 응답이 올때까지 기다린다.
        yield return request.SendWebRequest();

        // 만일, 응답이 성공이라면...
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 텍스트를 받는다.
            string result = request.downloadHandler.text;
            
            // 응답 받은 json 데이터를 RequestImage 구조체 형태로 파싱한다.
            RequestImage reqImageData = JsonUtility.FromJson<RequestImage>(result);

            //byte[] binaries =  Encoding.UTF8.GetBytes(reqImageData.img);
            byte[] binaries = Convert.FromBase64String(reqImageData.img);

            if (binaries.Length > 0)
            {
                Texture2D texture = new Texture2D(184, 274);

                // byte 배열로 된 raw 데이터를 텍스쳐 형태로 변환해서 texture2D 인스턴스로 변환한다.
                texture.LoadImage(binaries);
                //texture.EncodeToJPG();

                img_response.texture = texture;
            }

        }
        // 그렇지 않다면...
        else
        {
            // 에러 내용을 text_response에 전달한다.
            text_response.text= request.responseCode + " : " + request.error;
            //Debug.LogError(request.responseCode + " : " + request.error);
        }
        btn_getjson.interactable = true;

    }

    // 서버에 Json 데이터를 Post하는 함수
    public void PostJson()
    {
        btn_postJson.interactable= false;
        StartCoroutine(PostJsonRequest(url));
    }

    IEnumerator PostJsonRequest(string url)
    {
        // 사용자의 입력 정보를 Json 데이터로 변환하기
        JoinUserData userData = new JoinUserData();
        userData.id = Convert.ToInt32(userInputs[0].text);
        userData.password = userInputs[1].text;
        userData.nickname = userInputs[2].text;
        userData.freeAccount = freeUser.isOn;
        string userJsonData = JsonUtility.ToJson(userData,true);
        byte[] jsonBins = Encoding.UTF8.GetBytes(userJsonData);
        
        // POST를 하기 위한 준비를 한다.
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(jsonBins);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 서버에 Post를 전송하고 응답이 올때까지 기다린다.
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 다운로드 핸들러에서 텍스트 값을 받아서 UI에 출력한다.
            string response =  request.downloadHandler.text;
            text_response.text = response;
            Debug.LogWarning(response);
        }
        else
        {
            text_response.text = request.error;
            Debug.LogError(request.error);
        }

        btn_postJson.interactable = true;

    }


    public void PostImage()
    {
        btn_postImage.interactable = false;
        StartCoroutine(postImageRequest("http://192.168.0.66:5000/post/imageShow"));
    }

    IEnumerator postImageRequest(string url)
    {
        //string path = "C:/Fire/TPS/Assets/Materials/Icon.png";
        string path = EditorUtility.OpenFilePanel("이미지 파일 찾기", "C:/", "png, jpg, bmp");

        // 바이트 배열로 데이터를 읽어올 때
        byte[] imageBinaries = File.ReadAllBytes(path);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        //request.SetRequestHeader("Content-Type", "multipart/form-data"); // 영상
        request.SetRequestHeader("Content-Type", "image/png"); // 이미지
        request.uploadHandler = new UploadHandlerRaw(imageBinaries);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            text_response.text = response;
            print(response);
        }
        else
        {
            text_response.text = $"{request.responseCode} - {request.error}";
            Debug.LogError($"{request.responseCode} - {request.error}");

        }
        btn_postImage.interactable = true;
    }
}

[System.Serializable]
public struct RequestImage
{
    public string img;

}

[System.Serializable]
public struct JoinUserData
{
    public int id;
    public string password;
    public string nickname;
    public bool freeAccount;
}
