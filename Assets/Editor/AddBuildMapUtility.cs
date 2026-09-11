using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

public class AddBuildMapUtility : EditorWindow
{
    public enum SuffixEnum
    {
        Prefab = 0,
        Image,
        Txt,
        Font,
        Audio,
        Material,
        Scene,
        All
    }

    protected List<string> projectList = new List<string>();

    protected Dictionary<string, List<string>> bundleNameListMap = new Dictionary<string, List<string>>();
    protected Dictionary<string, List<SuffixEnum>> suffixListMap = new Dictionary<string, List<SuffixEnum>>();
    protected Dictionary<string, List<string>> pathListMap = new Dictionary<string, List<string>>();
    protected Dictionary<string, int> countMap = new Dictionary<string, int>();

    protected int selectedIndex = 0;

    private Texture2D frame, selected, unselect;
    private bool toRename = false;
    private string newProjectName = "";
    private Rect renameWindowRect = new Rect(100, 100, 200, 60);
    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 projectScrollPosition = Vector2.zero;
    private GUIContent notice = new GUIContent("请往项目内添加AssetBundle");

    private static GUIStyle selectedStyle = new GUIStyle();
    private static GUIStyle unselectStyle = new GUIStyle();
    private static GUIContent windowTitle = new GUIContent("素材打包工具");

    [MenuItem("LuaFramework/素材打包工具")]
    static void SetAssetBundleNameExtension()
    {
        AddBuildMapUtility window = EditorWindow.GetWindow<AddBuildMapUtility>();
        window.titleContent = windowTitle;

        SetStyle(selectedStyle, AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LuaFramework/Editor/Imgs/Selected.png"));
        SetStyle(unselectStyle, AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LuaFramework/Editor/Imgs/Unselect.png"));
    }

    static GUIStyle SetStyle(GUIStyle style, Texture2D background, Texture2D pressBackground = null)
    {
        style.border = new RectOffset(2, 2, 2, 2);
        style.richText = true;
        style.alignment = TextAnchor.MiddleCenter;
        pressBackground = pressBackground ?? background;
        style.normal.background = style.onNormal.background = style.focused.background = style.onFocused.background = style.hover.background = style.onHover.background = background;
        style.active.background = style.onActive.background = pressBackground;
        return style;
    }

    protected void OnGUI()
    {
        if (toRename)
        {
            BeginWindows();
            renameWindowRect = GUILayout.Window(10, renameWindowRect, RenameWindow, "重命名");
            EndWindows();
            return;
        }
        #region 工具总栏
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加新项目", GUILayout.MinHeight(30)))
        {
            AddProject("New Project");
            selectedIndex = projectList.Count - 1;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, 9999);
        }
        if (GUILayout.Button("读取文件(.csv)", GUILayout.MinHeight(30)))
        {
            string path = EditorUtility.OpenFilePanel("", Application.dataPath, "csv");
            if (!string.IsNullOrEmpty(path))
            {
                ClearAll();
                Debug.LogError(">>>>path:" + path);
                string content = File.ReadAllText(path);

               

                string[] contents = content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < contents.Length; i++)
                {
                    string oneLine = contents[i];
                    if (oneLine[0] == '<')
                    {
                        oneLine = oneLine.Trim(new[] { '<', '>', ',' });
                        string[] spx = oneLine.Split(',');
                        string pName = spx[0]; //项目名称
                        int abCount = int.Parse(spx[1]); //项目AssetBundle数量
                        pName = AddProject(pName);
                        countMap[pName] = abCount;
                        for (int j = 0; j < abCount; j++)
                        {
                            string[] a = contents[i + j + 1].Split(',');
                            AddItem(pName, a[0], StringToEnum(a[1]), a[2]);
                        }
                    }
                }
                Focus();

                Debug.LogError("读取结束!");
            }
        }
        if (GUILayout.Button("保存到本地", GUILayout.MinHeight(30)))
        {
            string path = EditorUtility.SaveFilePanel("", Application.dataPath, "AssetBundleInfo", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < projectList.Count; i++)
                {
                    string name = projectList[i];
                    sb.Append(string.Format("<{0},{1}>", name, countMap[name]));
                    sb.AppendLine();
                    for (int j = 0; j < countMap[name]; j++)
                    {
                        if (string.IsNullOrEmpty(bundleNameListMap[name][j])) break;
                        sb.Append(bundleNameListMap[name][j] + ",");
                        sb.Append(EnumToString(suffixListMap[name][j]) + ",");
                        sb.Append(pathListMap[name][j]);
                        sb.AppendLine();
                    }
                }
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf("Assets"));
                path = Path.Combine(path, "Backup");
                Directory.CreateDirectory(path);
                path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".csv");
                File.WriteAllText(path, sb.ToString());

