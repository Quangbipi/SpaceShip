using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController: MonoBehaviour
{
    public bool isLargeAsteroid;

    private bool isDestroyed;

    private PhotonView photonView;

    

#pragma warning disable 0109
    private new Rigidbody rigidbody;
#pragma warning restore 0109

    #region UNITY

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();

        rigidbody = GetComponent<Rigidbody>();
        // x? ly du lieu khoi tao
        if (photonView.InstantiationData != null)
        {
            rigidbody.AddForce((Vector3)photonView.InstantiationData[0]);
            rigidbody.AddTorque((Vector3)photonView.InstantiationData[1]);

            isLargeAsteroid = (bool)photonView.InstantiationData[2];
        }

       
    }

   

    public void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        // pha huy thien thach khi di qua screen
        if (Mathf.Abs(transform.position.x) > Camera.main.orthographicSize * Camera.main.aspect || Mathf.Abs(transform.position.z) > Camera.main.orthographicSize)
        {
            // Out of the screen
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed)
        {
            return;
        }
        // check va cham vs dan
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // check xem dan co ng dkhien hien tai khong
            if (photonView.IsMine)
            {
                
                Rocket bullet = collision.gameObject.GetComponent<Rocket>();
                bullet.Owner.AddScore(isLargeAsteroid ? 2 : 1);

                DestroyAsteroidGlobally();
            }
            else
            {
                DestroyAsteroidLocally();
            }
        }
        // va cham voi phi thuyen khac
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (photonView.IsMine)
            {
                collision.gameObject.GetComponent<PhotonView>().RPC("DestroySpaceship", RpcTarget.All);

                DestroyAsteroidGlobally();
            }
        }
    }

    #endregion

    private void DestroyAsteroidGlobally()
    {
        isDestroyed = true;

        if (isLargeAsteroid)
        {
            // random so luong thien thach nho
            int numberToSpawn = Random.Range(3, 6);

            // spawn thien thach
            for (int counter = 0; counter < numberToSpawn; ++counter)
            {
                Vector3 force = Quaternion.Euler(0, counter * 360.0f / numberToSpawn, 0) * Vector3.forward * Random.Range(0.5f, 1.5f) * 300.0f;
                Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);
                object[] instantiationData = { force, torque, false, PhotonNetwork.Time };
                // tao ra thien thach nho tren server
                PhotonNetwork.InstantiateRoomObject("SmallAsteroid", transform.position + force.normalized * 10.0f, Quaternion.Euler(0, Random.value * 180.0f, 0), 0, instantiationData);
            }
        }
        

        PhotonNetwork.Destroy(gameObject);

    }

    private void DestroyAsteroidLocally()
    {
        isDestroyed = true;

        GetComponent<Renderer>().enabled = false;
    }
}
