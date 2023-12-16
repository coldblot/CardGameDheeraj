using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float clampVertical;

    public float cameraSensitivity;

    float verticalRotation;
    float horizonRotation;
    private void Update()
    {
        Look();
        Debug.DrawLine(this.transform.position,transform.forward,Color.red);
    }

    private void Look()
    {
        float mouseHorizontal = Input.GetAxis("Mouse X");
        float mouseVertical = Input.GetAxis("Mouse Y");

        verticalRotation += mouseVertical * cameraSensitivity*Time.deltaTime;
        horizonRotation += mouseHorizontal * cameraSensitivity*Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation,-clampVertical,clampVertical);
        this.transform.localRotation = Quaternion.Euler(verticalRotation,0, 0);
        player.transform.rotation = Quaternion.Euler(0,horizonRotation,0);
    }
}
