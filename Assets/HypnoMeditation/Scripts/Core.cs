using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;


public class Core : MonoBehaviour {

    private static Core instance = null;

    public static Core Instance
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

    // Игровая камера
    public Camera gameCamera;

    public SkeletonDataAsset transition_up_data;
    public SkeletonDataAsset transition_under_data;

    // Вектор3 разницы юнита в пикселях
    Vector3 diff;

    // Вектор3 разницы пикселей в юнитах
    Vector3 dif;

    // Текущий Resolution который берётся от размера камеры в пикселях
    float default_height;
    float default_width;

    // Блок экрана на основе Текущего Resolution
    float margin_x;
    float margin_y;

    // Координаты точек на экране для спавна обьектов 
    float spawn_pixel_x;
    float spawn_pixel_y;

    // Игровой экран внутри нашего Текущего Resolution
    Rect internal_screen;

    // Текущий массив для подбора градиента
    public Gradient MyGradient;

    // Текущий цвет который будет применятся к игровым обьектам
    public Color play_cl;
    public Color win_cl;
    public Color lose_cl;

    // Текущая позиция для спавна обьектов-элементов
    Vector3 spawn_position;

    bool can_tap;
    bool can_start_tap;

    GameObject item_shape;
    GameObject item_zone;
    GameObject item_transition_up;
    GameObject item_transition_under;
    GameObject item_transition_change;
    GameObject bg;
    GameObject level_show;
    GameObject level_stage;
    GameObject panel_success;
    GameObject panel_fail;
    GameObject panel_success_text;
    GameObject panel_fail_text;
    GameObject button_reset_shape;
    GameObject prev_item_shape;
    GameObject prev_item_zone;
	GameObject item_frame;
	GameObject item_lives;
	GameObject item_background_lives;


    List<ItemDB.Item> item_data_base;
    List<int> item_data_easy;
    List<int> item_data_hard;

    Vector3 spawn_scale;
    float scale;
    float startTime;
    float journeyLength;
    int r;
    string animation_name;

    enum item_Status
    {
        in_play,
        not_in_play,
    }

    item_Status item_status;

    enum game_Stage
    {
        win_stage,
        lose_stage
    }

    game_Stage game_stage_status;

    enum game_status
    {
        playing,
        win_level,
        lose_level
    }

    game_status current_game_status;

    int game_stage;
    int game_level;

    bool game_init;
    bool game_start;
    bool game_over;
    bool game_easy;
    bool item_can_move;
    bool item_can_blank;
    bool start_time_scale;

    float temp_time_scale;
    float RealTimeScale;
    float t_animation;
    float saturation_time;

    // Включение Дебаг Моба
    public enum DebugMode
    {
        DebugMode,
        PlayMode,
        TestMode
    }

    public DebugMode GameMode;

    public int debug_level;
    public float debug_step;
    public int debug_shape;

	SkeletonAnimation frame_skeleton;

	string[] bg_animation_names = new string[8]
	{
		"animation_1",
		"animation_2",
		"animation_3",
		"animation_4",
		"animation_5",
		"animation_6",
		"animation_7",
		"animation_8"
	};

	bool frame_closed;
	bool frame_fading;
	float frame_fading_time;

	public int player_lives;


    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Use this for initialization
    void Start () {

        game_init = true;
	    DataHandle.Load();
        if (GameMode == DebugMode.DebugMode)
            VisualLog.Initialize();
        UnitToPixel();
        CalculateGameRectSize();
        PreLoadUI();
        Init();
        ChangeUI();
    }

    // Update is called once per frame
    void Update () {

        if (GameMode == DebugMode.DebugMode)
            VisualLog.Update();
        RandomItemAnimation();
        CalculateGradient();
        SaturationGradient();
        InputHandler();
        ShowPlayUI();
        ItemMoveToSpawnPosition();
        LerpGradient();
	    FadingDelay();
    }


    #region Initialization
    void UnitToPixel()
    {
        Vector3 min_x = Vector3.zero;
        Vector3 max_x = new Vector3(1, 0, 0);

        Vector3 screen_min_x = gameCamera.WorldToScreenPoint(min_x);
        Vector3 screen_max_x = gameCamera.WorldToScreenPoint(max_x);

        diff = screen_max_x - screen_min_x;
        dif = gameCamera.ScreenToWorldPoint(diff);
    }

    void CalculateGameRectSize()
    {
        default_height = gameCamera.pixelHeight;
        default_width = gameCamera.pixelWidth;

        margin_x = default_width * Global.pixel_coefficient;
        margin_y = default_height * Global.pixel_coefficient;

        internal_screen = new Rect(margin_x * 2f, margin_y * 2f, default_width - (margin_x * 2f), default_height - (margin_y * 2f));
    }

