class MGNunchiDefines
{
    public const string PLAT_NAME = "minigame_nunchigame_jump";

    public const int COIN_ITEM_PLACE_LAYER = 1;
    public const int PLAYERS_PLACE_LAYER = 0;

    #region COIN_CONTROL
    public const string COIN_PREFAB_NAME = "position_";
    public const string COIN_UNDER_PREFAB_NAME = "minigame_nunchigame_coin";
    public const string COIN_ANIM_Name_SPAWN = "minigame_acting_spawn";
    public const string COIN_ANIM_Name_UPNDOWN = "minigame_acting_upndown"; //Loop
    public const string COIN_ANIM_Name_DISAPPEAR = "minigame_acting_disappear";

    public const int COIN_ROTATION_NORMAL_SPEED = 200;
    public const int COIN_ROTATION_FAST_SPEED = 500;

    public const int MAX_COIN = 10;
    #endregion COIN_CONTROL

    #region ANIM_NAME
    public const string ANIM_IDLE = "idle";
    public const string ANIM_JUMP = "jump_nunchigame";
	public const string ANIM_JUMP_START = "jump_start";//0.2f, 0.033f
	public const string ANIM_JUMP_FINISH = "jump_end";
    public const string ANIM_VICTORY_TURN = "game_victory_tern";
    public const string ANIM_VICTORY = "game_victory";
    public const string ANIM_VICTORY_IDLE = "game_victory_idle";
    public const string ANIM_FAIL = "game_failure";
    public const string ANIM_FAIL_IDLE = "game_failure_idle";
    public const string ANIM_KNOCKDOWN = "game_knockdown";
    public const string ANIM_KNOCKDOWN_STAND = "game_knockdown_stand";
    public const string ANIM_JUMP_CLIP_NAME = "animation_m03_jump_nunchigame";

    public const string ANIM_SCORE_GAIN_COIN = "fu_minigame_fx_9";

    public const string ANIM_SCORE_GAIN_ITEM = "fu_minigame_fx_2";
    //public const string ANIM_PLAT_CAN_SELECT = "minigame_nunchigame_jump_flicker";//선택가능
    public const string ANIM_PLAT_TOUCH_DOWN = "minigame_nunchigame_jump_touch";//터치
    //public const string ANIM_PLAT_SELECT_COMPLETE = "minigame_nunchigame_jump_select";//선택완료
    public const string ANIM_PLAT_DOWN = "minigame_nunchigame_jump_landing";//착지
    //public const string ANIM_PLAT_IDLE = "minigame_nunchigame_jump_idle";

    public const string ANIM_UI_TIMERCOUNT = "fu_minigame01_count01";
    public const string ANIM_UI_START = "fu_minigame_start_img";
    #endregion ANIM_NAME

    public const string PLAT_EFF_MATERIAL_NAME = "minigame_nunchigame_jump_state";
    public const string PLAT_SHADERPROP_EMISSION_NAME = "_EmissionColor";
    public const float PLAT_SHADERPROP_SELECTABLE_ALPHAVALUE_MAX = 0.45f;

    public const string BPW_MINIGAME_NUNCHI_TIMESTREAMKEY = "nunchiGameStreamKey";
    public const string BPW_MINIGAME_NUNCHI_RESULT_TIMESTREAMKEY = "nunchiGameResultStreamKey";

    public const int MAX_ROUND = 10;
    public const int DIRECTION_BUTTON_LENGTH = 4;
    public const float SCOREUI_HEIGHT = 1.5f;

    public const int PLAYERS_3P = 3;
    public const int PLAYERS_4P = 4;
    public const int DIRECTIONCOUNT = 4;

    public const int MAP_SIZE_4P_WIDTH = 3;
    public const int MAP_SIZE_4P_HEIGHT = 3;
    public const int MAP_CENTER_IDX = 4;

