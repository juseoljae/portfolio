using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class HeroMedalLvUpFactorData
{
    #region VARIABLES

    private static int m_SummonGradeMax = 5;

    private static List<float> Pet_Norm = new List<float>();
    public  static List<float> PetNorms { get { return Pet_Norm; } }

    private static List<float> Pet_Quot = new List<float>();
    public static List<float> PetQuots { get { return Pet_Quot; } } 

    public static float Pet_Medal_Start { get; private set; }
    public static float Pet_Medal_Norm { get; private set; } 
    public static float Pet_Medal_Quot { get; private set; } 

    public static float Pet_Medal_Hp_Start
    {
        get; private set;
    } 
    public static float Pet_Medal_Hp_Norm
    {
        get; private set;
    } 
    public static float Pet_Medal_Hp_Quot
    {
        get; private set;
    }

    public static float Pet_Enchant_Circle
    {
        get; private set;
    }
    public static float Pet_Enchant_Coefficent
    {
        get; private set;
    }


    public static float Pet_Medal_Quot_Start
    {
        get; private set;
    }
    public static float Pet_Medal_Quot_Norm
    {
        get; private set;
    }
    public static float Pet_Medal_Quot_Level
    {
        get; private set;
    }


    public static float Pet_Gold_Start { get; private set; } 
    public static float Pet_Gold_Norm { get; private set; }
    public static float Pet_Gold_Quot { get; private set; }

    public static float Pet_Exp_Norm { get; private set; }
    public static float Pet_Exp_Quot { get; private set; }

    
    public static float Pet_UpStone_Norm { get; private set; } 
	public static float Pet_UpStone_Start { get; private set; }

    public static float Pet_OneStar_Ratio
    {
        get; private set;
    }
    public static float Pet_TwoStar_Ratio
    {
        get; private set;
    }
    public static float Pet_ThreeStar_Ratio
    {
        get; private set;
    }
    public static float Pet_FourStar_Ratio
    {
        get; private set;
    }
    public static float Pet_FiveStar_Ratio
    {
        get; private set;
    }

    #endregion VARIABLES



    #region CONSTRUCTOR

    static HeroMedalLvUpFactorData()
    {
        CExcelData_NPC_FACTOR_DATA.instance.Create();
        CExcelData_NPC_FACTOR_DATA.instance.MEMORYTEXT_Create();

        Pet_Medal_Start = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_ATTDEF_START(0);
        Pet_Medal_Norm = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_ATTDEF_NORM(0);
        Pet_Medal_Quot = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_ATTDEF_QUOT(0);

        Pet_Medal_Hp_Start = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_HP_START(0);
        Pet_Medal_Hp_Norm = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_HP_NORM(0);
        Pet_Medal_Hp_Quot = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_HP_QUOT(0);

        Pet_Enchant_Circle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_CIRCLE(0);
        Pet_Enchant_Coefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ENCHANT_COEFFICENT(0);

        Pet_Medal_Quot_Start = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_MEDAL_QUOT_START(0);
        Pet_Medal_Quot_Norm = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_MEDAL_QUOT_NORM(0);
        Pet_Medal_Quot_Level = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_MEDAL_QUOT_LEVEL(0);

        Pet_Gold_Start = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_GOLD_START(0);
        Pet_Gold_Norm		= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_GOLD_NORM(0);
        Pet_Gold_Quot		= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_GOLD_QUOT(0);

        Pet_Exp_Norm		= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_EXP_NORM(0);
        Pet_Exp_Quot		= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_EXP_QUOT(0);

        Pet_UpStone_Norm	= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetUPGRADE_STONE_NORM(0);
		Pet_UpStone_Start	= CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetUPGRADE_STONE_START(0);

        Pet_OneStar_Ratio = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_ONESTAR_RATIO(0);
        Pet_TwoStar_Ratio = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_TWOSTAR_RATIO(0);
        Pet_ThreeStar_Ratio = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_THREESTAR_RATIO(0);
        Pet_FourStar_Ratio = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_FOURSTAR_RATIO(0);
        Pet_FiveStar_Ratio = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_FIVESTAR_RATIO(0);
    }

    #endregion CONSTRUCTOR



    #region PUBLIC_FUNC
    public static double GetEnchantGoldPrice(int _UpLavel)
    {
        return Pet_Gold_Start + (Pet_Gold_Norm * (Mathf.Round(UtilManager.instance.CalcPow((_UpLavel - 1), Pet_Gold_Quot))));
    }
 
    public static int GetEnchantUpStoneCount(int _UpLavel)
    {
        return 1 + (int)((_UpLavel - 1) / Pet_UpStone_Norm);
    }

    public static long GetMedalLvupPrice(int _Grade, uint _lv)
    {
        //((표준 강화비용 * 레벨 ^ 강화비용 지수)) 
        return (long)(Pet_Norm[_Grade - 1] * Mathf.Round(UtilManager.instance.CalcPow((_lv + 1), Pet_Quot[_Grade - 1])));
    }

    public static double GetEnchantUpAbilitykRatio(float _lv, bool NextEnchant = false)
    {
        if (_lv <= 0)
            return 0;
     
        float lPet_Medal_Quot = Pet_Medal_Quot;
        float enchatRatio = (Pet_Medal_Start + Pet_Medal_Norm * (UtilManager.instance.CalcPow((_lv - 1), lPet_Medal_Quot)));
        return enchatRatio;

    }
    public static float GetEnchantUpAbilityAttack(float NowValue, int _lv, int grade, bool NextEnchant = false)
    {
        if (_lv < 1)
            return NowValue;

        float lPet_Medal_Norm = Pet_Medal_Norm;
    
       switch (grade)
        {
            case 1:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_OneStar_Ratio;
                break;
            case 2:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_TwoStar_Ratio;
                break;
            case 3:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_ThreeStar_Ratio;
                break;
            case 4:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_FourStar_Ratio;
                break;
            default:
                break;
        }

        float lPet_Medal_Quot = Pet_Medal_Quot;
        float l_fCoefficent = 1;
        if (_lv > Pet_Enchant_Circle)
        {
            l_fCoefficent = Pet_Enchant_Coefficent * (int)(_lv / Pet_Enchant_Circle);
        }
        float AttackValue = (Pet_Medal_Start + lPet_Medal_Norm * (UtilManager.instance.CalcPow((_lv), lPet_Medal_Quot))) * (l_fCoefficent);

        float EnchantAttackValue = NowValue + AttackValue;
        return EnchantAttackValue;
    }

    public static float GetEnchantUpAbilityDefense(float NowValue, int _lv, int grade, bool NextEnchant = false)
    {
        if (_lv < 1)
            return NowValue;

        float lPet_Medal_Norm = Pet_Medal_Norm;
  
        switch (grade)
        {
            case 1:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_OneStar_Ratio;
                break;
            case 2:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_TwoStar_Ratio;
                break;
            case 3:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_ThreeStar_Ratio;
                break;
            case 4:
                lPet_Medal_Norm = Pet_Medal_Norm * Pet_FourStar_Ratio;
                break;
            default:
                break;
        }

        float lPet_Medal_Quot = Pet_Medal_Quot;
        float l_fCoefficent = 1;
        if (_lv > Pet_Enchant_Circle)
        {
            l_fCoefficent = Pet_Enchant_Coefficent * (int)(_lv / Pet_Enchant_Circle);
        }
        float DefenseValue = (Pet_Medal_Start + lPet_Medal_Norm * (UtilManager.instance.CalcPow((_lv), lPet_Medal_Quot))) * (l_fCoefficent);

        float EnchantDefenseValue = NowValue + DefenseValue;
        return EnchantDefenseValue;
    }

    public static float GetEnchantUpAbilityHP(float NowValue, int _lv, int grade, bool NextEnchant = false)
    {
        if (_lv < 1)
            return NowValue;

        float lPet_Medal_Norm = Pet_Medal_Hp_Norm;
 
        switch (grade)
        {
            case 1:
                lPet_Medal_Norm = Pet_Medal_Hp_Norm * Pet_OneStar_Ratio;
                break;
            case 2:
                lPet_Medal_Norm = Pet_Medal_Hp_Norm * Pet_TwoStar_Ratio;
                break;
            case 3:
                lPet_Medal_Norm = Pet_Medal_Hp_Norm * Pet_ThreeStar_Ratio;
                break;
            case 4:
                lPet_Medal_Norm = Pet_Medal_Hp_Norm * Pet_FourStar_Ratio;
                break;
            default:
                break;
        }
        float lPet_Medal_Quot = Pet_Medal_Hp_Quot;
        float l_fCoefficent = 1;
        if (_lv > Pet_Enchant_Circle)
        {
            l_fCoefficent = Pet_Enchant_Coefficent * (int)(_lv / Pet_Enchant_Circle);
        }
        float HpValue = (Pet_Medal_Hp_Start + lPet_Medal_Norm * (UtilManager.instance.CalcPow((_lv), lPet_Medal_Quot))) * (l_fCoefficent);

        float EnchantHpValue = NowValue + HpValue;
        return EnchantHpValue;
    }

    public static float GetEnchantRequireGoldPrice(int _lv)
    {
        // 비용 시작값 + 표준값 * (레벨) ^ 지수
        return (float)(Pet_Gold_Start +((Pet_Gold_Norm * Mathf.Round(UtilManager.instance.CalcPow(_lv, Pet_Gold_Quot)))));
    }

    public static float GetEnchantRequireStonePrice(int _lv)
    {
        // 비용 시작값 + 표준값 * (레벨) ^ 지수
        return (float)Pet_UpStone_Start + (int)((_lv / Pet_UpStone_Norm));
    }


    public static float GetSummonMaxExp(int level)
    {
        int levelPer50 = (int)((level - 1) / 50);
        int levelExtra50 = (int)((level - 1) % 50) + 1;

        float Pet_Exp_AddQuot = Pet_Exp_Quot;

        if (level > 1600)
        {
            Pet_Exp_AddQuot = Pet_Exp_AddQuot + 0.025f * (long)( (level - 1600) / 50);
        }
        
        float pow = 0;
        pow = Mathf.Round(UtilManager.instance.CalcPow(levelPer50, Pet_Exp_AddQuot));

		float nextExp = PlayerManager.instance.m_playerLevelupData[levelExtra50].m_nNextExp;
        
        return Mathf.Floor(Pet_Exp_Norm * pow + nextExp);
    }
#endregion PUBLIC_FUNC
}