    void PreLoadUI()
    {
        bg = GameObject.Find("background");
        level_show = GameObject.Find("game_level");
        level_stage = GameObject.Find("game_stage");
        panel_success = GameObject.Find("PanelSuccess");
        panel_fail = GameObject.Find("PanelFail");
        panel_success_text = GameObject.Find("TextNextLevel");
        panel_fail_text = GameObject.Find("TextGameRetry");
        item_transition_change = GameObject.Find("changescreens");
        button_reset_shape = GameObject.Find("ButtonReset");
		item_frame = GameObject.Find("frame");
	    item_lives = GameObject.Find("lives");
		item_background_lives = GameObject.Find("back_lives");
    }

    void Init()
    {
        switch (GameMode)
        {
            case DebugMode.PlayMode:
                if (PlayerPrefs.GetInt("game_level") == 0)
                {
                    game_level = 1;
                    PlayerPrefs.SetInt("game_level", game_level);
                    PlayerPrefs.Save();
                }
                else
                    game_level = PlayerPrefs.GetInt("game_level");

                if (PlayerPrefs.GetInt("max_game_level") == 0)
                {
                    PlayerPrefs.SetInt("max_game_level", game_level);
                    PlayerPrefs.Save();
                }
                button_reset_shape.SetActive(false);
                break;
            case DebugMode.DebugMode:
                if (debug_level == 0)
                    debug_level = 1;
                game_level = debug_level;

                button_reset_shape.SetActive(true);
                break;
                case DebugMode.TestMode:
                if (PlayerPrefs.GetInt("game_level") == 0)
                {
                    game_level = 1;
                    PlayerPrefs.SetInt("game_level", game_level);
                    PlayerPrefs.Save();
                }
                else
                    game_level = PlayerPrefs.GetInt("game_level");

                if (PlayerPrefs.GetInt("max_game_level") == 0)
                {
                    PlayerPrefs.SetInt("max_game_level", game_level);
                    PlayerPrefs.Save();
                }
                button_reset_shape.SetActive(false);
                break;
        }

        game_stage = game_level;

        level_show.GetComponent<tk2dTextMesh>().text = "Level:" + game_level;
        level_stage.GetComponent<tk2dTextMesh>().text = game_stage.ToString();

        item_data_base = ItemDB.GetVar("GetTarget") as List<ItemDB.Item>;
        item_data_easy = ItemDB.GetVarEasy() as List<int>;
        item_data_hard = ItemDB.GetVarHard() as List<int>;

        panel_fail.SetActive(false);
        panel_success.SetActive(false);
        panel_success_text.SetActive(false);
        panel_fail_text.SetActive(false);

        game_init = false;
        game_start = true;
        game_easy = true;
        game_over = false;
        can_start_tap = true;
        item_can_move = false;
        item_can_blank = false;
        start_time_scale = false;
	    frame_closed = false;
	    frame_fading = false;

		saturation_time = Global.saturation_gradient_time;
		frame_fading_time = Global.FadingDelay;

		frame_skeleton = item_frame.GetComponent<SkeletonAnimation>();

	    player_lives = 3;
    }
    #endregion

    #region Spawn
    void SpawnInitPosition()
    {
        RandomRangeItem();

        // Пиксельный коеффициент
        if (item_data_base[r].pixel_coefficient != 0)
        {
            margin_x = default_width * item_data_base[r].pixel_coefficient;
            margin_y = default_height * item_data_base[r].pixel_coefficient;

            spawn_pixel_x = Convert.ToInt32(Random.Range(margin_x, default_width - margin_x));
            spawn_pixel_y = Convert.ToInt32(Random.Range(margin_y, default_height - margin_y));
        }
        else
        {
            margin_x = default_width * Global.pixel_coefficient;
            margin_y = default_height * Global.pixel_coefficient;

            spawn_pixel_x = Convert.ToInt32(Random.Range(margin_x, default_width - margin_x));
            spawn_pixel_y = Convert.ToInt32(Random.Range(margin_y, default_height - margin_y));
        }

	    spawn_position = Vector3.zero; // = gameCamera.ScreenToWorldPoint(new Vector3(spawn_pixel_x, spawn_pixel_y, 20));
    }

