using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public Player Owner { get; private set; }

    private PhotonView photonView;

    public void Start()
    {
        Destroy(gameObject, 3.0f);
        photonView = GetComponent<PhotonView>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);

        if(collision.gameObject.CompareTag("Player"))
        {
            if (photonView.IsMine)
            {
                collision.gameObject.GetComponent<PhotonView>().RPC("DestroySpaceship", RpcTarget.All);
            }
            

            
        }

    }

    public void InitializeBullet(Player owner, Vector3 originalDirection, float lag)
    {
        Owner = owner;

        transform.forward = originalDirection;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = originalDirection * 200.0f;
        rigidbody.position += rigidbody.velocity * lag;
    }
}
