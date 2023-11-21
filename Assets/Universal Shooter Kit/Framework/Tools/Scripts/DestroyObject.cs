using UnityEngine;
using System.Collections;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif

namespace GercStudio.USK.Scripts
{
    public class DestroyObject : MonoBehaviour
    {
        public float destroyTime;
        private float currentTime;

        [Tooltip("Decreases bullet trails color alpha over a specified time")]
        public bool reduceColor;

        void Start()
        {
            StartCoroutine(CheckIfAlive());
        }

        void Update()
        {
            currentTime += Time.deltaTime;
            
            if (gameObject.GetComponent<LineRenderer>())
            {
                var script = gameObject.GetComponent<LineRenderer>();
                var startColor = script.startColor;
                var endColor = script.endColor;
                
                DecreaseColor(ref startColor, ref endColor);

                script.startColor = startColor;
                script.endColor = endColor;
            }
            else if (gameObject.GetComponent<TrailRenderer>())
            {
                var script = gameObject.GetComponent<TrailRenderer>();
                var startColor = script.startColor;
                var endColor = script.endColor;

                DecreaseColor(ref startColor, ref endColor);

                script.startColor = startColor;
                script.endColor = endColor;
            }
        }

        IEnumerator CheckIfAlive()
        {
            yield return new WaitForSeconds(destroyTime);

            if (gameObject.GetComponent<Blip>())
            {
                var blipScript = gameObject.GetComponent<Blip>();

                if (blipScript.blipImage != null && blipScript.blipImage.image)
                    Destroy(blipScript.blipImage.image.gameObject);
            }

#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && gameObject.GetComponent<PhotonView>())
            {
                if(PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
#else
                Destroy(gameObject);
#endif
        }

        void DecreaseColor(ref Color startColor, ref Color endColor)
        {
            startColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.LerpUnclamped(startColor.a, 0, currentTime / destroyTime));
            endColor = new Color(endColor.r, endColor.g, endColor.b, Mathf.LerpUnclamped(endColor.a, 0, currentTime / destroyTime));
        }
    }
}