    void SpawnPosition()
    {
        RandomRangeItem();

        if (r >= item_data_base.Count)
            r = item_data_base.Count - 1;

		spawn_position = Vector3.zero;

//		do
//        {
//            // Пиксельный коеффициент
//            if (item_data_base[r].pixel_coefficient != 0)
//            {
//                margin_x = default_width * item_data_base[r].pixel_coefficient;
//                margin_y = default_height * item_data_base[r].pixel_coefficient;
//
//                spawn_pixel_x = Convert.ToInt32(Random.Range(margin_x, default_width - margin_x));
//                spawn_pixel_y = Convert.ToInt32(Random.Range(margin_y, default_height - margin_y));
//            }
//            else
//            {
//                margin_x = default_width * Global.pixel_coefficient;
//                margin_y = default_height * Global.pixel_coefficient;
//
//                spawn_pixel_x = Convert.ToInt32(Random.Range(margin_x, default_width - margin_x));
//                spawn_pixel_y = Convert.ToInt32(Random.Range(margin_y, default_height - margin_y));
//            }
//
//            spawn_position = gameCamera.ScreenToWorldPoint(new Vector3(spawn_pixel_x, spawn_pixel_y, 20));
//
//			if (item_shape != null && GameMode == DebugMode.DebugMode)
//                VisualLog.Note("Distance: " + Vector3.Distance(spawn_position, item_shape.transform.position).ToString(CultureInfo.InvariantCulture));
//            break;
//        }
//        while (Vector3.Distance(spawn_position, item_shape.transform.position) > Global.Min_Spawn_Distance);
    }

    void SpawnItem()
    {
        level_stage.SetActive(true);

        GameObject obj_shape = new GameObject();
        GameObject obj_zone = new GameObject();

        obj_shape.name = "item_" + (r + 1) + "_shape";
        obj_zone.name = "item_" + (r + 1) + "_zone";

        item_shape = obj_shape.gameObject;
        item_zone = obj_zone.gameObject;

        item_status = item_Status.in_play;
        startTime = Time.time;
        journeyLength = Vector3.Distance(item_shape.transform.position, spawn_position);

        item_shape.transform.position = spawn_position + new Vector3(0, 0, 5f);
        item_zone.transform.position = spawn_position;

        // Скейлы
        if (item_data_base[r].min_scale != 0 && item_data_base[r].max_scale != 0)
        {
            scale = Random.Range(item_data_base[r].min_scale, item_data_base[r].max_scale);
        }
        else if (item_data_base[r].min_scale == 0 && item_data_base[r].max_scale == 0)
        {
            scale = Random.Range(Global.min_scale, Global.max_scale);
        }

        spawn_scale = new Vector3(scale, scale, scale);

        if (!game_start)
        {
            item_shape.transform.localScale = new Vector3(0, 0, 0);
            item_zone.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            item_shape.transform.localScale = spawn_scale;
            item_zone.transform.localScale = spawn_scale;
        }

        SkeletonAnimation shape = item_shape.AddComponent<SkeletonAnimation>();
        SkeletonAnimation zone = item_zone.AddComponent<SkeletonAnimation>();

        shape.skeletonDataAsset = item_data_base[r].shapeData;
        zone.skeletonDataAsset = item_data_base[r].zoneData;

        string animation_shape = item_data_base[r].animation_names_shape[item_data_base[r].index_shape];
        string animation_zone = item_data_base[r].animation_names_zone[item_data_base[r].index_zone];

        shape.Reset();
        shape.loop = true;
        shape.AnimationName = animation_shape;
        shape.state.Event += State_Event;

        zone.Reset();
        zone.loop = true;
        zone.AnimationName = animation_zone;

        // Скорость проигрывания анимации
        if (item_data_base[r].min_speed != 0 && item_data_base[r].max_speed != 0)
        {
            switch (GameMode)
            {
                case DebugMode.PlayMode:
                    temp_time_scale = item_data_base[r].min_speed * Global.Speed_Multiplier * Mathf.Pow(Global.game_step, Global.game_level);
                    break;
                case DebugMode.TestMode:
                    temp_time_scale = item_data_base[r].min_speed * Global.Speed_Multiplier * Mathf.Pow(Global.game_step, Global.game_level);
                    break;
                case DebugMode.DebugMode:
                    temp_time_scale = item_data_base[r].min_speed * Global.Speed_Multiplier * Mathf.Pow(debug_step, debug_level);
                    break;
            }

            if (temp_time_scale >= (item_data_base[r].max_speed * Global.Speed_Multiplier))
                temp_time_scale = item_data_base[r].max_speed * Global.Speed_Multiplier;
        }
        else if (item_data_base[r].min_speed == 0 && item_data_base[r].max_speed == 0)
        {
            switch (GameMode)
            {
                case DebugMode.PlayMode:
                    temp_time_scale = Global.min_speed * Mathf.Pow(Global.game_step, Global.game_level);
                    break;
                case DebugMode.TestMode:
                    temp_time_scale = Global.min_speed * Mathf.Pow(Global.game_step, Global.game_level);
                    break;
                case DebugMode.DebugMode:
                    temp_time_scale = Global.min_speed * Mathf.Pow(debug_step, debug_level);
                    break;
            }

            if (temp_time_scale >= Global.max_speed)
                temp_time_scale = Global.max_speed;
        }

        RealTimeScale = temp_time_scale;
        float t = Random.Range(150f, 250f);
        shape.timeScale = t;
        zone.timeScale = t;
        t_animation = Random.Range(0.0f, 0.1f);
        start_time_scale = true;
	    item_shape.GetComponent<MeshRenderer>().enabled = false;
		item_zone.GetComponent<MeshRenderer>().enabled = false;

		// Ротация
		int rotation = Random.Range(15, 360);

        shape.transform.eulerAngles = new Vector3(0, 0, rotation);
        zone.transform.eulerAngles = new Vector3(0, 0, rotation);

        level_stage.transform.position = item_zone.transform.position;

        can_tap = false;

        switch (GameMode)
        {
            case DebugMode.DebugMode:
                VisualLog.Note("Name: " + item_shape.name);
                VisualLog.Note("Time Scale: " + temp_time_scale.ToString(CultureInfo.InvariantCulture));
                VisualLog.Note("Spawn Scale: " + spawn_scale);
                VisualLog.Note("Position: " + (new Vector2(spawn_pixel_x, spawn_pixel_y)));
                break;
            case DebugMode.TestMode:
                VisualLog.entries.Clear();
                VisualLog.Note("Name: " + item_shape.name);
                break;
        }
    }

