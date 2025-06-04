using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPWorldSlotInfo
{

	public class EmoticonData
	{
		public long motionId;
		public long timeStamp;

		public EmoticonData(BPWPacketDefine.EmoticonInfo? emoticonInfo)
		{
			motionId = (long)emoticonInfo.Value.Tid;
			timeStamp = (long)emoticonInfo.Value.PurchaseTimestamp;
		}
	}

	public class EmoticonSlotInfo
	{
		public int slotIndex;
		public long emoticonId;
		public EmoticonSlotInfo()
		{

		}
		public EmoticonSlotInfo(BPWPacketDefine.EmoticonSlotInfo? emoticonSlotInfo)
		{
			slotIndex = (int)emoticonSlotInfo.Value.SlotIndex;
			emoticonId = (long)emoticonSlotInfo.Value.EmoticonTid;
		}
	}
}
