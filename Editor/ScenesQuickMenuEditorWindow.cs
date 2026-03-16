using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ScenesQuickMenuEditorWindow : EditorWindow
{
    private Vector2 _favoritesScroll;
    private Vector2 _allScenesScroll;
    private List<string> _allScenePaths;
    private List<string> _favoriteScenes;
    private string _searchFilter = string.Empty;
    private static string _editorPrefKey = string.Empty;

    private string _favoriteSceneToRemove = string.Empty;

    private static string GetEditorPrefKey()
    {
        if(!string.IsNullOrEmpty(_editorPrefKey))
        {
            string projectName = new DirectoryInfo(Application.dataPath).Parent.Name;
            _editorPrefKey = $"{projectName}_SceneLoaderFavorites";
        }

        return _editorPrefKey;
    }

    [MenuItem("Tools/GRFT+/Scene Loader #s")]
    public static void ShowWindow()
    {
        if(Application.isPlaying)
        {
            return;
        }

        GetWindow<ScenesQuickMenuEditorWindow>("Scene Loader");
    }

    private void OnEnable()
    {
        _allScenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => path.StartsWith("Assets/"))
            .OrderBy(path => path)
            .ToList();

        string stored = EditorPrefs.GetString(GetEditorPrefKey(), "");
        _favoriteScenes = string.IsNullOrEmpty(stored)? new List<string>() : stored.Split('|').ToList();
        _favoriteScenes = _favoriteScenes.Where(fav => _allScenePaths.Contains(fav)).ToList();
        EditorPrefs.SetString(GetEditorPrefKey(), string.Join("|", _favoriteScenes));
    }

    private void OnGUI()
    {
        DrawSearchBar();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.45f));
        DrawColumnHeader("Favorites");
        _favoritesScroll = EditorGUILayout.BeginScrollView(_favoritesScroll);

        foreach (string scenePath in FilteredFavorites())
        {
            DrawSceneRow(scenePath, true);
        }

        if (!string.IsNullOrEmpty(_favoriteSceneToRemove))
        {
            _favoriteScenes.Remove(_favoriteSceneToRemove);
            _favoriteSceneToRemove = string.Empty;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        DrawVerticalSeparator();

        EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.5f));
        DrawColumnHeader("All Scenes");
        _allScenesScroll = EditorGUILayout.BeginScrollView(_allScenesScroll);

        foreach (string scenePath in FilteredAllScenes())
        {
            DrawSceneRow(scenePath, false);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSearchBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        _searchFilter = GUILayout.TextField(_searchFilter, GUI.skin.FindStyle("ToolbarSearchTextField"));

        if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            _searchFilter = string.Empty;
            GUI.FocusControl(null);
        }

        EditorGUILayout.EndHorizontal();
    }

    private IEnumerable<string> FilteredFavorites()
    {
        if (string.IsNullOrEmpty(_searchFilter))
        {
            return _favoriteScenes;
        }

        return _favoriteScenes.Where(p => Path.GetFileNameWithoutExtension(p)
                .ToLowerInvariant()
                .Contains(_searchFilter.ToLowerInvariant()));
    }

    private IEnumerable<string> FilteredAllScenes()
    {
        if (string.IsNullOrEmpty(_searchFilter))
        {
            return _allScenePaths;
        }

        return _allScenePaths.Where(p => Path.GetFileNameWithoutExtension(p)
                .ToLowerInvariant()
                .Contains(_searchFilter.ToLowerInvariant()));
    }

    private void DrawSceneRow(string scenePath, bool isFavoriteColumn)
    {
        EditorGUILayout.BeginHorizontal("box");

        if (GUILayout.Button(Path.GetFileNameWithoutExtension(scenePath), GUILayout.Height(25)))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
                Close();
            }
        }

        if (isFavoriteColumn)
        {
            if (GUILayout.Button("Remove", GUILayout.Width(60), GUILayout.Height(25)))
            {
                _favoriteSceneToRemove = scenePath;
                EditorPrefs.SetString(GetEditorPrefKey(), string.Join("|", _favoriteScenes));
            }
        }
        else
        {
            EditorGUI.BeginDisabledGroup(_favoriteScenes.Contains(scenePath));
            if (GUILayout.Button("Add", GUILayout.Width(60), GUILayout.Height(25)))
            {
                _favoriteScenes.Add(scenePath);
                EditorPrefs.SetString(GetEditorPrefKey(), string.Join("|", _favoriteScenes));
            }
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private void DrawVerticalSeparator()
    {
        Rect rect = GUILayoutUtility.GetRect(2, position.height, GUILayout.Width(2));
        EditorGUI.DrawRect(rect, Color.gray);
        GUILayout.Space(4);
    }

    private void DrawColumnHeader(string title)
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };
        style.normal.textColor = Color.white;

        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(25));
        EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 1f));
        EditorGUI.LabelField(rect, title, style);
        GUILayout.Space(5);
    }
}