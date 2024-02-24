using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Random = UnityEngine.Random;


#if UNITY_EDITOR
public partial class ItemDB : EditorWindow
{
    private static ItemDB instance = null;

    public static ItemDB Instance
    {
        get
        {
            return instance;
        }
        set
        {
            if (instance == null)
                instance = value;
        }
    }

    Vector2 scrollPos;
    int index;
    Texture2D icon;
    public AssetFile af;
    List<string> names = new List<string>();

    public SerializedObject GetTarget;
    public SerializedProperty temp_list;

    [MenuItem("Editor/GameEditor")]
    public static void Init()
    {
        ItemDB window = (ItemDB)EditorWindow.GetWindow(typeof(ItemDB), true, "GameEditor");
        window.Show();

        DataHandle.Load();
    }

    public List<Item> Items = new List<Item>();
    public List<int> difficulty_easy_list = new List<int>();
    public List<int> difficulty_hard_list = new List<int>();
    public List<int> difficulty_unused_list = new List<int>();

    void AddNew()
    {
        Items.Add(new Item());
    }

    void Remove(int index)
    {
        Items.RemoveAt(index);
    }


    public void OnEnable()
    {
        af = (AssetFile)AssetDatabase.LoadAssetAtPath("Assets/Resources/GetTarget.asset", typeof(AssetFile));

        if (af)
        {
            GetTarget = new SerializedObject(af);
            Items = af.Items;
            af.hideFlags = HideFlags.HideInInspector;
        }
        else
        {
            af = ScriptableObject.CreateInstance<AssetFile>();
            AssetDatabase.CreateAsset(af, "Assets/Resources/GetTarget.asset");
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            af.hideFlags = HideFlags.HideInInspector;

            af.Items = Items;
            af.named = "PYATIHATKA_2";
            EditorUtility.SetDirty(af);
            GetTarget = new SerializedObject(this);
        }

        if (GetTarget != null)
            temp_list = GetTarget.FindProperty("Items");

        AssetDatabase.Refresh();

        foreach (var item in this.Items)
        {
            names.Add(item.name);
        }

    }

    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, false, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));

        GetTarget.Update();

        EditorGUILayout.LabelField("Global Game Settings:");
        EditorGUILayout.Space();

        Global.pixel_coefficient = EditorGUILayout.FloatField("Coefficient:", Global.pixel_coefficient);
        Global.journeyTime = EditorGUILayout.FloatField("Figures Changing Speed:", Global.journeyTime);
        Global.saturation_gradient_time = EditorGUILayout.FloatField("Saturation Gradient Time:", Global.saturation_gradient_time);
        Global.Min_Spawn_Distance = EditorGUILayout.FloatField("Min Spawn Distance:", Global.Min_Spawn_Distance);
        Global.Speed_Multiplier = EditorGUILayout.FloatField("Speed Multiplier:", Global.Speed_Multiplier);
        Global.min_scale = EditorGUILayout.FloatField("Min Scale:", Global.min_scale);
        Global.max_scale = EditorGUILayout.FloatField("Max Scale:", Global.max_scale);
        Global.min_speed = EditorGUILayout.FloatField("Min Speed:", Global.min_speed);
        Global.max_speed = EditorGUILayout.FloatField("Max Speed:", Global.max_speed);
        Global.game_step = EditorGUILayout.FloatField("Game Step:", Global.game_step);

		Global.FadingDelay = EditorGUILayout.FloatField("Fading Delay:", Global.FadingDelay);
		Global.FadingSpeed = EditorGUILayout.FloatField("Fading Speed:", Global.FadingSpeed);

		Global.game_stage = EditorGUILayout.IntField("Game Stage:", Global.game_stage);
        Global.game_level = EditorGUILayout.IntField("Game Level:", Global.game_level);

        if (EditorApplication.isPlaying)
        {
            DataHandle.Load();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Save Settings"))
        {
            DataHandle.Save();
        }

        if (GUILayout.Button("Load Settings"))
        {
            DataHandle.Load();
        }

        EditorGUILayout.LabelField("________________________________________________________");

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Add a new item_shape with a button");

        if (GUILayout.Button("Add New"))
        {
            this.Items.Add(new Item());
            this.Items[this.Items.Count-1].name = this.Items.Count.ToString();
            string name = this.Items.Count.ToString();
            names.Add(name);
            GetTarget.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current Selected Item: ");

        EditorGUILayout.Space();

        index = EditorGUILayout.Popup(index, names.ToArray());

        SerializedProperty item_list = temp_list.GetArrayElementAtIndex(index);
        SerializedProperty item_name = item_list.FindPropertyRelative("name");
        SerializedProperty item_min_scale = item_list.FindPropertyRelative("min_scale");
        SerializedProperty item_max_scale = item_list.FindPropertyRelative("max_scale");
        SerializedProperty item_min_speed = item_list.FindPropertyRelative("min_speed");
        SerializedProperty item_max_speed = item_list.FindPropertyRelative("max_speed");
        SerializedProperty item_coefficient = item_list.FindPropertyRelative("pixel_coefficient");
        SerializedProperty item_shape_data = item_list.FindPropertyRelative("shapeData");
        SerializedProperty item_zone_data = item_list.FindPropertyRelative("zoneData");
        SerializedProperty item_difficulty_group = item_list.FindPropertyRelative("difficulty_Group");

        EditorGUILayout.LabelField("Item " + index.ToString() + " Settings :");
   
        EditorGUILayout.PropertyField(item_name);
        EditorGUILayout.PropertyField(item_min_scale);
        EditorGUILayout.PropertyField(item_max_scale);
        EditorGUILayout.PropertyField(item_min_speed);
        EditorGUILayout.PropertyField(item_max_speed);
        EditorGUILayout.PropertyField(item_coefficient);
        EditorGUILayout.PropertyField(item_shape_data);
        EditorGUILayout.PropertyField(item_zone_data);
        EditorGUILayout.PropertyField(item_difficulty_group);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    
        EditorGUILayout.LabelField("Animation");
        EditorGUILayout.LabelField("Shape: ");

        if (item_shape_data != null && item_shape_data.objectReferenceValue != null)
        {
            if (this.Items[index].tempShapeName != item_shape_data.objectReferenceValue.name)
            {
                this.Items[index].tempShapeName = item_shape_data.objectReferenceValue.name;
                SkeletonDataAsset shape = (SkeletonDataAsset)item_shape_data.objectReferenceValue;
                ParseShape(shape.skeletonJSON, index);
            }
        }
    
        if (item_zone_data != null && item_zone_data.objectReferenceValue != null)
        {
            if (this.Items[index].tempZoneName != item_zone_data.objectReferenceValue.name)
            {
                this.Items[index].tempZoneName = item_zone_data.objectReferenceValue.name;
                SkeletonDataAsset zone = (SkeletonDataAsset)item_zone_data.objectReferenceValue;
                ParseZone(zone.skeletonJSON, index);
            }
        }
    
        this.Items[index].index_shape = EditorGUILayout.Popup(this.Items[index].index_shape, this.Items[index].animation_names_shape.ToArray());
    
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Zone: ");
    
        this.Items[index].index_zone = EditorGUILayout.Popup(this.Items[index].index_zone, this.Items[index].animation_names_zone.ToArray());

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Play"))
        {
            EditorApplication.isPlaying = true;
            EditorApplication.NewScene();
   
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.transform.position = new Vector3(0,0,-10f);
            camera.GetComponent<Camera>().orthographic = true;
            camera.GetComponent<Camera>().orthographicSize = 20f;
   
            GameObject obj_shape = new GameObject();
            GameObject obj_zone = new GameObject();
   
            obj_shape.name = "item_" + (index + 1).ToString() + "_shape";
            obj_zone.name = "item_" + (index + 1).ToString() + "_zone";
   
            obj_shape.transform.position = new Vector3(0, 0, 2f);
            obj_zone.transform.position = new Vector3(0, 0, 0f);
   
            obj_shape.AddComponent<SkeletonAnimation>();
            obj_zone.AddComponent<SkeletonAnimation>();
   
            SkeletonAnimation shape = obj_shape.GetComponent<SkeletonAnimation>();
            SkeletonAnimation zone = obj_zone.GetComponent<SkeletonAnimation>();
   
            shape.skeletonDataAsset = this.Items[index].shapeData;
            zone.skeletonDataAsset = this.Items[index].zoneData;
   
            string animation_shape = this.Items[index].animation_names_shape[this.Items[index].index_shape];
            string animation_zone = this.Items[index].animation_names_zone[this.Items[index].index_zone];
   
            // Скейлы
            if (this.Items[index].min_scale != 0 && this.Items[index].max_scale != 0)
            {
                float scale = Random.Range(this.Items[index].min_scale, this.Items[index].max_scale);
                obj_shape.transform.localScale = new Vector3(scale, scale, scale);
                obj_zone.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                float scale = Random.Range(Global.min_scale, Global.max_scale);
                obj_shape.transform.localScale = new Vector3(scale, scale, scale);
                obj_zone.transform.localScale = new Vector3(scale, scale, scale);
            }
   
            // Скорость проигрывания анимации
            if (this.Items[index].min_speed != 0 && this.Items[index].max_speed != 0)
            {
                float speed = Random.Range(this.Items[index].min_speed , this.Items[index].max_speed);
                shape.timeScale = speed * Global.Speed_Multiplier;
                zone.timeScale = speed * Global.Speed_Multiplier;
            }
            else
            {
                float temp_time_scale = Global.min_speed * Mathf.Pow(Global.game_step, Global.game_level);

                if (temp_time_scale >= Global.max_speed)
                    temp_time_scale = Global.max_speed;
   
                shape.timeScale = temp_time_scale;
                zone.timeScale = temp_time_scale;
            }
   
            shape.Reset();
            shape.loop = true;
            shape.AnimationName = animation_shape;
            shape.state.Event += State_Event;
   
            zone.Reset();
            zone.loop = true;
            zone.AnimationName = animation_zone;
            zone.state.Event += State_Event;
        }   
   
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        difficulty_easy_list = this.Items.IndexesWhere(i => i.difficulty_Group == Item.Difficulty_Group.easy).ToList();
        difficulty_hard_list = this.Items.IndexesWhere(i => i.difficulty_Group == Item.Difficulty_Group.hard).ToList();
        difficulty_unused_list = this.Items.IndexesWhere(i => i.difficulty_Group == Item.Difficulty_Group.unused).ToList();

        af.items_easy = difficulty_easy_list;
        af.items_hard = difficulty_hard_list;
        af.items_unused = difficulty_unused_list;

        GetTarget.ApplyModifiedProperties();

        EditorUtility.SetDirty(af);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GetTarget.Update();

        EditorGUILayout.EndScrollView();
    }


    void OnInspectorUpdate()
    {
        Repaint();
    }


    void ParseShape(TextAsset text, int index)
    {
        JsonData jsonText = JsonMapper.ToObject(text.text);

        foreach (var item in jsonText["animations"])
        {
            this.Items[index].animation_names_shape.Add(((DictionaryEntry)item).Key.ToString());
        }
    }

    void ParseZone(TextAsset text, int index)
    {
        JsonData jsonText = JsonMapper.ToObject(text.text);

        foreach (var item in jsonText["animations"])
        {
            this.Items[index].animation_names_zone.Add(((DictionaryEntry)item).Key.ToString());
        }
    }

    private void State_Event(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {

    }

    void OnDestroy()
    {
        af.Items = Items;
        EditorUtility.SetDirty(af);
    }

}
#endif

public partial class ItemDB
{
    [Serializable]
    public class Item
    {
        public string name;
        public string tempShapeName = "";
        public string tempZoneName = "";

        public float pixel_coefficient;
        public int index_shape;
        public int index_zone;

        public float min_scale;
        public float max_scale;
        public float min_speed;
        public float max_speed;

        public SkeletonDataAsset shapeData;
        public SkeletonDataAsset zoneData;

        public List<string> animation_names_shape = new List<string>();
        public List<string> animation_names_zone = new List<string>();

        public enum Difficulty_Group
        {
            easy,
            hard,
            unused
        }

        public Difficulty_Group difficulty_Group;
    }

    public static object GetVar(string name)
    {
        var af = Resources.Load<AssetFile>(name);
        //var af = (AssetFile)AssetDatabase.LoadAssetAtPath("Assets/Resources/" + name + ".asset", typeof(AssetFile));
        return af.Items;
    }

    public static object GetVarEasy()
    {
        var af = Resources.Load<AssetFile>("GetTarget");
        return af.items_easy;
    }

    public static object GetVarHard()
    {
        var af = Resources.Load<AssetFile>("GetTarget");
        return af.items_hard;
    }

    public static object GetVarUnused()
    {
        var af = Resources.Load<AssetFile>("GetTarget");
        return af.items_unused;
    }
}

public static class Ext
{
    public static IEnumerable<int> IndexesWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (T element in source)
        {
            if (predicate(element))
            {
                yield return index;
            }
            index++;
        }
    }
}