    void SpawnTransitionStage()
    {
        GameObject obj_transition_under = new GameObject();

        item_transition_under = obj_transition_under;
        item_transition_under.transform.localScale = item_shape.transform.localScale;

        obj_transition_under.name = "item_transition_under";
        obj_transition_under.transform.position = item_shape.transform.position + new Vector3(0,0,-15f);

        SkeletonAnimation transition_under = obj_transition_under.AddComponent<SkeletonAnimation>();
        transition_under.skeletonDataAsset = transition_under_data;

        switch (game_stage_status)
        {
            case game_Stage.win_stage:
                animation_name = "animation_yes";
                break;
            case game_Stage.lose_stage:
                animation_name = "animation_no";
                break;
        }

        transition_under.Reset();
        transition_under.loop = false;
        transition_under.AnimationName = animation_name;
        transition_under.timeScale = 0.9f;
        transition_under.state.Complete += MyLoopListener;

        obj_transition_under.GetComponent<MeshRenderer>().enabled = false;
	}

    void ItemMoveToSpawnPosition()
    {
        if (item_shape != null && item_status == item_Status.in_play && !game_start && !game_over)
        {
            float distCovered = (Time.time - startTime) * Global.journeyTime;
            float fracJourney = distCovered / journeyLength;

            item_shape.transform.localScale = Vector3.Lerp(item_shape.transform.localScale, spawn_scale, fracJourney);
            item_zone.transform.localScale = Vector3.Lerp(item_zone.transform.localScale, spawn_scale, fracJourney);

            if (item_shape.transform.localScale.x >= (spawn_scale /2).x && !can_start_tap)
            {
                can_start_tap = true;
				//frame_fading = true;
				return;
            }
        }
        else if (item_shape != null && item_status != item_Status.in_play && !game_start && !game_over)
        {
            if (item_can_move)
            {
                float distCovered = (Time.time - startTime) * Global.journeyTime;
                float fracJourney = distCovered / journeyLength;
                
                item_shape.transform.position = Vector3.Lerp(item_shape.transform.position, spawn_position, fracJourney);
                item_zone.transform.position = Vector3.Lerp(item_zone.transform.position, spawn_position, fracJourney);
                item_shape.transform.localScale = Vector3.Lerp(item_shape.transform.localScale, Vector3.zero, fracJourney);
                item_zone.transform.localScale = Vector3.Lerp(item_zone.transform.localScale, Vector3.zero, fracJourney);

                level_stage.transform.position = Vector3.Lerp(level_stage.transform.position, spawn_position, fracJourney);
                item_transition_under.transform.position = Vector3.Lerp(item_transition_under.transform.position, spawn_position, fracJourney);

                if (Vector3.Distance(item_shape.transform.position, spawn_position) <= 0.5f)
                {
                    if (game_stage == game_level)
                        game_over = true;
                    else
                    {
                        //Win
                        Reload();
                        SpawnItem();
                    }

                    item_can_move = false;
                    return;
                }
            }
        }
    }

