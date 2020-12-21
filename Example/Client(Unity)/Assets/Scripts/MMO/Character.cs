using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string Name { get; set; }
    private CharacterController controller;
    private Vector3 targetPosition;
    private float speed = 0.0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        enabled = false;
    }
    //Задать новую точку назначения
    internal void Move(Vector3 position)
    {
        targetPosition = position;
        //Скорость необхидамая чтобы достичь полученной точки за 0.5сек
        speed = (targetPosition - transform.position).magnitude / 0.5f;
        enabled = true;
    }

    private void Update()
    {
        //Направление в сторону точки назначения
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude > 0.01f)
        {
            Vector3 step = direction.normalized * speed * Time.deltaTime;
            //Если длина шага больше либо ровна длине до точки назначения, сделать шаг ровно в точку назначения и выключить скрипт
            if (direction.magnitude <= Vector3.Distance(transform.position, transform.position + step)) { step = direction; enabled = false; }
            controller.Move(step);
        }
    }
}
