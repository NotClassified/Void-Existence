using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCamMove : MonoBehaviour
{

    public Vector3[] pointPos;
    public Vector3[] pointRot;
    public float[] speed;

    IEnumerator Start()
    {
        int index = 0;
        while (true)
        {
            float time = 0;

            Vector3 initialPos = pointPos[index];
            Vector3 initialRot = pointRot[index];
            while (time < 1)
            {
                time += Time.deltaTime * speed[index];
                transform.position = Vector3.Lerp(initialPos, pointPos[index + 1], time);
                transform.eulerAngles = Vector3.Lerp(initialRot, pointRot[index + 1], time);
                yield return null;
            }
            transform.position = pointPos[index + 1];
            transform.eulerAngles = pointRot[index + 1];
            while (pointPos[index + 2] == Vector3.zero)
                index++;
            index += 2;
        }
    }
}