    void RandomRangeItem()
    {
        switch(GameMode)
        {
            case DebugMode.DebugMode:
                for (int i = 0; i < item_data_base.Count; i++)
                {
                    if (i == (debug_shape - 1))
                    {
                        r = (debug_shape - 1);
                        return;
                    }
                }
                break;
            case DebugMode.PlayMode:
                if (game_easy)
                {
                    int e = Random.Range(0, item_data_easy.Count);
                    r = item_data_easy[e];
                    game_easy = false;
                }
                else
                {
                    int h = Random.Range(0, item_data_hard.Count);
                    r = item_data_hard[h];
                    game_easy = true;
                }
                break;
            case DebugMode.TestMode:
                if (game_easy)
                {
                    int e = Random.Range(0, item_data_easy.Count);
                    r = item_data_easy[e];
                    game_easy = false;
                }
                else
                {
                    int h = Random.Range(0, item_data_hard.Count);
                    r = item_data_hard[h];
                    game_easy = true;
                }
                break;
        }

        if (r >= item_data_base.Count)
            r = item_data_base.Count - 1;
    }

    void RandomItemAnimation()
    {
        if (start_time_scale)
        {
            t_animation -= Time.deltaTime;

            if (t_animation <= 0)
            {
				item_shape.GetComponent<MeshRenderer>().enabled = true;
				item_zone.GetComponent<MeshRenderer>().enabled = true;
				item_shape.GetComponent<SkeletonAnimation>().timeScale = RealTimeScale;
                item_zone.GetComponent<SkeletonAnimation>().timeScale = RealTimeScale;
                start_time_scale = false;
                t_animation = 0;
            }
        }
    }
    #endregion

    #region Gradient
    void CalculateGradient()
    {
        //float t = Mathf.PingPong(Time.time / Global.saturation_gradient_time, 1f);
        float t = PingPong(Time.time / saturation_time, 0f,1f);
        play_cl = MyGradient.Evaluate(t);
    }

    float PingPong(float t, float minLength, float maxLength)
    {
        return Mathf.PingPong(t, maxLength - minLength) + minLength;
    }

    void SaturationGradient()
    {
        if (item_shape != null && item_status == item_Status.in_play && !game_over)
        {
            SkeletonAnimation sk = item_shape.GetComponent<SkeletonAnimation>();

            sk.skeleton.r = play_cl.r;
            sk.skeleton.g = play_cl.g;
            sk.skeleton.b = play_cl.b;
            sk.skeleton.a = play_cl.a;
        }

        bg.GetComponent<SkeletonAnimation>().skeleton.r = play_cl.r;
        bg.GetComponent<SkeletonAnimation>().skeleton.g = play_cl.g;
        bg.GetComponent<SkeletonAnimation>().skeleton.b = play_cl.b;
        bg.GetComponent<SkeletonAnimation>().skeleton.a = play_cl.a;

        item_transition_change.GetComponent<SkeletonAnimation>().skeleton.r = play_cl.r;
        item_transition_change.GetComponent<SkeletonAnimation>().skeleton.g = play_cl.g;
        item_transition_change.GetComponent<SkeletonAnimation>().skeleton.b = play_cl.b;
        item_transition_change.GetComponent<SkeletonAnimation>().skeleton.a = play_cl.a;

		frame_skeleton.skeleton.r = play_cl.r;
		frame_skeleton.skeleton.g = play_cl.g;
		frame_skeleton.skeleton.b = play_cl.b;
		frame_skeleton.skeleton.a = play_cl.a;

		item_background_lives.GetComponent<SkeletonAnimation>().skeleton.r = play_cl.r;
		item_background_lives.GetComponent<SkeletonAnimation>().skeleton.g = play_cl.g;
		item_background_lives.GetComponent<SkeletonAnimation>().skeleton.b = play_cl.b;
		item_background_lives.GetComponent<SkeletonAnimation>().skeleton.a = play_cl.a;
	}

    void LerpGradient()
    {
        if (item_can_blank && item_shape != null && item_status == item_Status.not_in_play)
        {
            SkeletonAnimation sk = item_shape.GetComponent<SkeletonAnimation>();

			float distCovered = (Time.time - startTime) * Global.journeyTime;
            float fracJourney = distCovered / 5f;

            if (game_stage_status == game_Stage.win_stage)
            {
                sk.skeleton.r = Mathf.Lerp(sk.skeleton.r, win_cl.r, fracJourney);
                sk.skeleton.g = Mathf.Lerp(sk.skeleton.g, win_cl.g, fracJourney);
                sk.skeleton.b = Mathf.Lerp(sk.skeleton.b, win_cl.b, fracJourney);
                sk.skeleton.a = Mathf.Lerp(sk.skeleton.a, win_cl.a, fracJourney);
            }
            else if (game_stage_status == game_Stage.lose_stage)
            {
                sk.skeleton.r = Mathf.Lerp(sk.skeleton.r, lose_cl.r, fracJourney);
                sk.skeleton.g = Mathf.Lerp(sk.skeleton.g, lose_cl.g, fracJourney);
                sk.skeleton.b = Mathf.Lerp(sk.skeleton.b, lose_cl.b, fracJourney);
                sk.skeleton.a = Mathf.Lerp(sk.skeleton.a, lose_cl.a, fracJourney);
            }

			if (!frame_closed)
				frame_skeleton.skeleton.a = Mathf.Lerp(frame_skeleton.skeleton.a, 0f, fracJourney * 4f);
		}
    }

