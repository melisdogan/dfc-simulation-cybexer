using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    readonly float angularSpeed = 2f;
    float angle = 0f;
    float prevAngle;
    readonly float radius = 70300f;
    Vector3 center;

    GameObject celestial;
    GameObject b;
    GameObject camera;


    // Start is called before the first frame update
    void Start()
    {
        center = GameObject.FindGameObjectWithTag("Center").transform.position;
        celestial = GameObject.FindGameObjectWithTag("Celestial");
        camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        prevAngle = angle;
        angle += Time.deltaTime * angularSpeed; // update angle
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up; // calculate direction from center - rotate the up vector Angle degrees clockwise
        celestial.transform.position = center + direction * radius; // update position based on center, the direction, and the radius (which is a constant)
        camera.transform.position = (center + (Vector3.forward * 80000)) + direction * (radius - 30000);
        //camera.transform.LookAt(celestial.transform);
        camera.transform.RotateAround(center, Vector3.forward, angle - prevAngle);
    }

}
