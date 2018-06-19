using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform Camera;
    public Transform CameraRoot;
    public float RotationSpeed = 3;
    public float Zoom = 3;
    public float ZoomSpeed = 2;
    public float MinZoom = 0;
    public float MaxZoom = 10;
    [Tooltip("Camera distance = 10 * ZoomBase^Zoom")]
    public float ZoomBase = 1.1f;

    private float TargetRotation = 60;
    private float Rotation = 0;

    private void Update() {
        float RotationDelta = 0;
        float ZoomDelta = 0;
        if (Input.GetButton("Rotate (by dragging)")) {
            RotationDelta += Input.GetAxis("Mouse X");
        }
        ZoomDelta += Input.GetAxis("Mouse ScrollWheel");
        RotationDelta -= Input.GetAxis("Rotate");
        ZoomDelta += Input.GetAxis("Zoom") * 0.2f;

        Vector3 Rot = CameraRoot.localEulerAngles;
        TargetRotation = (TargetRotation + RotationDelta * RotationSpeed) % 360;
        Rotation = Mathf.Lerp(Rotation, TargetRotation, 40f * Time.deltaTime);
        Rot.y = Rotation;
        CameraRoot.localEulerAngles = Rot;

        Zoom = Mathf.Clamp(Zoom - ZoomDelta * ZoomSpeed, MinZoom, MaxZoom);
        Vector3 Pos = Camera.localPosition;
        Pos.z = Mathf.Lerp(Pos.z, Mathf.Pow(ZoomBase, Zoom) * 10, 20f * Time.deltaTime);
        Camera.localPosition = Pos;
    }
}
