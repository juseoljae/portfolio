using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharAnimControllerDataManager : Singleton<CCharAnimControllerDataManager>
{
    private Dictionary<long, List<CAnimationControllerInfo>> CharAnimControllerDic = new Dictionary<long, List<CAnimationControllerInfo>>();

    public static void LoadCharAnimControllerData()
    {
        Instance._loadCharAnimControllerData();
    }

    private void _loadCharAnimControllerData()
    {
        DataTable table = CDataManager.GetTable( ETableDefine.TABLE_ANIMATORCONTROLLER );
        CAnimationControllerRawData _rawData = null;
        CAnimationControllerInfo _cntlrData = null;


        if (null == table)
        {
            CDebug.LogError( string.Format( "Not Found Table : [{0}]", ETableDefine.TABLE_ANIMATORCONTROLLER ) );
            return;
        }

        if (CharAnimControllerDic != null)
        {
            CharAnimControllerDic.Clear();
        }

        else
        {
            CharAnimControllerDic = new Dictionary<long, List<CAnimationControllerInfo>>();
        }

        for (int index = 0; index < table.RowCount; ++index)
        {
            _rawData = new CAnimationControllerRawData();
            _rawData.ID = table.GetValue<long>("ID", index);
            _rawData.GroupID = table.GetValue<long>( "Group_ID", index );
            _rawData.ControllerType = (ANIMCONTROLLER_TYPE)table.GetValue<byte>( "Controller_Type", index );
            _rawData.Type = (ANIMCONTROLLER_USE_TYPE)table.GetValue<byte>("Type", index);
            _rawData.ResPath = table.GetValue<string>( "Controller", index );

            _cntlrData = new CAnimationControllerInfo( _rawData );

            if (CharAnimControllerDic.ContainsKey(_rawData.GroupID) == false)
            {
                CharAnimControllerDic.Add( _rawData.GroupID, new List<CAnimationControllerInfo>() );
                CharAnimControllerDic[_rawData.GroupID].Add( _cntlrData );
            }
            else
            {
                CharAnimControllerDic[_rawData.GroupID].Add( _cntlrData );
            }
        }
    }

    public CAnimationControllerInfo GetAnimControllerInfoByType(long grpID, ANIMCONTROLLER_USE_TYPE type)
    {
        CAnimationControllerInfo _info = null;

        if (CharAnimControllerDic.ContainsKey( grpID ))
        {
            CharAnimControllerDic[grpID].ForEach( t =>
            {
                if (t.Type == type)
                {
                    _info = t;
                }
            } );
        }

        return _info;
    }

    public string GetAnimatorControllerResPath(long grpID, long ID)
    {
        CAnimationControllerInfo _info = null;

        if (CharAnimControllerDic.ContainsKey( grpID ))
        {
            CharAnimControllerDic[grpID].ForEach( t =>
            {
                if (t.ID == ID)
                {
                    _info = t;
                }
            } );
        }

        return _info.ResPath;
    }
}
