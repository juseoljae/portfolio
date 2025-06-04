
public class MGPihagiDefine
{
    public const byte PLAYER_COUNT = 4;
    public const byte PLAYER_MAX_LIFE_COUNT = 5;

    public const float ENEMY_SPAWN_YPOS = 0.5f;
    public const float SPAWN_PREHEIGHT = 1.8f;
    public const float SPAWN_HEIGHT = 0.6f;    
    public const float GROUND_HEIGHT = 0;

    public const float ENEMY_STAY_LEFTPOS = 0.6f;
    public const float ENEMY_STAY_RIGHTPOS = 8.77f;
    public const float ENEMY_START_POS = 2.285f;
    public const float ENEMY_POS_GAP = 1.2f;

    public const float SPAWN_READY_HEIGHT = 0.535f;


    public const float REQUEST_MOVESEQ = 0.1f;


    //cordinate by dir
    public const float JUMPDOWN_LINE_POS_LEFT = 3.6f;
    public const float JUMPDOWN_LINE_POS_RIGHT = 10.6f;
    public const float JUMPDOWN_LINE_POS_TOP = 10.6f;
    public const float JUMPDOWN_LINE_POS_DOWN = 3.6f;

    public const float JUMP_LINE_GAP = 0.6f;
    public const float CHECK_JUMP_DIST = 0.05f;

    public const float JUMPUP_LINE_POS_LEFT = JUMPDOWN_LINE_POS_LEFT + JUMP_LINE_GAP;
    public const float JUMPUP_LINE_POS_RIGHT = JUMPDOWN_LINE_POS_RIGHT - JUMP_LINE_GAP;
    public const float JUMPUP_LINE_POS_TOP = JUMPDOWN_LINE_POS_TOP - JUMP_LINE_GAP;
    public const float JUMPUP_LINE_POS_DOWN = JUMPDOWN_LINE_POS_DOWN + JUMP_LINE_GAP;

    public const float JUMP_STEP_PERFRAME = 0.1f;

    public const int COUNTDOWN_MAX = 3 + 1;
    public const string ANIM_UI_START = "fu_minigame_start_img";

    public const int FINALIST_RANK = 2;
    public const int FINAL_WAIT_TIME = 3;//5;

    public const float DEATH_AREA_LEFT = 6.5f;
    public const float DEATH_AREA_RIGHT = 7.7f;

    public const byte GROUND_SPAWN_COUNT = 5;
    public const byte GROUND_DIR_LEFT_STARTIDX = 1;
    public const byte GROUND_DIR_TOP_STARTIDX = 6;
    public const byte GROUND_DIR_RIGHT_STARTIDX = 11;

    public const string BPW_MINIGAME_PIHAGI_TIMESTREAMKEY = "pihagiGameStreamKey";
    public const string BPW_MINIGAME_PIHAGI_RESULT_TIMESTREAMKEY = "pihagiGameResultStreamKey";
}

public class MGPihagiConstants
{
    public const string FX_GROUND_PATH = "FX/Open/prefab/Minigame_fx_15.prefab";
    public const string FX_ENEMYDASH_PATH = "FX/Open/prefab/Minigame_fx_14.prefab";
    public const string FX_STUN_PATH = "FX/Open/prefab/Minigame_fx_17.prefab";
    public const string FX_INVINCIBLE_PATH = "FX/Open/prefab/Minigame_fx_18.prefab";
    public const string CONFETTI_FX_PATH = "FX/Open/prefab/fu_minigame_nunchi_confetti.prefab";
}


public enum ENEMY_TYPE
{
    NONE = 0,
    TAKE_HEART = 1,
}

public enum ENEMY_POSDIR
{
    NONE = 0,
    LEFT,
    TOP,
    RIGHT,
}

public enum ENEMY_JUNPSTATE
{
    POS_UP = 0,
    JUMPDOWN,
    JUMPUP,
    POS_DOWN,
}