using UnityEngine;
using System.Collections;

public class Tank : Unit
{
    public float moveSpeed;
    public float rotateSpeed;

    private float horizontal;
    private float vertical;
    private TankWeapon tw;

    void Start()
    {
        base.StartTo();
        tw = GetComponent<TankWeapon>();
        tw.Init(team);
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime * vertical);
        transform.Rotate(Vector3.down * rotateSpeed * Time.deltaTime * -horizontal);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            tw.Shoot();
        }

        /*if (Input.GetKey (KeyCode.W)) {
            transform.Translate (Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey (KeyCode.S)) {
            transform.Translate (Vector3.back * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey (KeyCode.A)) {
            transform.Rotate (Vector3.down * rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey (KeyCode.D)) {
            transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
        }*/
    }
}