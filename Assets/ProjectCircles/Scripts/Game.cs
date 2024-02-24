using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.UI;

public class Game : MonoBehaviour {

    // Игровая камера
    public Camera gameСamera;

    // Префаб для создания кругов
    public GameObject empty_prefab;

    // Игровой экран внутри нашего Текущего Resolution
    private Rect internal_screen;

    // Вектор3 разницы юнита в пикселях
    private Vector3 diff;

    // Вектор3 разницы пикселей в юнитах
    private Vector3 dif;

    // Текущая позиция для спавна блоков
    private Vector3 spawn_position;

    // Коэффициент который отвечает за пределы экрана (текущий - 10% от сторон экрана)
    public float pixel_screen_coef = 0.1f;

    // Текущая скорость роста кружочка
    public float growing_time = 1.5f;

    // Текущая скорость спавна группы
    public float spawn_time = 3f;

    // Скорость загрузки следующего уровня
    public float nextlevel_time = 2f;

    // Время задержки на отклик по нажатию на экран для переходов между уровнями
    public float stop_spawn_time = 0.5f;

    // Временная скорость спавна группы и время ожидания
    private float temp_spawn_time;
    private float temp_stop_spawn_time;

    // Текущий MAX и MIN размеры диаметра BigCircle
    public float bc_max_diam = 5f;
    public float bc_min_diam = 2f;

    // Коеффициент диаметра в % от значения диаметра BigCircle
    public float bc_diam_coef = 20f;

    // Текущий MAX и MIN размеры диаметра SmallCircle ПОКА не влияет - ОБГОВОРИТЬ!!!
    public float sm_max_diam = 5f;
    public float sm_min_diam = 1f; 

    // Текущий Resolution который берётся от размера камеры в пикселях
    private float default_height;
    private float default_width;

    // Блок экрана на основе Текущего Resolution
    private float margin_x;
    private float margin_y;

    // Координаты точек на экране для спавна обьектов 
    public float spawn_pixel_x;
    public float spawn_pixel_y;

    // Рандомные диаметры кругов
    private float random_bc_diam;
    private float random_sm_diam;
    private float temp_gd_diam;

    // Текущий Блок из обьектов
    private GameObject growing_dot;
    private GameObject big_circle;
    private GameObject small_circle;

    List<GameObject> green_dots = new List<GameObject>();

    #region UI elements
    // Обьекты Интерфейса
    private GameObject ui_button_play;
    public GameObject game_stage_show;
    public GameObject game_level_show;
    public GameObject game_result_show;
    #endregion

    //Уровень игры
    int game_level;
    
    //Сколько осталось шагов до конца текущего уровня
    int game_stage;

    // Флажки игры
    bool start_stage;
    bool start_next;
    bool level_completed;
    bool cant_reload_stage;

    float sprite_size;

    // Лист всех ресурсов игровых спрайтов
    List<Sprite> sprite_collection = new List<Sprite>();

    // Use this for initialization
    void Start () {

        UnitToPixel();
        CalculateGameRectSize();
        InitGameResources();
        InitSettings();
        VisualLog.Initialize();
    }
	
	// Update is called once per frame
	void Update () {

        GameListener();
        InputHandler();
        VisualLog.Update();
    }

    #region ResolutionSettings
    void UnitToPixel()
    {
        Vector3 min_x = Vector3.zero;
        Vector3 max_x = new Vector3(1, 0, 0);

        Vector3 screen_min_x = gameСamera.WorldToScreenPoint(min_x);
        Vector3 screen_max_x = gameСamera.WorldToScreenPoint(max_x);

        diff = screen_max_x - screen_min_x;
        dif = gameСamera.ScreenToWorldPoint(diff);
    }

    void CalculateGameRectSize()
    {
        default_height = gameСamera.pixelHeight; // высота
        default_width = gameСamera.pixelWidth; // ширина

        margin_x = default_width * pixel_screen_coef; // 10% от сторон экрана
        margin_y = default_height * pixel_screen_coef;

        internal_screen = new Rect(margin_x * 2f, margin_y * 2f, default_width - (margin_x * 2f), default_height - (margin_y * 2f));
    }
    #endregion

