using Unity.Collections;

public class CMarbleDefine
{
    public const int IS_FROM_NAVIGATION = 1;
    
    public const float ACTIVE_CAM_DEFAULT_FOV = 30;
    public const float MARBLE_TARGET_DIST = 300f;
    public const float MARBLE_DICE_ROLLING_TIME = 1.0f;

    //Avatar
    public const int AVATAR_SCALE = 8;
    public const float AVATAR_SPAWN_HEIGHT = 5.5f;
    public const int PLAYER_ROTATION_ANGLE = 90;
    public const string ANIM_NAME_PLAYER_STAND = "stand_0";
    public const string ANIM_NAME_PLAYER_STAND_VAR = "stand_{0}";
    public const string ANIM_NAME_PLAYER_ARRIVE = "arrive";
    public const string ANIM_NAME_PLAYER_MOVE_READY = "move_ready";
    public const string ANIM_NAME_PLAYER_MOVE = "move";
    public const string ANIM_NAME_PLAYER_MOVE_FINISH = "move_finish";
    public const string ANIM_NAME_PLAYER_SMOVE_READY = "s_move_ready";
    public const string ANIM_NAME_PLAYER_SMOVE = "s_move";
    public const string ANIM_NAME_PLAYER_SMOVE_FINISH = "s_move_finish";
    public const string ANIM_NAME_PLAYER_INTERACTION = "interaction_{0}";
    

    public const string EFF_NAME_PLAYER_MOVE = "fx_dice_tile_move";
    public const int AVATAR_STAND_CLIP_COUNT = 3;

    // avatar animclip name
    public const string ANIM_ARRIVESTATE_NAME_REWARD = "reward";
    public const string ANIM_ARRIVESTATE_NAME_MOVE = "move";
    public const string ANIM_ARRIVESTATE_NAME_BUFF = "buff";
    public const string ANIM_ARRIVESTATE_NAME_DEBUFF = "debuff";
    
    public const string ANIM_PARAM_NAME_REWARD = "arrive_reward";
    public const string ANIM_PARAM_NAME_MOVE =   "arrive_move";
    public const string ANIM_PARAM_NAME_BUFF =   "arrive_buff";
    public const string ANIM_PARAM_NAME_DEBUFF = "arrive_debuff";
    

    public const string ANIMCLIP_NAME_ARRIVE_REWARD = "touch_high";//"ch_ani_touch_high";
    public const string ANIMCLIP_NAME_ARRIVE_MOVE = "ch_ani_dice_arrive_move_01";
    public const string ANIMCLIP_NAME_ARRIVE_BUFF = "ch_ani_dice_arrive_buff_01";
    public const string ANIMCLIP_NAME_ARRIVE_DEBUFF = "ch_ani_idle_sigh";

    public const string ANIMCLIP_NAME_TOUCH_01 = "ch_ani_touch_mid";
    public const string ANIMCLIP_NAME_TOUCH_02 = "ch_ani_gacha_shortgreet";



    //Block
    public const float APEX_SIZE = 10.0f;
    public const float BLOCK_SIZE = 8.0f;
    public const int BLOCK_COUNT_PER_SIDE = 9;
    public const int BLOCK_AREA_1 = 1;
    public const int BLOCK_AREA_2 = 2;
    public const int BLOCK_AREA_3 = 3;
    public const int BLOCK_AREA_4 = 4;
    public const string BASE_BLOCK_PREFAB_NAME = "dice_block";
    public const string BASE_APEX_PREFAB_NAME = "dice_apex";
    public const string TAG_APEX_BLOCK = "ApexTile";
    public const string TAG_NORMAL_BLOCK = "BlockTile";
    public const int BLOCK_MAX_COUNT = 40;
    public const int BLOCK_ROTATION_ANGLE = 360;
    public const int BLOCK_AREA_1_IDX_START = 1;
    public const int BLOCK_AREA_1_IDX_END = 11;
    public const int BLOCK_AREA_2_IDX_START = 12;
    public const int BLOCK_AREA_2_IDX_END = 21;
    public const int BLOCK_AREA_3_IDX_START = 22;
    public const int BLOCK_AREA_3_IDX_END = 31;
    public const int BLOCK_AREA_4_IDX_START = 32;
    public const int BLOCK_AREA_4_IDX_END = 40;
    public const int BLOCK_AREA_AXIS_X_DIST = 45;
    public const float BLOCK_LIMIT_RANGE_TYPE_1_MIN = 17.0f;
    public const float BLOCK_LIMIT_RANGE_TYPE_1_MAX = 25.0f; 
    public const float BLOCK_LIMIT_RANGE_TYPE_2_MIN = 55.0f;
    public const float BLOCK_LIMIT_RANGE_TYPE_2_MAX = 65.0f;
    public const float BLOCK_LIMIT_RANGE_TYPE_3_MAX = 69.0f;
    public const int BLOCK_APX_INDEX_1 = 1;
    public const int BLOCK_APX_INDEX_2 = 11;
    public const int BLOCK_APX_INDEX_3 = 21;
    public const int BLOCK_APX_INDEX_4 = 31;
    public const int BLOCK_APX_INDEX_5 = 40;
    public const int BLOCK_CHECK_RAY_HEIGHT = 5;
    public const int BLOCK_CHECK_RAY_DISTANCE = 20;
    public const string ANIM_NAME_BLOCK_RESET = "reset";
    public const string ANIM_NAME_BLOCK_WORK = "work";
    public const float BLOCK_CHANGE_TIME = 0.3f;
    public const float BLOCK_CHANGE_HALFTIME = 0.15f;
    public const float BLOCK_BUFF_FLYING_TIME = 1.5f;
    
