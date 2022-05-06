using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public static class TwitterModel
{
    [DllImport("__Internal")]
    private static extern void OpenNewWindow(string openUrl);

    public static void Tweet(string text)
    {
        var url = "https://twitter.com/intent/tweet?text=" + UnityWebRequest.EscapeURL(text);
#if UNITY_EDITOR
        Application.OpenURL(url);
# elif UNITY_WEBGL
        OpenNewWindow(url);
#else
        Application.OpenURL(url);
#endif
    }
}
