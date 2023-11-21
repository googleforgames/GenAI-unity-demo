using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;

public class ImageRender : MonoBehaviour
{
    //public string imageUrl;
    public string imageTheme;
    public string colliderObjectName;

    private Texture2D texture;

    private bool hasTriggered = false;

    //async void Start()
    async private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == colliderObjectName && !hasTriggered)
        {
            hasTriggered = true;

            await generateImage();
        }
    }

    async Task generateImage()
    {
        //Create a new web request
        Debug.Log("Generating Billboard 2 Image - Triggered");

        // Get imageURL from VarManager class
        string imageUrl = VarManager.varEndpointURI_ImageGen;

        //string brandedAdContent = AdRequest.adContentGlobal;
        imageUrl += "fantasy%20art," + imageTheme;
        Debug.Log("Trigger Image 2 Gen URL: " + imageUrl);
        HttpWebRequest www = (HttpWebRequest)WebRequest.Create(imageUrl);

        //Send request to server and get the response
        HttpWebResponse response = (HttpWebResponse)await www.GetResponseAsync();
        Stream stream = response.GetResponseStream();

        //Read the response stream and convert it to a byte array
        byte[] buffer = new byte[16384];
        using (MemoryStream ms = new MemoryStream())
        {   
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ms.WriteAsync(buffer, 0, bytesRead);
            }
            byte[] data = ms.ToArray();

            //Convert the byte array to a base64 string
            string base64String = Convert.ToBase64String(data);

            //Convert the base64 string back to a byte array
            byte[] imageData = Convert.FromBase64String(base64String);

            //Create a new texture and load the byte array data into it
            texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            GameObject gameObjectToApplySprite = GameObject.Find("BillboardSprite2");
            SpriteRenderer spriteRenderer = gameObjectToApplySprite.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            spriteRenderer.sortingOrder = -1;

        }
    }

}
