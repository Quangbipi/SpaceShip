using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Script.GameScript
{

    /*
    public class HighScoreManager : MonoBehaviourPunCallbacks
    {
        private const string HIGH_SCORE_KEY = "high_score";
        private const string PLAYER_NAME_KEY = "player_name";
        private int highScore;
        private string playerName;

        public void SaveHighScore(int score, string name)
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Lưu onl");
                ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
                customProps[HIGH_SCORE_KEY] = score;
                customProps[PLAYER_NAME_KEY] = name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            }
            else
            {
                Debug.Log("Lưu local");
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
                PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
                PlayerPrefs.Save();
            }
        }

        public int GetHighScore()
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Get onl");
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(HIGH_SCORE_KEY, out object value))
                {
                    return (int)value;
                }
                return 0;
            }
            else
            {
                return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            }
        }

        public string GetPlayerName()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PLAYER_NAME_KEY, out object value))
                {
                    return (string)value;
                }
                return "";
            }
            else
            {
                return PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(HIGH_SCORE_KEY))
            {
                highScore = (int)changedProps[HIGH_SCORE_KEY];
            }
            if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(PLAYER_NAME_KEY))
            {
                playerName = (string)changedProps[PLAYER_NAME_KEY];
            }
        }

        private void Start()
        {
            highScore = GetHighScore();
            playerName = GetPlayerName();
        }
    }
    */

    
    public class HighScoreManager : MonoBehaviourPunCallbacks
    {
        private const string HIGH_SCORE_KEY = "high_score";
        private const string PLAYER_NAME_KEY = "player_name";
        private int highScore;
        private string playerName;

        private static HighScoreManager instance; // Thực thể duy nhất

        public static HighScoreManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<HighScoreManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "HighScoreManager";
                        instance = obj.AddComponent<HighScoreManager>();
                    }
                }
                return instance;
            }
        }

        public void SaveHighScore(int score, string name)
        {
            if (PhotonNetwork.IsConnected)
            {
                ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
                customProps[HIGH_SCORE_KEY] = score;
                customProps[PLAYER_NAME_KEY] = name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            }
            else
            {
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
                PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
                PlayerPrefs.Save();
            }
        }

        public int GetHighScore()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(HIGH_SCORE_KEY, out object value))
                {
                    return (int)value;
                }
                return 0;
            }
            else
            {
                return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            }
        }

        public string GetPlayerName()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PLAYER_NAME_KEY, out object value))
                {
                    return (string)value;
                }
                return "";
            }
            else
            {
                return PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(HIGH_SCORE_KEY))
            {
                highScore = (int)changedProps[HIGH_SCORE_KEY];
            }
            if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(PLAYER_NAME_KEY))
            {
                playerName = (string)changedProps[PLAYER_NAME_KEY];
            }
        }

        private void Start()
        {
            highScore = GetHighScore();
            playerName = GetPlayerName();
        }
    }
}