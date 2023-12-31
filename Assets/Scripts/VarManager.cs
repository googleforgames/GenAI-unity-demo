using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;

public class VarManager : MonoBehaviour
{
    public string EndpointURI_EventIngest;
    public string EndpointURI_ImageGen;
    public string EndpointURI_LLM;

    public static string varEndpointURI_EventIngest;
    public static string varEndpointURI_ImageGen;
    public static string varEndpointURI_LLM;

    private Texture2D texture;
    private float timeStamp = 0;
    private float imageGenerationRefreshRate = 0.5f;
    private int imageGenerationCycle = 0;

    // Start is called before the first frame update
    void Start()
    {
        varEndpointURI_EventIngest = EndpointURI_EventIngest;
        varEndpointURI_ImageGen = EndpointURI_ImageGen;
        varEndpointURI_LLM = EndpointURI_LLM;


        // Reset Sprite (Billboard) Texture to Neutral
        texture = new Texture2D(2, 2);
        /*
        GameObject gameObjectToApplySprite = GameObject.Find("BillboardSprite");
        SpriteRenderer spriteRenderer = gameObjectToApplySprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer.sortingOrder = -1;
        */

        //TODO: Rename variables for readability
        GameObject gameObjectToApplySprite2 = GameObject.Find("BillboardSpriteBurger");
        SpriteRenderer spriteRenderer2 = gameObjectToApplySprite2.GetComponent<SpriteRenderer>();
        spriteRenderer2.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer2.sortingOrder = -1;
        
        GameObject gameObjectToApplySprite3 = GameObject.Find("BillboardSpriteGate");
        SpriteRenderer spriteRenderer3 = gameObjectToApplySprite3.GetComponent<SpriteRenderer>();
        spriteRenderer3.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer3.sortingOrder = -1;
        
        GameObject gameObjectToApplySprite4 = GameObject.Find("BillboardSpriteBus");
        SpriteRenderer spriteRenderer4 = gameObjectToApplySprite4.GetComponent<SpriteRenderer>();
        spriteRenderer4.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer4.sortingOrder = -1;
        
        GameObject gameObjectToApplySprite5 = GameObject.Find("BillboardSpriteShip");
        SpriteRenderer spriteRenderer5= gameObjectToApplySprite5.GetComponent<SpriteRenderer>();
        spriteRenderer5.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer5.sortingOrder = -1;
       

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeStamp > imageGenerationRefreshRate)
        {
            //Debug.Log("Generate new image");
            
           switch (imageGenerationCycle)
            {
                case 0:
                    generateImage("BillboardSpriteCube", "scifi image of black cube with bright teal borders", 1);
                    break;
                case 1:
                    generateImage("WallDisplay1Sprite", "fantasy art of a robot holding a hamburger pointing in one hand", 1);
                    break;

                case 2:
                    generateImage("WallDisplay2Sprite", "scifi image of an alien sitting on top of a jeep within a city", 1);
                    break;
                case 3:
                    generateImage("BillboardSpriteBurger", "scifi image of a dinner with a hamburger and an open now sign in the background, lightning jumping around the sign", 1);
                    break;
                case 4:
                    generateImage("BillboardSpriteGate", "scifi image of a small flame coming out of turbine", 1);
                    break;
                case 5:
                    generateImage("BillboardSpriteBus", "scifi image of a school bus and an alien playing with a plant", 1);
                    break;
                case 6:
                    generateImage("BillboardSpriteShip", "scifi image of a spaceship taking off within a city", 1);
                    break;
            }
            
            imageGenerationCycle++;
            if (imageGenerationCycle > 7)
            { 
                imageGenerationCycle = 0;
            }
            /*
            
                generateImage("BillboardSpriteCube", "scifi image of black cube with bright teal borders", 5);
                generateImage("WallDisplay1Sprite", "fantasy art of a robot holding a hamburger pointing in one hand", 10);
                generateImage("WallDisplay2Sprite", "scifi image of an alien sitting on top of a jeep within a city", 15);
                generateImage("BillboardSpriteBurger", "scifi image of a dinner with a hamburger and an open now sign in the background, lightning jumping around the sign", 20);
                generateImage("BillboardSpriteGate", "scifi image of a small flame coming out of turbine", 25);
                generateImage("BillboardSpriteBus", "scifi image of a school bus and an alien playing with a plant", 30);
                generateImage("BillboardSpriteShip", "scifi image of a spaceship taking off within a city", 35);
            */
            timeStamp = Time.time;
            //Debug.Log("Images Generated");
        }

        //Debug.Log("Time since last prompt: " + (Time.time - timeStamp));
    }

    async Task generateImage(string SpriteName, string imageContext, int timeDelay)
    {
        //await Task.Delay(timeDelay);

        //Create a new web request
        Debug.Log("Generating Initial Billboard Image");

        // Get imageURL from VarManager class
        string imageUrl = VarManager.varEndpointURI_ImageGen;
        //imageUrl = "http://" + imageUrl + "/vertex_image_gen?prompt=";
        //imageUrl = "http://" + imageUrl + "/get_image?prompt=";
        imageUrl = "http://" + imageUrl + "/generate_img?prompt=";

        //string brandedAdContent = AdRequest.adContentGlobal;
        string brandedAdContent = "";
        string[] words = brandedAdContent.Split(' ');
        imageUrl += "" + words[0] + imageContext;
        Debug.Log("Initial Image Gen URL: " + imageUrl);
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

            GameObject gameObjectToApplySprite = GameObject.Find(SpriteName);
            SpriteRenderer spriteRenderer = gameObjectToApplySprite.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            spriteRenderer.sortingOrder = -1;

        }
    }

}