    #region GameSettings
    public enum GameType
    {
        game1,
        game2,
        game3
    }

    GameType gameType = GameType.game1;

    public enum GameState
    {
        wait,
        start,
        stop
    }

    GameState gameState = GameState.wait;

    public Data data;

    void InitGameResources()
    {
        UnityEngine.Object[] sprites = Resources.LoadAll("Sprites/", typeof(Sprite));

        foreach (var item in sprites)
        {
            Sprite sp = (Sprite)item;
            sprite_collection.Add(sp);
        }
    }

    void InitSettings()
    {
        if (PlayerPrefs.GetInt("level") == 0)
        {
            game_level = 1;
            PlayerPrefs.SetInt("level",game_level);
            PlayerPrefs.Save();
        }
        else
            game_level = PlayerPrefs.GetInt("level");

        start_stage = false;
        start_next = false;
        level_completed = false;
        cant_reload_stage = false;


        game_stage = game_level;
        game_level_show.GetComponent<Text>().text = "Level: " + game_level.ToString();
        game_stage_show.GetComponent<Text>().enabled = false;
        game_result_show.GetComponent<Text>().enabled = false;

        ui_button_play = GameObject.Find("ButtonPlay");

        temp_stop_spawn_time = stop_spawn_time;
    }
    #endregion