    #endregion

    #region Input
    void InputHandler()
    {
        if (Input.GetMouseButtonDown(0) && can_start_tap && !game_over && item_status == item_Status.in_play && !EventSystem.current.IsPointerOverGameObject())
        {
			prev_item_shape = item_shape;
            prev_item_zone = item_zone;

			if (can_tap)
            {
				WinStage();

                if (game_stage <= 0)
                    WinLevel();
                else
                    SpawnPosition();
            }
            else
                LoseLevel();
        }
    }
    #endregion

    #region GameStatus
    void WinStage()
    {
        item_can_blank = true;
        startTime = Time.time;

        game_stage--;
        item_status = item_Status.not_in_play;
        game_stage_status = game_Stage.win_stage;
        journeyLength = Vector3.Distance(item_shape.transform.position, spawn_position);
        game_start = false;
        can_start_tap = false;
        SpawnTransitionStage();
        ReloadAnimation();
        level_stage.SetActive(false);

		frame_fading_time = Global.FadingDelay;
	    frame_skeleton.timeScale = 0;
		frame_fading = true;
	}

    void WinLevel()
    {
        item_can_blank = true;
        startTime = Time.time;

        game_level++;
        game_stage = game_level;
        if (game_level > PlayerPrefs.GetInt("max_game_level"))
            PlayerPrefs.SetInt("max_game_level", game_level);
        PlayerPrefs.SetInt("game_level", game_level);
        PlayerPrefs.Save();
        current_game_status = game_status.win_level;
        game_start = false;
        can_start_tap = false;
        game_over = true;

		frame_skeleton.timeScale = 0;
	}

    void LoseLevel()
    {
        item_can_blank = true;
        startTime = Time.time;

		item_status = item_Status.not_in_play;
		game_stage_status = game_Stage.lose_stage;

		can_start_tap = false;
		game_start = false;
		game_over = true;
		//SpawnTransitionStage();
		ReloadAnimation();

		prev_item_shape = item_shape;
		prev_item_zone = item_zone;

		frame_skeleton.timeScale = 0;

		SetLives();
    }

	void FadingDelay()
	{
		if (frame_fading && current_game_status == game_status.playing)
		{
			frame_fading_time -= Time.deltaTime;

			if (frame_fading_time <= 0)
			{
				frame_skeleton.Reset();
				frame_skeleton.timeScale = 1f * Global.FadingSpeed;
				frame_skeleton.loop = false;
				frame_skeleton.state.Event += Frame_State_Event;
				frame_fading_time = Global.FadingDelay;
				frame_fading = false;
			}
		}
	}


	void ReloadDelay()
	{
		frame_skeleton.Reset();
		frame_skeleton.timeScale = 1f * Global.FadingSpeed;
		frame_skeleton.loop = false;
		frame_skeleton.state.Event += Frame_State_Event;
		frame_fading_time = Global.FadingDelay;
		frame_fading = false;
	}    
    #endregion

    #region Reload
    void Reload()
    {
        if (!game_over)
        {
            Destroy(item_shape.gameObject);
            Destroy(item_zone.gameObject);
        }

        if (item_transition_under != null)
        {
            Destroy(item_transition_under.gameObject);
        }

        item_shape = null;
        item_zone = null;
    }

    void ReloadStage()
    {
        game_stage = game_level;
    }

    void ReloadLevel()
    {
        game_start = true;
        game_easy = true;
        game_over = false;
        can_start_tap = true;

        ReloadStage();
        SpawnPosition();
        SpawnItem();
    }

    void ReloadAnimation()
    {
        if (item_shape != null && item_zone != null)
        {
            item_shape.GetComponent<SkeletonAnimation>().timeScale = 0;
            item_zone.GetComponent<SkeletonAnimation>().timeScale = 0;
        }
    }
    #endregion

    #region Spine Animation State
    private void State_Event(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
        if (e.Data.name == "point_2")
            can_tap = false;
        else if (e.Data.name == "point_1")
            can_tap = true;

        if (e.Data.name == "change_screens")
        {
            if (game_init)
            {
				SpawnInitPosition();
                SpawnItem();
                game_init = false;
			}
            else
            {
				frame_skeleton.Reset();

				ReloadUI();
                if (current_game_status == game_status.win_level)
                    ShowSuccessUI();
                else if (current_game_status == game_status.lose_level)
                    ShowFailUI();
                else
                    ReloadLevel();

                if (prev_item_shape != null)
                {
                    Destroy(prev_item_shape);
                    Destroy(prev_item_zone);
                }
            }
        }
    }

