using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameController gameController;
    public bool isEnabled = false;
    void Update()
    {
        if (isEnabled)
        {
            if (Input.GetButtonDown("Fire1")) Fire1();
            if (Input.GetButtonDown("Fire2")) Fire2();
            if (Input.GetButtonDown("Fire3")) Fire3();
            if (Input.GetButtonDown("Fire4")) Fire4();

            if (Input.GetButtonDown("Submit") && gameController.Hub.isDead && !gameController.isLoading) gameController.Restart();
            if (Input.GetButtonDown("Cancel")) Cancel();
        }
    }

    public void Fire1()
    {
        gameController.Hub.ShootTriangles(1);
    }
    public void Fire2()
    {
        gameController.Hub.ShootTriangles(2);
    }
    public void Fire3()
    {
        gameController.Hub.ShootTriangles(3);
    }
    public void Fire4()
    {
        gameController.Hub.ShootTriangles(4);
    }

    public void Cancel()
    {
        if(!gameController.Hub.isDead) gameController.Hub.Die();
    }

}
