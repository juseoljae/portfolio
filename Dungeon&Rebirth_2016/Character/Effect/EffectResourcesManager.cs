using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KSPlugins;

public class EffectResourcesManager : KSManager
{
    public Dictionary<string, GameObject> m_EffOriginObjs;
    public List<GameObject> m_EffectResObj;
    private GameObject m_gobjParent = null;

    #region     INSTANCE
    private static EffectResourcesManager m_Instance;
    public static EffectResourcesManager instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (MainManager.instance != null)
                    m_Instance = CreateManager(typeof(EffectResourcesManager).Name).AddComponent<EffectResourcesManager>();
                else
                    m_Instance = new EffectResourcesManager();

                m_Instance.Initialize();
            }

            return m_Instance;
        }
    }
    #endregion  INSTANCE

    #region     OVERRIDE MEMBERS
    public override void Initialize()
    {
        m_EffectResObj = new List<GameObject>();
        m_EffOriginObjs = new Dictionary<string, GameObject>();
    }


    public override void Destroy()
    {
        base.Destroy();

		if(m_EffectResObj != null)
		{
			m_EffectResObj.Clear();
		}
		if(m_EffOriginObjs != null)
		{
			m_EffOriginObjs.Clear();
		}

    }
    #endregion  OVERRIDE MEMBERS

    public GameObject GetEffectFindAndLoadGameObject(string p_strEffectName)
    {
        StringBuilder strbPath = new StringBuilder();

        if (m_gobjParent == null)
        {
            m_gobjParent = new GameObject("EffectResources");
            m_gobjParent.transform.localPosition = Vector3.zero;
            m_gobjParent.transform.localEulerAngles = Vector3.zero;
            m_gobjParent.transform.localScale = Vector3.one;
            m_gobjParent.SetActive(false);
        }

        strbPath.AppendFormat("Effect/{0}", p_strEffectName);
        

        if (m_EffOriginObjs != null)
        {
            if (!m_EffOriginObjs.ContainsKey(p_strEffectName))
            {
                GameObject go = LoadEffectResource(strbPath);

                m_EffOriginObjs.Add(p_strEffectName, go);
            }
            else
            {
                if(m_EffOriginObjs[p_strEffectName] == null)
                {
                    GameObject go = LoadEffectResource(strbPath);

                    m_EffOriginObjs[p_strEffectName] = go;
                }
            }
        }

        return m_EffOriginObjs[p_strEffectName];
    }

    private GameObject LoadEffectResource(StringBuilder path)
    {
        GameObject loadedObj = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>(path.ToString());
        UtilManager.instance.ResetShader(loadedObj);

        if (loadedObj.activeSelf == false)
        {
            loadedObj.SetActive(true);
        }

        loadedObj.transform.parent = m_gobjParent.transform;
        loadedObj.transform.localPosition = new Vector3(-1000, -1000, -1000);

        return loadedObj;
    }
}