	private void Frame_State_Event(Spine.AnimationState state, int trackIndex, Spine.Event e)
	{
		if (e.Data.name == "over")
		{
			frame_closed = true;

			prev_item_shape = item_shape;
			prev_item_zone = item_zone;

			frame_skeleton.timeScale = 0;

			if (prev_item_shape != null) {
				Destroy(prev_item_shape);
				Destroy(prev_item_zone);
				Destroy(item_shape);
				Destroy(item_zone);
			}

			SetLives();
		}
	}

    public void MyStartListener(Spine.AnimationState state, int trackIndex)
    {
        //Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": start ");
    }

    public void MyLoopListener(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        item_can_move = true;
		frame_skeleton.Reset();
		//Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": complete [" + loopCount + " loops]");
	}

    public void MyEndListener(Spine.AnimationState state, int trackIndex)
    {
        SkeletonAnimation transition_change = item_transition_change.GetComponent<SkeletonAnimation>();

        transition_change.Reset();
        transition_change.timeScale = 0;

	    if (current_game_status == game_status.playing)
			frame_fading = true;

		//Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": end");
	}


	public void HearthChangeEnd(Spine.AnimationState state, int trackIndex, int loopCount)
	{
		SkeletonAnimation sa = item_lives.GetComponent<SkeletonAnimation>();
		sa.Reset();

		switch (player_lives) {
			case 0:
				sa.AnimationName = "heart_0_static";
				sa.state.Complete -= HearthChangeEnd;

				current_game_status = game_status.lose_level;
				//ChangeUI();
				frame_skeleton.timeScale = 0;
				break;
			case 1:
				sa.AnimationName = "heart_1_static";
				sa.state.Complete -= HearthChangeEnd;
                break;
			case 2:
				sa.AnimationName = "heart_2_static";
				sa.state.Complete -= HearthChangeEnd;
                break;
			case 3:
				sa.AnimationName = "heart_3_static";
				sa.state.Complete -= HearthChangeEnd;
                break;
		}

		ChangeUI(); ReloadStage();

		frame_fading_time = Global.FadingDelay;
		frame_skeleton.timeScale = 0;
		frame_fading = true;
	}
	#endregion

	#region Lives
	void SetLives()
	{
		SkeletonAnimation sa = item_lives.GetComponent<SkeletonAnimation>();
		sa.timeScale = 0.5f;
		sa.Reset();

		player_lives--;
		if (player_lives <= 0)
			player_lives = 0;

		switch (player_lives) {
			case 0:
				sa.AnimationName = "heart_lower_1";
				sa.state.Complete += HearthChangeEnd;
				break;
			case 1:
				sa.AnimationName = "heart_lower_2";
				sa.state.Complete += HearthChangeEnd;
				break;
			case 2:
				sa.AnimationName = "heart_lower_3";
				sa.state.Complete += HearthChangeEnd;
				break;
			case 3:
				break;
		}
	}
	#endregion

	#region UI
	void RandomBGAnimation()
	{
		int r = Random.Range(0, bg_animation_names.Length);
		bg.GetComponent<SkeletonAnimation>().AnimationName = bg_animation_names[r];
		bg.GetComponent<SkeletonAnimation>().Reset();
    }

    void ShowPlayUI()
    {
        if (!game_over)
        {
            level_show.GetComponent<tk2dTextMesh>().text = "Level:" + game_level.ToString();
            level_stage.GetComponent<tk2dTextMesh>().text = game_stage.ToString();
        }
        else
        {
            if (current_game_status == game_status.lose_level)
                panel_fail_text.GetComponent<tk2dTextMesh>().text = "Level: " + game_level.ToString();
            else if (current_game_status == game_status.win_level)
                panel_success_text.GetComponent<tk2dTextMesh>().text = "Level: " + (game_level - 1).ToString() + "\n" + " is completed";
        }

        if (game_over && item_can_move)
        {
            Reload();
            ChangeUI();
            item_can_move = false;
        }
    }

    void ShowFailUI()
    {
        level_show.SetActive(false);
        level_stage.SetActive(false);
        panel_success.SetActive(false);
        panel_fail.SetActive(true);
        panel_fail_text.SetActive(true);

	    RandomBGAnimation();
    }

    void ShowSuccessUI()
    {
        level_show.SetActive(false);
        level_stage.SetActive(false);
        panel_fail.SetActive(false);
        panel_success.SetActive(true);
        panel_success_text.SetActive(true);

		RandomBGAnimation();
	}

