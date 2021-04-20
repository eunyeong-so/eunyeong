using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;


public class GetImageByWebsite : MonoBehaviour
{

    public TMP_InputField Keyword;
    public Button ConfirmButton;
    public Transform ImageRoot;

    private List<string> store = new List<string>();
    private const string defaultURL = "https://search.naver.com/search.naver?where=image&sm=tab_jum&query=";
    private string textFile;
    private string url = "";

    void Start()
    {
        ConfirmButton.onClick.AddListener(() => OnClick());
    }


    // UI 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            url = defaultURL + Keyword.text;
            textFile = Keyword.text + "SearchResult.txt";
            StartCoroutine(GetData(url));
        }
    }


    public void OnClick()
    {
        Debug.Log("## URL: " + defaultURL + Keyword);
        url = defaultURL + Keyword.text;
        textFile = Keyword.text + "SearchResult.txt";
        StartCoroutine(GetData(url));
    }



    // 파일 생성

    IEnumerator GetData(string url)
    {
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        var data = request.downloadHandler.text;
        string htmlText = UnityWebRequest.UnEscapeURL(data);

        MakeFile(htmlText);
    }


    async void MakeFile(string htmlText)
    {
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.streamingAssetsPath, textFile)))
        {
            await outputFile.WriteAsync(htmlText);
            Debug.Log("## 파일생성 ");
        }

        MakeImageURLFile();
        SetImage();
    }




    //이미지 URL만 모인 파일 생성
    void MakeImageURLFile()
    {
        List<string> jpgURL = new List<string>();
        List<string> pngURL = new List<string>();

        string[] lines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, textFile));


        //png jpg 저장
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("jpg"))
                jpgURL.Add(lines[i]);
            else if (lines[i].Contains("png"))
                pngURL.Add(lines[i]);
        }

        //parse 
        for (int i = 0; i < jpgURL.Count - 1; i++)
        {
            store.Add(ParseData(jpgURL[i], ".jpg"));
        }

        for (int i = 0; i < pngURL.Count - 1; i++)
        {
            store.Add(ParseData(pngURL[i], ".png"));
        }

        File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, textFile), store);
        Debug.Log("## 이미지 주소저장 ");
    }


    string ParseData(string url, string endTag)
    {
        string startTag = "\"originalUrl\":\"";

        int startIndex = url.IndexOf(startTag);
        int lastIndex = url.IndexOf(endTag, startIndex);

        if (startIndex > 0 && lastIndex > 0)
        {
            int length = (lastIndex - startIndex) + endTag.Length - startTag.Length;

            //Debug.Log("== startIndex : " + startIndex);
            //Debug.Log("== lastIndex : " + lastIndex);
            //Debug.Log("== length : " + length);

            string result = url.Substring(startIndex + startTag.Length, length);
            Debug.Log(result);
            return result;
        }
        return null;
    }



    //이미지 생성
    public void SetImage()
    {
        foreach (string line in store)
        {
            StartCoroutine(MakeImage(line));
        }

    }

    IEnumerator MakeImage(string data)
    {
        yield return new WaitForSeconds(Random.Range(0, 10));
        Vector3 spawnPosition = new Vector3(Random.Range(-10, 10), -3, Random.Range(-10, 10));
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(data);
        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            Debug.Log("연결 오류 " + www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.GetComponent<Renderer>().material.mainTexture = texture;
            quad.transform.SetParent(ImageRoot);
            quad.transform.position = spawnPosition;

            quad.AddComponent<ImageAnimation>();
        }
    }

}

public class ImageAnimation : MonoBehaviour
{ 
    private void Update()
    {
        SlowUp();
    }

    void SlowUp()
    {
        transform.position += Vector3.up * Time.deltaTime * 5.0f;
    }

    void PingPongAnimation()
    {
        AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
     
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;
        transform.position = new Vector3(transform.position.x, curve.Evaluate(Time.time), transform.position.z);
    }
     
}