    //block Eff
    //public const string PREFAB_NAME_REWARD = "";
    public const string EFFECT_NAME_START_TILE = "fx_dice_tile_01_unit";
    public const string EFFECT_NAME_REWARD = "fx_dice_tile_reward_unit";
    public const string EFFECT_NAME_MOVE_TILE = "fx_dice_tile_movetile";

    public const string BUFF_EFFECT_NAME = "fx_dice_tile_buff";//player arrive effect
    public const string BUFF_APPLY_EFFECT_NAME = "fx_dice_tile_buff_apply";//Set on buff effect on target block
    public const string BUFF_MOVE_EFFECT_NAME = "fx_dice_tile_buff_move";//fly to target block
    public const string BUFF_MOVE_HIT_EFFECT_NAME = "fx_dice_tile_buff_move_hit"; //hit effect after flying

    public const string DEBUFF_EFFECT_NAME = "fx_dice_tile_debuff";
    public const string DEBUFF_APPLY_EFFECT_NAME = "fx_dice_tile_debuff_apply";
    public const string DEBUFF_MOVE_EFFECT_NAME = "fx_dice_tile_debuff_move";
    public const string DEBUFF_MOVE_HIT_EFFECT_NAME = "fx_dice_tile_debuff_move_hit"; //hit effect after flying

    //Dice
    public const string ANIM_NAME_DICE_RESET = "reset";
    public const string ANIM_NAME_DICE_WORK = "work";


    public const string Layer_BLOCK = "BlockTile";

}

public enum MARBLE_GAME_STATE
{
    ENTRY = 0,
    IDLE,
    PLAY,
    CAM_ACTION_RESTORE,
    FINISH,
}

public enum MARBLE_BLOCK_STATE
{
    IDLE = 0,
    PLAYER_ARRIVE,
    TO_NONE,
    RENEWAL,
    ACTIVE,
    INACTIVE,
    TOUCHABLE,
    REWARD,
    BUFF,
    DEBUFF,
}

public enum MARBLE_BLOCK_TYPE
{
    NONE = 0,
    START,
    REWARD,
    MOVE,
    REWARD_BUFF,
    REWARD_DEBUFF,
}

public enum MARBLE_BLOCK_SHAPE_TYPE
{
    NORMAL = 0,
    APEX
}

// public enum MARBLE_BUFF_TYPE
// {
//     NONE = 0,
//     BUFF,
//     DEBUFF,
// }

public enum ANIM_CONTROLLER_TYPE
{
    SNG = 0,
    MARBLE,
}

public enum DICE_STATE
{
    READY = 0,
    IDLE,
    READYTO,
    WORK,
    WORKFINISH,
}

public enum BLOCK_AREA_TYPE
{
    NONE = 0,
    AREA_1,
    AREA_2,
    AREA_3,
    AREA_4,
}

public enum MARBLE_PLAYER_STATE
{
    IDLE = 0,
    PLAY,
    WAIT,
    FINISH,
    ARRIVE,
    TOUCHED,
}

public enum MARBLE_IMPROVE_SIMUL_REJECT_TYPE
{
    NONE = 0,
    DATA_NULL,
    NOT_ENOUGH_POINT,
    NOT_ENOUGH_MEMBER_LV,
    MAX_LEVEL,
}