                AssetDatabase.Refresh();
            }
            Debug.LogError("保存结束!");
        }
        if (GUILayout.Button("关闭所有项目", GUILayout.MinHeight(30)))
        {
            ClearAll();
        }
        EditorGUILayout.EndHorizontal();
        #endregion
        if (projectList.Count == 0) return;
        if (Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                selectedIndex = selectedIndex > 0 ? selectedIndex - 1 : (projectList.Count - 1);
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                selectedIndex++;
                selectedIndex = selectedIndex % projectList.Count;
                Event.current.Use();
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        //-----项目列表-----//
        projectScrollPosition = EditorGUILayout.BeginScrollView(projectScrollPosition, GUILayout.MinWidth(170));
        for (int i = 0; i < projectList.Count; i++)
        {
            GUILayout.Space(2);
            if (GUILayout.Button(string.Format("<color=white>{0}</color>", projectList[i]), i == selectedIndex ? selectedStyle : unselectStyle, GUILayout.MinWidth(140), GUILayout.MinHeight(22)))
            {
                selectedIndex = i;
            }
        }
        EditorGUILayout.EndScrollView();
        //------------------//
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        //项目选择
        EditorGUILayout.LabelField("当前项目: " + projectList[selectedIndex], GUILayout.MinHeight(30));
        if (GUILayout.Button("重命名", GUILayout.MaxWidth(80)))
        {
            toRename = true;
            newProjectName = projectList[selectedIndex];
        }
        if (GUILayout.Button("添加所有选中的文件", GUILayout.MaxWidth(150)))
        {
            if (Selection.objects.Length > 0)
            {
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    AutoFill(projectList[selectedIndex], Selection.objects[i]);
                }
            }
        }
        if (GUILayout.Button("清空当前项目", GUILayout.MaxWidth(100)))
        {
            string pName = projectList[selectedIndex];
            bundleNameListMap[pName].Clear();
            suffixListMap[pName].Clear();
            pathListMap[pName].Clear();
            countMap[pName] = 0;
        }
        if (GUILayout.Button("删除当前项目", GUILayout.MaxWidth(100)))
        {
            string pName = projectList[selectedIndex];
            bundleNameListMap.Remove(pName);
            suffixListMap.Remove(pName);
            pathListMap.Remove(pName);
            countMap.Remove(pName);
            projectList.RemoveAt(selectedIndex);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, projectList.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        string projectName = projectList[selectedIndex];
        if (countMap[projectName] > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < countMap[projectName]; j++)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(j.ToString() + ". AB包名", GUILayout.MaxWidth(65));
              //  Debug.LogError("j:" + j + ",bundleNameListMap:" + bundleNameListMap[projectName][j]);
                if(j < bundleNameListMap[projectName].Count)
                    bundleNameListMap[projectName][j] = EditorGUILayout.TextField("", bundleNameListMap[projectName][j], GUILayout.MinWidth(150));
                EditorGUILayout.LabelField("   类型", GUILayout.MaxWidth(40));
                // 
                if (j < suffixListMap[projectName].Count)
                {
                   // Debug.LogError(",suffixListMap:" + suffixListMap[projectName][j]);
                    suffixListMap[projectName][j] = (SuffixEnum)EditorGUILayout.EnumPopup(suffixListMap[projectName][j], GUILayout.MaxWidth(80));
                }
                EditorGUILayout.LabelField("   路径", GUILayout.MaxWidth(40));
                if(j< pathListMap[projectName].Count)
                    pathListMap[projectName][j] = EditorGUILayout.TextField(pathListMap[projectName][j], GUILayout.MinWidth(200));
                if (GUILayout.Button("删除该项", GUILayout.MaxWidth(60)))
                {
                    RemoveItem(projectName, j);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            RemoveNotification();
        }
        else
        {
            ShowNotification(notice);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    protected void RenameWindow(int id)
    {
        newProjectName = GUILayout.TextField(newProjectName);
        if (GUILayout.Button("确定"))
        {
            if (!string.IsNullOrEmpty(newProjectName))
            {
                string lastName = projectList[selectedIndex];
                newProjectName = AddProject(newProjectName);
                bundleNameListMap[newProjectName].AddRange(bundleNameListMap[lastName]);
                suffixListMap[newProjectName].AddRange(suffixListMap[lastName]);
                pathListMap[newProjectName].AddRange(pathListMap[lastName]);
                countMap[newProjectName] = countMap[lastName];

                projectList.RemoveAt(selectedIndex);
                selectedIndex = projectList.Count - 1;
                selectedIndex = Mathf.Clamp(selectedIndex, 0, 9999);
            }
            toRename = false;
        }
        if (GUILayout.Button("取消"))
        {
            toRename = false;
        }
    }

    protected void ClearAll()
    {
        bundleNameListMap.Clear();
        suffixListMap.Clear();
        pathListMap.Clear();
        countMap.Clear();
        projectList.Clear();
    }

    protected void AddItem(string projectName, string bundleName, SuffixEnum suffix, string path)
    {
        bundleNameListMap[projectName].Add(bundleName);
        suffixListMap[projectName].Add(suffix);
        pathListMap[projectName].Add(path);
    }

    protected void RemoveItem(string projectName, int index)
    {
        bundleNameListMap[projectName].RemoveAt(index);
        suffixListMap[projectName].RemoveAt(index);
        pathListMap[projectName].RemoveAt(index);
        countMap[projectName]--;
    }

    protected void AutoFill(string projectName, UnityEngine.Object selectedObject)
    {
        string path = AssetDatabase.GetAssetPath(selectedObject);
        string[] files = Directory.GetFiles(path);
        if (files.Length == 0) return;
        string parent = path.Substring(0, path.LastIndexOf('/'));
        if (parent.Contains("/"))
        {
            int lastIndex = parent.LastIndexOf('/') + 1;
            int len = parent.Length - lastIndex;
            parent = parent.Substring(lastIndex, len).ToLower() + '_';
        }
        else parent = "";
        bundleNameListMap[projectName].Add(parent + path.Remove(0, path.LastIndexOf('/') + 1).ToLower() + ".bundle"/*LuaFramework.AppConst.ExtName*/);

        for (int i = 0; i < files.Length; i++)
        {
            string sfx = files[i];
            if (sfx.EndsWith(".meta"))
            {
                continue;
            }
            suffixListMap[projectName].Add(StringToEnum(sfx));
            pathListMap[projectName].Add(path);
            break;
        }
        countMap[projectName]++;
    }

    protected string AddProject(string name)
    {
        projectList = projectList ?? new List<string>();
        while (projectList.Contains(name)) name += " (New)";
        Debug.LogError("AddProject:" + name);
        projectList.Add(name);
        bundleNameListMap.Add(name, new List<string>());
        suffixListMap.Add(name, new List<SuffixEnum>());
        pathListMap.Add(name, new List<string>());
        countMap.Add(name, 0);
        return name;
    }

    public static string EnumToString(SuffixEnum se)
    {
        switch (se)
        {
            case SuffixEnum.Prefab:
                return ".prefab|.fbx|.obj|.dae|.max";
            case SuffixEnum.Image:
                return ".png|.jpg|.jpeg|.bmp|.psd";
            case SuffixEnum.Txt:
                return ".txt|.xml|.json|.shader|.csv";
            case SuffixEnum.Font:
                return ".ttf|.fontsettings";
            case SuffixEnum.Audio:
                return ".mp3|.ogg|.wav";
            case SuffixEnum.Material:
                return "*.mat";
            case SuffixEnum.Scene:
                return "*.unity";
            case SuffixEnum.All:
                return "*all*";
            default:
                return "null";
        }
    }

    public static SuffixEnum StringToEnum(string s)
    {
        s = s.ToLower();
        if (s.EndsWith(".prefab") || s.EndsWith(".fbx") || s.EndsWith(".obj") || s.EndsWith(".dae") || s.EndsWith(".max"))
        {
            return SuffixEnum.Prefab;
        }
        else if (s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".psd"))
        {
            return SuffixEnum.Image;
        }
        else if (s.EndsWith(".txt") || s.EndsWith(".xml") || s.EndsWith(".json") || s.EndsWith(".shader") || s.EndsWith(".csv"))
        {
            return SuffixEnum.Txt;
        }
        else if (s.EndsWith(".ttf") || s.EndsWith(".fontsettings"))
        {
            return SuffixEnum.Font;
        }
        else if (s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav"))
        {
            return SuffixEnum.Audio;
        }
        else if (s.EndsWith(".mat"))
        {
            return SuffixEnum.Material;
        }
        else if (s.EndsWith(".unity"))
        {
            return SuffixEnum.Scene;
        }
        else if (s.Contains("*all*"))
        {
            return SuffixEnum.All;
        }
        else
        {
            Debug.Log("未知的文件格式: " + s);
            return SuffixEnum.All;
        }
    }

}
