using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class SetupLevelsMenu
{
    [MenuItem("Tools/1 - Fix Levels Button!")]
    public static void DoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogError("Lütfen Play Modundan çýk ve tekrar dene!");
            return; 
        }

        Scene activeScene = EditorSceneManager.GetActiveScene();
        string scenePath = "Assets/Scenes/MenuScene.unity";
        bool wasActive = activeScene.path.Replace("\\\\", "/") == scenePath;
        
        Scene scene;
        if (wasActive) {
            scene = activeScene;
        } else {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
        
        GameObject mainMenuPanel = GameObject.Find("MainMenuPanel");
        if (mainMenuPanel == null) {
            Debug.LogError("MainMenuPanel bulunamadi!");
            return;
        }
        
        // CLEANUP USER'S MANUAL OBJECTS
        Transform[] allTransforms = mainMenuPanel.transform.parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms) {
            if (t != null && (t.name == "LevelsButton" || t.name == "LevelsPanel")) {
                Object.DestroyImmediate(t.gameObject);
            }
        }
        
        Transform buttonGroup = FindDeepChild(mainMenuPanel.transform, "ButtonGroup");
        if (buttonGroup == null) {
            buttonGroup = mainMenuPanel.transform; 
        }

        Transform playBtn = FindDeepChild(buttonGroup, "Play");
        Transform upgradeBtn = FindDeepChild(buttonGroup, "UpgradesButton");
        Transform quitBtn = FindDeepChild(buttonGroup, "QuitButton");
        
        if (playBtn == null) {
            Debug.LogError("Play butonu bulunamadi! Hiyerarsiyi kontrol et.");
            if (!wasActive) EditorSceneManager.CloseScene(scene, true);
            return;
        }

        Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Yellow/Normal.png");
        Sprite yellowHover = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Yellow/Hover.png");
        Sprite yellowPressed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Yellow/Pressed.png");
        Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Red/Normal.png");
        Sprite closeHover = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Red/Hover.png");
        Sprite closePressed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RPG Button 7/PNG/Red/Pressed.png");

        GameObject levelsBtnObj = Object.Instantiate(playBtn.gameObject, buttonGroup);
        levelsBtnObj.name = "LevelsButton";
        
        if (upgradeBtn != null) {
            levelsBtnObj.transform.SetSiblingIndex(upgradeBtn.GetSiblingIndex());
        }

        Image levelsImg = levelsBtnObj.GetComponent<Image>();
        if (levelsImg != null && yellowBtnSprite != null) {
            levelsImg.sprite = yellowBtnSprite;
            levelsImg.color = Color.white;
        }

        Button levelsBtn = levelsBtnObj.GetComponent<Button>();
        if (levelsBtn != null) {
            levelsBtn.transition = Selectable.Transition.SpriteSwap;
            SpriteState st = levelsBtn.spriteState;
            st.highlightedSprite = yellowHover;
            st.pressedSprite = yellowPressed;
            st.selectedSprite = yellowHover;
            levelsBtn.spriteState = st;
            levelsBtn.onClick = new Button.ButtonClickedEvent();
        }

        TextMeshProUGUI levelsText = levelsBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (levelsText != null) {
            levelsText.text = "Levels";
            levelsText.color = Color.white;
        }

        GameObject levelsPanelObj = new GameObject("LevelsPanel");
        levelsPanelObj.transform.SetParent(mainMenuPanel.transform.parent, false);
        levelsPanelObj.transform.SetAsLastSibling();
        
        RectTransform panelRt = levelsPanelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;
        
        Image panelBg = levelsPanelObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.9f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(levelsPanelObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -150);
        titleRt.sizeDelta = new Vector2(600, 100);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "SELECT LEVEL";
        titleTxt.fontSize = 72;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = Color.white;

        GameObject containerObj = new GameObject("Container");
        containerObj.transform.SetParent(levelsPanelObj.transform, false);
        RectTransform containerRt = containerObj.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0, -50);
        containerRt.sizeDelta = new Vector2(400, 400);
        
        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 30;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        Button[] lvlBtns = new Button[3];
        for (int i=1; i<=3; i++) {
            GameObject lvlBtnObj = Object.Instantiate(playBtn.gameObject, containerObj.transform);
            lvlBtnObj.name = "Level " + i + " Button";
            
            LayoutElement le = lvlBtnObj.AddComponent<LayoutElement>();
            le.minHeight = 100;
            le.minWidth = 350;

            Image img = lvlBtnObj.GetComponent<Image>();
            if (img != null) {
                img.sprite = yellowBtnSprite;
                img.color = Color.white;
            }

            Button btn = lvlBtnObj.GetComponent<Button>();
            if (btn != null) {
                btn.transition = Selectable.Transition.SpriteSwap;
                SpriteState st = btn.spriteState;
                st.highlightedSprite = yellowHover;
                st.pressedSprite = yellowPressed;
                st.selectedSprite = yellowHover;
                btn.spriteState = st;
                btn.onClick = new Button.ButtonClickedEvent();
            }

            TextMeshProUGUI txt = lvlBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) {
                txt.text = "Level " + i;
                txt.color = Color.white;
            }
            
            lvlBtns[i-1] = btn;
        }

        GameObject closeBtnObj = Object.Instantiate(playBtn.gameObject, levelsPanelObj.transform);
        closeBtnObj.name = "CloseButton";
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0, 150);
        
        Image closeImg = closeBtnObj.GetComponent<Image>();
        if (closeImg != null && closeSprite != null) {
            closeImg.sprite = closeSprite;
            closeImg.color = Color.white;
        }

        Button closeBtn = closeBtnObj.GetComponent<Button>();
        if (closeBtn != null) {
            closeBtn.transition = Selectable.Transition.SpriteSwap;
            SpriteState st = closeBtn.spriteState;
            st.highlightedSprite = closeHover;
            st.pressedSprite = closePressed;
            st.selectedSprite = closeHover;
            closeBtn.spriteState = st;
            closeBtn.onClick = new Button.ButtonClickedEvent();
        }

        TextMeshProUGUI closeTxt = closeBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (closeTxt != null) {
            closeTxt.text = "Back";
            closeTxt.color = Color.white;
        }

        LevelSelectionUI lsu = mainMenuPanel.transform.parent.gameObject.GetComponent<LevelSelectionUI>();
        if (lsu == null) lsu = mainMenuPanel.transform.parent.gameObject.AddComponent<LevelSelectionUI>();
        
        SerializedObject so = new SerializedObject(lsu);
        so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
        so.FindProperty("levelsPanel").objectReferenceValue = levelsPanelObj;
        so.FindProperty("btnLevel1").objectReferenceValue = lvlBtns[0];
        so.FindProperty("btnLevel2").objectReferenceValue = lvlBtns[1];
        so.FindProperty("btnLevel3").objectReferenceValue = lvlBtns[2];
        so.FindProperty("btnLevel4").objectReferenceValue = lvlBtns[3];
        so.ApplyModifiedProperties();

        if (levelsBtn != null && lsu != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(levelsBtn.onClick, new UnityAction(lsu.OpenLevelsPanel));
        
        if (closeBtn != null && lsu != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(closeBtn.onClick, new UnityAction(lsu.CloseLevelsPanel));

        levelsPanelObj.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        
        if (!wasActive) EditorSceneManager.CloseScene(scene, true);
        
        Debug.Log("LEVELS KURULUMU BASARIYLA TAMAMLANDI!");
    }

    static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
