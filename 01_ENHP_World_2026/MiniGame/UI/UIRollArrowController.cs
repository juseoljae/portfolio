using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRollArrowController : MonoBehaviour
{
    [SerializeField] private RectTransform arrowRect;

    public bool isRolling = false;

    private Coroutine rollingCoroutine;

    private const float DURATION = 2f;    
    private const int ANGLE_MIN = -90;
    private const int ANGLE_MAX = 90;
    private const int ROLL_AREA_MAX = 10;
    private const int EACH_ROLLAREA_ANGLE = 18; // 180 / 10 areas


    public void SetRolling(bool rolling)
    {
        isRolling = rolling;
    }

    public bool GetRolling()
    {
        return isRolling;
    }

    public void StartRolling()
    {
        if (isRolling)
            return;

        if (rollingCoroutine != null)
            StopCoroutine(rollingCoroutine);

        isRolling = true;
        rollingCoroutine = StartCoroutine(RollingRoutine());
    }

    public void StopRolling()
    {
        if (!isRolling)
            return;

        isRolling = false;

        if (rollingCoroutine != null)
        {
            StopCoroutine(rollingCoroutine);
            rollingCoroutine = null;
        }
    }

    private IEnumerator RollingRoutine()
    {
        float time = 0f;

        while (isRolling)
        {
            time += Time.deltaTime;

            float t = Mathf.PingPong(time / (DURATION * 0.5f), CMarbleDefine.MARBLE_DICE_ROLLING_TIME);

            float z = Mathf.Lerp(ANGLE_MAX, ANGLE_MIN, t);
            arrowRect.localRotation = Quaternion.Euler(0f, 0f, z);

            yield return null;
        }
    }

    public int GetArrowSection()
    {
        float zRotation = arrowRect.localEulerAngles.z;

        if (zRotation > 180f)
            zRotation -= 360f;

        zRotation = Mathf.Clamp(zRotation, ANGLE_MIN, ANGLE_MAX);

        float normalized = ANGLE_MAX - zRotation;

        int section = Mathf.FloorToInt(normalized / EACH_ROLLAREA_ANGLE) + 1;

        return Mathf.Clamp(section, 1, ROLL_AREA_MAX) + 1;//+1 is ui start from 2
    }
}
