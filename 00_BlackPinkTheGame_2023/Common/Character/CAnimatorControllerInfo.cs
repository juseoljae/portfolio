using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationControllerRawData
{
    public long ID { get; set; }
    public long GroupID { get; set; }

    public ANIMCONTROLLER_TYPE ControllerType { get; set; }
    public ANIMCONTROLLER_USE_TYPE Type { get; set; }
    public string ResPath { get; set; }
}

public class CAnimationControllerInfo
{ 
    public long ID { get; private set; }
    public long GroupID { get; private set; }
    public ANIMCONTROLLER_TYPE ControllerType { get; set; }
    public ANIMCONTROLLER_USE_TYPE Type { get; private set; }
    public string ResPath { get; private set; }

    public CAnimationControllerInfo(CAnimationControllerRawData rowData)
    {
        this.ID = rowData.ID;
        this.GroupID = rowData.GroupID;
        this.ControllerType = rowData.ControllerType;
        this.Type = rowData.Type;
        this.ResPath= rowData.ResPath;
    }
}
