using Assets.Script;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SpaceShipController : MonoBehaviour
{
    public float RotationSpeed = 90.0f;
    public float MovementSpeed = 2.0f;
    public float MaxSpeed = 0.2f;

    public ParticleSystem Destruction;
    public GameObject EngineTrail;
    public GameObject BulletPrefab;

    private PhotonView photonView;

    public AudioClip Cup, Cup2;
    private AudioSource audioSource;

#pragma warning disable 0109
    private new Rigidbody rigidbody;
    private new Collider collider;
    private new Renderer renderer;
#pragma warning restore 0109

    private float rotation = 0.0f;
    private float acceleration = 0.0f;
    private float shootingTimer = 0.0f;

    private bool controllable = true;

    #region UNITY

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    public void Start()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = SpaceShip.GetColor(photonView.Owner.GetPlayerNumber());
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Cup;
        audioSource.Stop();
    }

    public void Update()
    {
        if (!photonView.AmOwner || !controllable)
        {
            return;
        }

        // we don't want the master client to apply input to remote ships while the remote player is inactive
        if (this.photonView.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            return;
        }

        rotation = Input.GetAxis("Horizontal");
        acceleration = Input.GetAxis("Vertical");

        if (Input.GetButton("Jump") && shootingTimer <= 0.0)
        {
            shootingTimer = 0.2f;

            photonView.RPC("Fire", RpcTarget.AllViaServer, rigidbody.position, rigidbody.rotation);
        }

        if (shootingTimer > 0.0f)
        {
            shootingTimer -= Time.deltaTime;
        }
    }

    public void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (!controllable)
        {
            return;
        }

        Quaternion rot = rigidbody.rotation * Quaternion.Euler(0, rotation * RotationSpeed * Time.fixedDeltaTime, 0);
        rigidbody.MoveRotation(rot);

        Vector3 force = (rot * Vector3.forward) * acceleration * 1000.0f * MovementSpeed * Time.fixedDeltaTime;
        rigidbody.AddForce(force);

        if (rigidbody.velocity.magnitude > (MaxSpeed * 1000.0f))
        {
            rigidbody.velocity = rigidbody.velocity.normalized * MaxSpeed * 1000.0f;
        }

        CheckExitScreen();
    }

    #endregion

    #region COROUTINES

    private IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(SpaceShip.PLAYER_RESPAWN_TIME);

        photonView.RPC("RespawnSpaceship", RpcTarget.AllViaServer);
    }

    #endregion

    #region PUN CALLBACKS

    [PunRPC]
    public void DestroySpaceship()
    {
        audioSource.clip = Cup2;
        audioSource.Play();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        collider.enabled = false;
        renderer.enabled = false;

        controllable = false;

        EngineTrail.SetActive(false);
        Destruction.Play();

        if (photonView.IsMine)
        {
            object lives;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(SpaceShip.PLAYER_LIVES, out lives))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { SpaceShip.PLAYER_LIVES, ((int)lives <= 1) ? 0 : ((int)lives - 1) } });

                if (((int)lives) > 1)
                {
                    StartCoroutine("WaitForRespawn");
                }
            }
        }
    }

    [PunRPC]
    public void Fire(Vector3 position, Quaternion rotation, PhotonMessageInfo info)
    {
        float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
        GameObject bullet;

        /** Use this if you want to fire one bullet at a time **/
        bullet = Instantiate(BulletPrefab, position, Quaternion.identity) as GameObject;
        bullet.GetComponent<Rocket>().InitializeBullet(photonView.Owner, (rotation * Vector3.forward), Mathf.Abs(lag));

        audioSource.clip = Cup;
        audioSource.Play();
    }

    [PunRPC]
    public void RespawnSpaceship()
    {
        collider.enabled = true;
        renderer.enabled = true;

        controllable = true;

        EngineTrail.SetActive(true);
        Destruction.Stop();
    }

    #endregion

    private void CheckExitScreen()
    {
        if (Camera.main == null)
        {
            return;
        }

        if (Mathf.Abs(rigidbody.position.x) > (Camera.main.orthographicSize * Camera.main.aspect))
        {
            rigidbody.position = new Vector3(-Mathf.Sign(rigidbody.position.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, rigidbody.position.z);
            rigidbody.position -= rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        }

        if (Mathf.Abs(rigidbody.position.z) > Camera.main.orthographicSize)
        {
            rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, -Mathf.Sign(rigidbody.position.z) * Camera.main.orthographicSize);
            rigidbody.position -= rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        }
    }
}
