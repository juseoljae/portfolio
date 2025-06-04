using UnityEngine;
using System.Collections;

public class UI_DPad : BaseUIComponent
{
    #region VARIABLES

    private bool m_bPressed                         = false;

    private Transform m_trBackGround                = null;
    private Transform m_trDirection                 = null;

    public Camera m_2dCamera                        = null;
    private Camera m_3dCamera                       = null;

    public Vector3 m_3dCameraAngle                  = Vector3.zero;

    public bool m_bUpdateMainCameraRotate           = true;

    private GameObject m_Hero                       = null;

    private MyCharacterController m_CharController    = null;

    private readonly float m_fMaxBound              = 0.17f;

    TweenAlpha tAlpha;

	private bool m_UpdateStart						= false;
    #endregion VARIABLES


    #region PROPERTIES

    public bool bPressed
    {
        get { return m_bPressed; }
    }

    #endregion PROPERTIES


    #region UNITY FUNC
	public void Initialize()
	{
		m_trDirection = this.transform.Find("UI_Position/Sp_Direction");
		m_trBackGround = this.transform.Find("UI_Position/Sp_Background");

		m_2dCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();

		m_3dCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		m_3dCameraAngle = m_3dCamera.transform.localEulerAngles;

		SetDirectionInitPosition();

		tAlpha = this.gameObject.GetComponent<TweenAlpha>();
		m_UpdateStart = true;
	}
	void Update()
    {
        if (InGameManager.instance.CanAccessState() == false || gameObject.activeSelf == false)
            return;

		if(m_UpdateStart == false)
		{
			return;
		}
        if (m_bPressed == true) 
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    {
                        if (Input.touchCount > 0)
                        {
                            Vector3 nTouchPosition = GetTouchVectorCloseByDirection();
                            SetDirectionPosition(nTouchPosition);
                        }
                    }
                    break;
                default:
                    {
                        Vector3 nMousePosition = GetMouseVector();
                        SetDirectionPosition(nMousePosition);
                    }
                    break;
            }
        }
        else if (m_bPressed == false) 
        {
            SetDirectionInitPosition();
        }
    }    

    private void OnDisable()
    {
        m_bPressed = false;
    }

    #endregion UNITY FUNC



    #region PUBLIC FUNC

    public void DoPress()
    {
        if (InGameManager.instance.CanAccessState() == false || gameObject.activeSelf == false)
            return;



        if (m_CharController == null) return;

        m_bPressed = true;
        m_CharController.PressMove(m_bPressed);
        
        tAlpha.from = 0.8f;
        tAlpha.to = 0.8f;
        tAlpha.PlayForward();
    }

    public void DoRelease()
    {
        if (InGameManager.instance.CanAccessState() == false || gameObject.activeSelf == false)
            return;

        if (m_CharController == null) return;

        m_bPressed = false;
        m_CharController.PressMove(m_bPressed);

        // gunny
        tAlpha.from = 0.3f;
        tAlpha.to = 0.3f;
        tAlpha.PlayForward();
    }

    /// <summary>
    /// 메인 카메라의 각을 구해서 방향키의 값을 가공하여 전달
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDirection()
    {
        if (m_bPressed == true)
        {
            if (m_bUpdateMainCameraRotate == true)
            {
                if (m_3dCamera == null)
                {
                    m_3dCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                }
                m_3dCameraAngle = m_3dCamera.transform.localEulerAngles;
            }

            Vector3 rotatiedDirection = (m_trDirection.position - m_trBackGround.position);

            float fX = rotatiedDirection.x;
            float fY = rotatiedDirection.y;

            Vector2 direction = new Vector2(fX, fY);

            return direction;    
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void SetHero(GameObject hero)
    {
        m_Hero = hero;
        if (m_CharController == null)
            m_CharController = m_Hero.GetComponent<MyCharacterController>();
    }

    #endregion PUBLIC FUNC




    #region PRIVATE FUNC

    /// <summary>
    /// 방향키의 위치 설정
    /// </summary>
    private void SetDirectionPosition(Vector3 nInputVector)
    {
        float x = nInputVector.x;
        float y = nInputVector.y;
        float z = m_trDirection.position.z;
      
        Vector3 inputPosition = new Vector3(x, y, z);

        float fInputDistanceFromBackground  = Vector3.Distance(inputPosition, m_trBackGround.position);

        /// 방향키가 갈수 있는 범위에 따라 위치 설정
        if (fInputDistanceFromBackground <= m_fMaxBound)        //범위 안에 있는 경우
        {
            m_trDirection.position = inputPosition;
        }
        else if (m_fMaxBound < fInputDistanceFromBackground)    //범위 밖에 있는 경우
        {
            float fRate = m_fMaxBound / fInputDistanceFromBackground;
            
            float fX = (inputPosition.x - m_trBackGround.position.x) * fRate;
            float fY = (inputPosition.y - m_trBackGround.position.y) * fRate;
            float fZ = 0;

            m_trDirection.position = m_trBackGround.position +  new Vector3(fX, fY, fZ);
        }
    }

    /// <summary>
    /// 방향키 위치 가운데로 맞춤
    /// </summary>
    private void SetDirectionInitPosition()
    {
        if ( m_trDirection.position != m_trBackGround.position )
        { 
            m_trDirection.position = m_trBackGround.position;
        }
    }

    /// <summary>
    /// 터치시 DPad의 가장 가까운 터치 위치를 찾음
    /// </summary>
    /// <returns></returns>
    private Vector3 GetTouchVectorCloseByDirection()
    {
        int nTouchCount = Input.touchCount;

        Ray[] rays = new Ray[nTouchCount];

        for (int i = 0; i < nTouchCount; i++ )
        {
            rays[i] = m_2dCamera.ScreenPointToRay(Input.GetTouch(i).position);
        }

        Vector3 returnVector = rays[0].origin;

        for (int i = 1; i < nTouchCount; i++ )
        {
            if (GetDistanceFromDirection(rays[i].origin) < GetDistanceFromDirection(returnVector))
                returnVector = rays[i].origin;
        }

        return returnVector;
    }   

    /// <summary>
    /// 방향키와의 거리를 구함
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    private float GetDistanceFromDirection(Vector3 vector)
    {
        return Vector3.Distance(m_trDirection.position, vector);
    }

    /// <summary>
    /// 에디터 상에서의 마우스 포지션
    /// </summary>
    /// <returns></returns>
    private Vector3 GetMouseVector()
    {
        Ray ray = m_2dCamera.ScreenPointToRay(Input.mousePosition);

        return ray.origin;
    }

    #endregion PRIVATE FUNC
}
