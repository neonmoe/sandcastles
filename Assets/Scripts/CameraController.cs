using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform Camera;
    public Transform CameraRoot;
    public float RotationSpeed = 3;
    public float MaxVerticalRotation = -30;
    public float MinVerticalRotation = -89;
    public float Zoom = 3;
    public float ZoomSpeed = 2;
    public float MinZoom = 0;
    public float MaxZoom = 10;
    [Tooltip("Camera distance = 10 * ZoomBase^Zoom")]
    public float ZoomBase = 1.1f;

    private float TargetHorizontalRotation = 60;
    private float HorizontalRotation = 0;
    private float TargetVerticalRotation = 60;
    private float VerticalRotation = 0;

    private void Update() {
        float HorizontalRotationDelta = 0;
        float VerticalRotationDelta = 0;
        float ZoomDelta = 0;
        if (Input.GetButton("Rotate (by dragging)")) {
            HorizontalRotationDelta += Input.GetAxis("Mouse X");
            VerticalRotationDelta += Input.GetAxis("Mouse Y");
        }
        ZoomDelta += Input.GetAxis("Mouse ScrollWheel");
        HorizontalRotationDelta -= Input.GetAxis("Rotate");
        ZoomDelta += Input.GetAxis("Zoom") * 0.2f;

        Vector3 Rot = CameraRoot.localEulerAngles;
        TargetHorizontalRotation = (TargetHorizontalRotation + HorizontalRotationDelta * RotationSpeed) % 360;
        TargetVerticalRotation = Mathf.Clamp((TargetVerticalRotation + VerticalRotationDelta * RotationSpeed) % 360, 
            MinVerticalRotation, MaxVerticalRotation);
        HorizontalRotation = Mathf.Lerp(HorizontalRotation, TargetHorizontalRotation, 40f * Time.deltaTime);
        VerticalRotation = Mathf.Lerp(VerticalRotation, TargetVerticalRotation, 40f * Time.deltaTime);
        Rot.y = HorizontalRotation;
        Rot.x = VerticalRotation;
        CameraRoot.localEulerAngles = Rot;

        Zoom = Mathf.Clamp(Zoom - ZoomDelta * ZoomSpeed, MinZoom, MaxZoom);
        Vector3 Pos = Camera.localPosition;
        Pos.z = Mathf.Lerp(Pos.z, Mathf.Pow(ZoomBase, Zoom) * 10, 20f * Time.deltaTime);
        Camera.localPosition = Pos;
    }
}