    #region GameFunctional
    void SpawnBlock()
    {
        switch(gameType)
        {
            case GameType.game1:

                spawn_pixel_x = Convert.ToInt32(Random.Range(margin_x, default_width - margin_x));
                spawn_pixel_y = Convert.ToInt32(Random.Range(margin_y, default_height - margin_y));

                random_bc_diam = Random.Range(bc_min_diam,bc_max_diam);
                //random_sm_diam = Random.Range(sm_min_diam,sm_max_diam);

                spawn_position = gameСamera.ScreenToWorldPoint(new Vector3(spawn_pixel_x + diff.x, spawn_pixel_y + diff.y, 10));
                game_stage_show.transform.position = gameСamera.WorldToScreenPoint(spawn_position);

                Vector2 center_circle = new Vector2(spawn_pixel_x, spawn_pixel_y);
                float end_x = center_circle.x - (random_bc_diam / 2) * dif.x;
                float end_y = center_circle.y;
                Vector2 check_point = new Vector2(end_x, end_y);
                internal_screen.Contains(check_point);

                foreach (var item in sprite_collection)
                {
                    GameObject obj = Instantiate(empty_prefab) as GameObject;
                    //Sprite sp = new Sprite();
                    Sprite sp = Sprite.Create(item.texture, new Rect(0.0f, 0.0f, item.texture.width, item.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    obj.GetComponent<SpriteRenderer>().sprite = item;
                    sp = obj.GetComponent<SpriteRenderer>().sprite;
                    obj.name = item.name;
                    obj.transform.position = spawn_position;
                    sprite_size = obj.GetComponent<SpriteRenderer>().sprite.bounds.size.x;

                    Circle cl = new Circle();

                    cl.obj = obj;
                    cl.name = obj.name;
                    cl.current_x = spawn_pixel_x;
                    cl.current_y = spawn_pixel_y;

                    if (obj.name == "BigCircle")
                    {
                        cl.diameter = random_bc_diam;
                        obj.transform.localScale = new Vector3(random_bc_diam / sp.bounds.size.x, random_bc_diam / sp.bounds.size.y, random_bc_diam / sp.bounds.size.z);
                        cl.growind_speed = 0;
                        big_circle = obj;
                    }
                    else if (obj.name == "SmallCircle")
                    {
                        random_sm_diam = (random_bc_diam * bc_diam_coef);
                        cl.diameter = random_sm_diam;
                        obj.transform.localScale = new Vector3(random_sm_diam / sp.bounds.size.x, random_sm_diam / sp.bounds.size.y, random_sm_diam / sp.bounds.size.z);
                        cl.growind_speed = 0;
                        small_circle = obj;
                    }
                    else
                    {
                        temp_gd_diam = 1f;
                        cl.diameter = temp_gd_diam;
                        cl.growind_speed = growing_time;
                        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        growing_dot = obj;
                    }

                    data.list.Add(cl);
                }

                game_stage_show.GetComponent<Text>().enabled = true;

                break;
            case GameType.game2:
                //ПОКА не влияет - ОБГОВОРИТЬ!!!
                break;
            case GameType.game3:
                //ПОКА не влияет - ОБГОВОРИТЬ!!!
                break;
        }   
    }

    void GameListener()
    {
        if (start_stage)
        {
            growing_dot.transform.localScale *= 1 + Time.deltaTime * growing_time;
            temp_gd_diam = growing_dot.transform.localScale.x * sprite_size;

            if (temp_gd_diam >= random_bc_diam)
            {
                start_stage = false;
                level_completed = false;
                cant_reload_stage = true;
                growing_dot.transform.localScale = big_circle.transform.localScale;
                growing_dot.GetComponent<SpriteRenderer>().color = Color.red;
                game_result_show.GetComponent<Text>().enabled = true;
                game_result_show.GetComponent<Text>().text = "Game Over" + "\n" + "Tap To Restart";
                game_stage_show.GetComponent<Text>().enabled = false;
                ResignPosition();
                ReloadStage();
                ReloadLevel();
                gameState = GameState.stop;
                VisualLog.Warning("GameOver at: " + game_stage);
            }

            if (game_stage_show.GetComponent<Text>().enabled)
                game_stage_show.GetComponent<Text>().text = game_stage.ToString();

            game_level_show.GetComponent<Text>().text = "Level: " + game_level.ToString();
        }

        if (start_next)
        {
            game_result_show.GetComponent<Text>().enabled = false;

            temp_spawn_time -= Time.deltaTime;

            if (temp_spawn_time <= 0)
            {
                foreach (var item in green_dots)
                {
                    Destroy(item);
                }

                green_dots.Clear();

                ReloadStage();
                SpawnBlock();

                start_next = false;
                start_stage = true;
                gameState = GameState.start;
                VisualLog.Note("stepStartAt: " + game_stage);
            }
        }

        if (cant_reload_stage)
        {
            temp_stop_spawn_time -= Time.deltaTime;

            if (temp_stop_spawn_time <= 0)
            {
                temp_stop_spawn_time = stop_spawn_time;
                cant_reload_stage = false;
            }
        }
    }

    void InputHandler()
    {
        if (Input.GetMouseButtonDown(0) && start_stage && !start_next && gameState == GameState.start && !cant_reload_stage)
        {
            if (temp_gd_diam < random_sm_diam && temp_gd_diam < random_bc_diam)
            {
                growing_dot.GetComponent<SpriteRenderer>().color = Color.red;
                game_stage_show.GetComponent<Text>().enabled = false;
                game_result_show.GetComponent<Text>().enabled = true;
                game_result_show.GetComponent<Text>().text = "Game Over" + "\n" + "Tap To Restart";
                start_stage = false;
                level_completed = false;
                cant_reload_stage = true;
                ResignPosition();
                ReloadStage();
                ReloadLevel();
                gameState = GameState.stop;
                VisualLog.Warning("GameOver at: " + game_stage);
            }

            if (temp_gd_diam >= random_sm_diam && temp_gd_diam < random_bc_diam)
            {
                growing_dot.GetComponent<SpriteRenderer>().color = Color.green;
                green_dots.Add(growing_dot);
                ResignPosition();
                ReloadStage();
                start_stage = false;
                game_stage--;
                gameState = GameState.stop;
                VisualLog.Note("stepStopAt: " + game_stage);

                if (game_stage <= 0)
                {
                    start_next = false;
                    level_completed = true;
                    VisualLog.Warning("Level " + game_level + " is completed!");
                    game_result_show.GetComponent<Text>().enabled = true;
                    game_result_show.GetComponent<Text>().text = "Congrats!" + "\n" + "Level " + game_level + " is complete";
                    game_level++;
                    ReloadStage();
                    ReloadLevel();
                    PlayerPrefs.SetInt("level", game_level);
                    PlayerPrefs.Save();
                }
                else
                {
                    start_next = true;
                    level_completed = false;
                }
            }

            if (temp_gd_diam > random_sm_diam && temp_gd_diam >= random_bc_diam)
            {
                growing_dot.GetComponent<SpriteRenderer>().color = Color.red;
                game_stage_show.GetComponent<Text>().enabled = false;
                game_result_show.GetComponent<Text>().enabled = true;
                game_result_show.GetComponent<Text>().text = "Game Over" + "\n" + "Tap To Restart";
                start_stage = false;
                level_completed = false;
                cant_reload_stage = true;
                ResignPosition();
                ReloadStage();
                ReloadLevel();
                gameState = GameState.stop;
                VisualLog.Warning("GameOver at: " + game_stage);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !start_stage && !start_next && gameState == GameState.stop && !cant_reload_stage)
        {
            ReloadGameScreen();
            start_next = true;
            VisualLog.Note("MoveToNext: " + game_stage);
        }

        /// LIFE-HACK for tests
        if (Input.GetKeyDown(KeyCode.P))
        {
            ReloadLevel();
            SpawnBlock();
            start_next = false;
            start_stage = true;
        }
        ///
    }

    void ResignPosition()
    {
        growing_dot.transform.localPosition = new Vector3(growing_dot.transform.localPosition.x, growing_dot.transform.localPosition.y,15f);
    }

    void ReloadStage()
    {
        if (!level_completed)
            temp_spawn_time = spawn_time;
        else
            temp_spawn_time = nextlevel_time;

        temp_gd_diam = 0;
        random_bc_diam = 0;
        random_sm_diam = 0;

        Destroy(small_circle);
        Destroy(big_circle);

        big_circle = null;
        growing_dot = null;
        small_circle = null;

        data.list.Clear();
    }

    void ReloadLevel()
    {
        game_stage = game_level;
        level_completed = false;
    }

    void ReloadGameScreen()
    {
        foreach (var item in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (item.name == "GrowingDot")
                Destroy(item);
        }

        game_stage_show.GetComponent<Text>().enabled = false;
    }

    #endregion

    #region UI functions
    public void OnClickButtonPlay()
    {
        start_next = false;
        start_stage = true;

        SpawnBlock();

        ActivatePlayButton(false);

        gameState = GameState.start;
    }

    public void OnClickButtonReset()
    {
        PlayerPrefs.DeleteKey("level");
        PlayerPrefs.SetInt("level",1);
        PlayerPrefs.Save();
        ReloadStage();
        ReloadLevel();
        ReloadGameScreen();

        start_stage = false;
        start_next = false;

        game_level = 1;
        game_stage = game_level;

        game_level_show.GetComponent<Text>().text = "Level: " + game_level.ToString();
        game_stage_show.GetComponent<Text>().enabled = false;
        game_result_show.GetComponent<Text>().enabled = false;

        gameState = GameState.stop;

        ActivatePlayButton(true);

        VisualLog.Warning("Reset Game!");
    }

    void ActivatePlayButton(bool active)
    {
        if (active)
            ui_button_play.SetActive(true);
        else
            ui_button_play.SetActive(false);
    }

    public void OnGUI()
    {
        VisualLog.Visualize();
    }
    #endregion
}

#region Object Information
[Serializable]
public class Circle
{
    public string name;
    public GameObject obj;
    public float diameter;
    public float current_x;
    public float current_y;
    public float growind_speed;
}

[Serializable]
public class Data
{
    public List<Circle> list = new List<Circle>();
    public Circle[] container;
}
#endregion
