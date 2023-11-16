# Unity GenAI and Agones game demo

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
* The Builds are created in a `ServerBuild` Folder and **Saved As** `Server`.

### Running the Dedicated Game Server on Kubernetes

#### Option 1: Build and push with Docker

Build docker image and push to registry.

```
docker build -t agones-example/unity-netcode:latest .
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

The output should be similar to:

```
kubectl get gs
NAME                        STATE   ADDRESS         PORT   NODE                                              AGE
unity-city-server-b4hnz   Ready   34.69.***.***   7953   gke-gke-agones-gke-agones-primary-65d17602-ld6z   5d21h
```

When running client use above `Address` and `Port` for clients.
