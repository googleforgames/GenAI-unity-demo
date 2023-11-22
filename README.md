# Unity GenAI game integration demo

# Server setup
## Prerequisites
This example is working on

```
Unity Editor: Unity 2022.3.3f1
OS: Windows
```

## Multiplayer server build and installation
### Install and configure Agones on Kubernetes
Check out [these instructions](https://agones.dev/site/docs/installation/).

### Building the Dedicated Game Server
* Open this project with UnityEditor.
* Click on the **File** > **Build Settings** menu item in the menu bar.
* Make sure that for **Platform** that **Dedicated Game Server** is selected and **Target Platform** is selected to **Linux**. If needed click **Switch Platform** before build.
* Verify the following scenes are selected in order:

  ```
  Scenes/NetworkBootstrap
  Scenes/MainMenu
  Universal Shooter Kit\Demos\Scenes\Single-player\Single Demo
  ```

* Click **Build**.
* The Builds are created in a `ServerBuild` Folder and saved as `Server`.

### Running the Dedicated Game Server on Kubernetes

#### Option 1: Build and push with Docker

Build docker image and push to registry.

```
docker build -t agones-agones-example/unity-city:latest .
```

#### Option 2: Google Cloud Build

As a another option, you can build with [Google Cloud Build](https://cloud.google.com/build/docs/build-config-file-schema). This will also push image to Google Artifact Registry at `us-docker.pkg.dev/globant-kubecon/unity-agones-example/unity-city`.

If needed, you can create an Artifact Registry if you have a Google Cloud Organization:

```
gcloud artifacts repositories create unity-agones-example \
    --repository-format=docker \
    --location=us \
    --async
```

Run Cloud Build config:

```
gcloud builds submit --config cloudbuild.yaml
```

### Create Agones Gameserver
Run:

```
$ kubectl create -f gameserver.yaml
```

The `kubectl get gs` output should be similar to:

```
kubectl get gs
NAME                        STATE   ADDRESS         PORT   NODE                                              AGE
unity-city-server-b4hnz   Ready   34.69.***.***   7953   gke-gke-agones-gke-agones-primary-65d17602-ld6z   5d21h
```

When running the client use above `Address` and `Port`.

# Client setup
## Setting up the AI endpoints
Look for the Game Manager object in the Hierarchy.

Inside it, select GameVars.

![image](https://github.com/googleforgames/multiplayer-demo-game/assets/82907841/59b7f39a-6567-49a7-8c4a-4e68b3beee5f)

In the inspector, look for Var Manager (Script)

For image generation, set the IP and Port in Endpoint URI_Image Gen.

For text generation, set the IP and Port in Endpoint URI_LLM.

![image](https://github.com/googleforgames/multiplayer-demo-game/assets/82907841/e493b288-2a21-481a-8b9e-ed0e7c647301)

Go inside VarManager.cs, look for imageUrl in line 133 and make sure to set the correct endpoint URL for image generation.

Go inside ChatManager.cs, look for _endpointUrl in line 72 and make sure to set the correct endpoint URL for text generation.

## Playing
To start a session the server must be already running.

After starting the game, the connection widget will appear.

Write the IP and Port of the server and press connect.

This data will be saved so it doesn’t need to be added every time.

![image](https://github.com/googleforgames/multiplayer-demo-game/assets/82907841/c8fb7b44-f43b-4326-b303-6dcd118b0c65)
