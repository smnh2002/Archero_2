using UnityEngine;

public class PlayModeResetter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteKey("MaxUnlockedLevel");
        Debug.Log("Reset MaxUnlockedLevel for testing in Editor.");
#endif
    }
}