    void ReloadUI()
    {
        level_show.SetActive(true);
        level_stage.SetActive(true);
        panel_fail.SetActive(false);
        panel_success.SetActive(false);
        panel_fail_text.SetActive(false);
        panel_success_text.SetActive(false);
    }

    void ChangeUI()
    {
		level_stage.SetActive(false);

        SkeletonAnimation transition_change = item_transition_change.GetComponent<SkeletonAnimation>();

        transition_change.Reset();
        transition_change.timeScale = 1;
        transition_change.AnimationName = "animation";
        transition_change.state.End += MyEndListener;
        transition_change.state.Event += State_Event;
    }

    void OnGUI()
    {
        VisualLog.Visualize();
    }
    #endregion

    #region UI Buttons
    public void OnClickButtonNextLevel()
    {
        if (current_game_status != game_status.playing)
        {
            current_game_status = game_status.playing;
            ChangeUI();
        }
    }

    public void OnClickButtonRetry()
    {
        if (current_game_status != game_status.playing && player_lives > 0)
        {
            current_game_status = game_status.playing;
            ChangeUI();
        }
    }

    public void OnClickButtonNextRetry()
    {
        if (game_level < PlayerPrefs.GetInt("max_game_level"))
            game_level++;
    }

    public void OnClickButtonPrevRetry()
    {
        if (game_level <= PlayerPrefs.GetInt("max_game_level"))
            game_level--;

        if (game_level <= 1)
            game_level = 1;
    }

    public void OnClickButtonReset()
    {
        ReloadItemDebug();
    }

	public void OnClickButtonHearts()
	{
		player_lives = 3;

		SkeletonAnimation sa = item_lives.GetComponent<SkeletonAnimation>();

		sa.Reset();
		sa.AnimationName = "heart_3_static";
		sa.timeScale = 0.5f;
		sa.loop = false;
	}
    #endregion

    #region Debug
    void ReloadItemDebug()
    {
        if (item_shape != null)
        {
            SpawnPosition();

            SkeletonAnimation shape = item_shape.GetComponent<SkeletonAnimation>();
            SkeletonAnimation zone = item_zone.GetComponent<SkeletonAnimation>();

            shape.Reset();
            zone.Reset();

            // Скейлы
            if (item_data_base[r].min_scale != 0 && item_data_base[r].max_scale != 0)
            {
                scale = Random.Range(item_data_base[r].min_scale, item_data_base[r].max_scale);
            }
            else if (item_data_base[r].min_scale == 0 && item_data_base[r].max_scale == 0)
            {
                scale = Random.Range(Global.min_scale, Global.max_scale);
            }

            spawn_scale = new Vector3(scale, scale, scale);

            item_shape.transform.localScale = spawn_scale;
            item_zone.transform.localScale = spawn_scale;

            item_shape.transform.position = spawn_position + new Vector3(0, 0, 5f);
            item_zone.transform.position = spawn_position;

            level_stage.transform.position = item_zone.transform.position;

            // Скорость проигрывания анимации
            if (item_data_base[r].min_speed != 0 && item_data_base[r].max_speed != 0)
            {
                temp_time_scale = item_data_base[r].min_speed * Global.Speed_Multiplier * Mathf.Pow(debug_step, debug_level);

                if (temp_time_scale >= (item_data_base[r].max_speed * Global.Speed_Multiplier))
                    temp_time_scale = item_data_base[r].max_speed * Global.Speed_Multiplier;
            }
            else if (item_data_base[r].min_speed == 0 && item_data_base[r].max_speed == 0)
            {
                temp_time_scale = Global.min_speed * Mathf.Pow(debug_step, debug_level);

                if (temp_time_scale >= Global.max_speed)
                    temp_time_scale = Global.max_speed;
            }

            shape.timeScale = temp_time_scale;
            zone.timeScale = temp_time_scale;

            item_status = item_Status.in_play;
            startTime = Time.time;
            journeyLength = Vector3.Distance(item_shape.transform.position, spawn_position);

            shape.loop = true;
            shape.state.Event += State_Event;

            zone.loop = true;

            int rotation = Random.Range(15, 360);

            shape.transform.eulerAngles = new Vector3(0, 0, rotation);
            zone.transform.eulerAngles = new Vector3(0, 0, rotation);

            if (GameMode == DebugMode.DebugMode)
            {
                VisualLog.Note("Name: " + item_shape.name.ToString());
                VisualLog.Note("Time Scale: " + temp_time_scale.ToString());
                VisualLog.Note("Spawn Scale: " + spawn_scale.ToString());
                VisualLog.Note("Position: " + (new Vector2(spawn_pixel_x, spawn_pixel_y)).ToString());
            }
        }
    }
    #endregion

}