    public const string SCORE_GAIN_SIGN = "+";
    public const string SCORE_LOSE_SIGN = "-";
    public const float SCORE_UI_ADD_HEIGHT = 0.5f;

    public const int FINALIST_RANK = 2;
    public const int FINAL_WAIT_TIME = 6;

    public const float ROTATION_TIME = 0.2f;
    public const float RETURN_TIME = 2;
    public const float BASEWAIT_TIME = 2;

	public const float JUMP_START_TIME = 0.2f;
	public const float JUMP_START_DELAY_TIME = 0.1f;
    public const float JUMP_DUR_TIME = 0.5f;//0.533f;//
    public const float JUMP_GETCOIN_TIME = 0.33f;
	public const float JUMP_HEIGHT = 0.7f;
    public const float KNOCKDOWN_TIME = 1.13f;
	
    public const float VICTORY_TIME = 2.13f;
}

static class MGNunchiConstants
{
    public const string PLAT_PATH = "Views/Minigame/Nunchi/Open/Prefabs/minigame_nunchigame_jump";
    public const string COIN_PATH = "Views/Minigame/Nunchi/Open/Prefabs/coin_";
    public const string ITEM_DOUBLE_PATH = "Views/Minigame/Nunchi/Open/Prefabs/minigame_nunchigame_item_coindouble.prefab";
    public const string ITEM_INVINCIBLE_PATH = "Views/Minigame/Nunchi/Open/Prefabs/minigame_nunchigame_item_invincible.prefab";
    public const string ITEM_STEAL_PATH = "Views/Minigame/Nunchi/Open/Prefabs/minigame_nunchigame_item_steal.prefab";
    public const string COINITEM_GAINUI_PATH = "UI/Minigame/Nunchi/prefabs/obj_minigame_item_get.prefab";
                                               

    public const string COINITEM_FX_GAIN_PATH = "FX/Open/prefab/fx_minigame01_coin_disappear.prefab";
    public const string COINITEM_FX_DUST_PATH = "FX/Open/prefab/fx_minigame01_dust.prefab";
    public const string COINITEM_FX_HIT_PATH = "FX/Open/prefab/fx_minigame01_hit.prefab";

    public const string SCORE_GAINITEM_FX_PATH = "FX/Open/prefab/fu_minigame_fx_2.prefab";
    public const string SCORE_GAIN_FX_PATH = "FX/Open/prefab/fu_minigame_fx_9.prefab";
    public const string SCORE_LOSE_FX_PATH = "FX/Open/prefab/fu_minigame_fx_10.prefab";
    public const string CONFETTI_FX_PATH = "FX/Open/prefab/fu_minigame_nunchi_confetti.prefab";

    public const string SCORE_ITEM_ICON_PATH = "UI/Common/Images/packer_icon.png#{0}";
    public const string SCORE_ITEM_NAME_COIN = "icon_minigame_coin";//"gold_icon";

    public const string PLAT_PUDDING_OBJ_PATH = "minigame_nunchigame_jump/Pudding";
    public const string PLAT_ORIGIN_MATERIAL_PATH = "Views/Minigame/Nunchi/Open/Materials/minigame_nunchigame_jump_state.mat";

    public const string MY_PLAYER_INDICATOR_PATH = "Views/Minigame/Nunchi/Open/Prefabs/minigame_indicator.prefab";

    /*
     * defaultstringtable 에 들어감
    public const string SCORE_ITEM_NAME_INVINCIBLE = "icon_minigame_invincibility";
    public const string SCORE_ITEM_NAME_STEAL = "icon_minigame_steal";
    public const string SCORE_ITEM_NAME_DOUBLE = "icon_minigame_x2";
    */


}


//MiniGame Nunchi
public enum DIRECTION
{
    NONE = -1,
    UP = 0,
    DOWN,
    LEFT,
    RIGHT
}

public enum PLAT_STATE
{
    DEFAULT = 0,
    TOUCHED,
    SELECTED,
    SELECTABLE
}