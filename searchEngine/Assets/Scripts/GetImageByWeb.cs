using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GetImageByWeb : MonoBehaviour
{

    public RawImage resultImage; 
    public TMP_InputField keyword;
    private string naverURL = "https://search.naver.com/search.naver?where=image&sm=tab_jum&query=";
     
    void Start()
    {
   
        StartCoroutine(GetSearchResults("https://blog.hmgjournal.com/images_n/contents/171024_season01.png"));
    }

   
    public void OnClick()
    {
        Debug.Log(naverURL + keyword.text);
        string url = naverURL + keyword.text;
        StartCoroutine(GetSearchResults(url));
    }


    
    IEnumerator GetSearchResults(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            Debug.Log("연결 오류 " + www.error);
        }
        else
        {
            resultImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }

}
