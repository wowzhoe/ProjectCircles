using UnityEngine;
using System.IO;
using SimpleJSON;

public class DataHandle
{
    public static void Save()
    {
        if (!File.Exists(Application.dataPath + "/Resources/" + "game_settings.json"))
            return;

        //записываем str в файл
        var obj = new JSONClass();

        obj["journeyTime"].AsFloat = Global.journeyTime;
        obj["saturation_gradient_time"].AsFloat = Global.saturation_gradient_time;
        obj["pixel_coefficient"].AsFloat = Global.pixel_coefficient;
        obj["Min_Spawn_Distance"].AsFloat = Global.Min_Spawn_Distance;
        obj["Speed_Multiplier"].AsFloat = Global.Speed_Multiplier;
		obj["FadingDelay"].AsFloat = Global.FadingDelay;
		obj["FadingSpeed"].AsFloat = Global.FadingSpeed;
		obj["min_scale"].AsFloat = Global.min_scale;
        obj["max_scale"].AsFloat = Global.max_scale;
        obj["min_speed"].AsFloat = Global.min_speed;
        obj["max_speed"].AsFloat = Global.max_speed;
        obj["game_step"].AsFloat = Global.game_step;
        obj["game_stage"].AsInt = Global.game_stage;
        obj["game_level"].AsInt = Global.game_level;

        var str = obj.ToString();
        File.WriteAllText(Application.dataPath + "/Resources/" + "game_settings.json", str);
        Debug.Log("Game Settings Saved!");
    }

    public static void Load()
    {
        //читаем из файла str
        var str = Resources.Load("game_settings") as TextAsset;
        var obj = JSON.Parse(str.text);

        Global.journeyTime = obj["journeyTime"].AsFloat;
        Global.saturation_gradient_time = obj["saturation_gradient_time"].AsFloat;
        Global.pixel_coefficient = obj["pixel_coefficient"].AsFloat;
        Global.Min_Spawn_Distance = obj["Min_Spawn_Distance"].AsFloat;
        Global.Speed_Multiplier = obj["Speed_Multiplier"].AsFloat;
	    Global.FadingDelay = obj["FadingDelay"].AsFloat;
	    Global.FadingSpeed = obj["FadingSpeed"].AsFloat;
        Global.min_scale = obj["min_scale"].AsFloat;
        Global.max_scale = obj["max_scale"].AsFloat;
        Global.min_speed = obj["min_speed"].AsFloat;
        Global.max_speed = obj["max_speed"].AsFloat;
        Global.game_step = obj["game_step"].AsFloat;
        Global.game_stage = obj["game_stage"].AsInt;
        Global.game_level = obj["game_level"].AsInt;
        Debug.Log("Game Settings Loaded!");
    }
}
