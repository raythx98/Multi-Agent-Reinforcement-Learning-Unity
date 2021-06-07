<img src="docs/images/image-banner.png" align="middle" width="3000"/>

# Installation

If you have completed the installation process, look at the [getting started guide](#getting-started-guide)

To install and use the ML-Agents Toolkit you will need to:

- Install Unity (2019.4 or later)
- Install Python (3.6.1 or higher)
- Clone this repository
  - __Note:__ If you do not clone the repository, then you will not be
  able to access the example environments and training configurations or the
  `com.unity.ml-agents.extensions` package. 
- Install the `com.unity.ml-agents` Unity package
- Install the `mlagents` Python package

### Install **Unity 2019.4** or Later

[Download](https://unity3d.com/get-unity/download) and install Unity. I
strongly recommend that you install Unity through the Unity Hub as it will
enable you to manage multiple Unity versions.

### Install **Python 3.6.1** or Higher

I recommend [installing](https://www.python.org/downloads/) Python 3.6 or 3.7.
If you are using Windows, please install the x86-64 version and not x86.
If your Python environment doesn't include `pip3`, see these
[instructions](https://packaging.python.org/guides/installing-using-linux-tools/#installing-pip-setuptools-wheel-with-linux-package-managers)
on installing it.

### Clone the ML-Agents Toolkit Repository

```sh
git clone https://github.com/raythx98/Cooperative-Agents.git
```

### Install the `com.unity.ml-agents` Unity package

The Unity ML-Agents C# SDK is a Unity Package. You can install the
`com.unity.ml-agents` package
[directly from the Package Manager registry](https://docs.unity3d.com/Manual/upm-ui-install.html).
Please make sure you enable 'Preview Packages' in the 'Advanced' dropdown in
order to find the latest Preview release of the package.

**NOTE:** If you do not see the ML-Agents package listed in the Package Manager
please follow the [advanced installation instructions](#advanced-local-installation-for-development) below.

#### Advanced: Local Installation for Development

You can [add the local](https://docs.unity3d.com/Manual/upm-ui-local.html)
`com.unity.ml-agents` package (from the repository that you just cloned) to your
project by:

1. navigating to the menu `Window` -> `Package Manager`.
1. In the package manager window click on the `+` button on the top left of the packages list).
1. Select `Add package from disk...`
1. Navigate into the `com.unity.ml-agents` folder.
1. Select the `package.json` file.

If you are going to follow the examples from this documentation, you can open the
`Project` folder in Unity and start tinkering immediately.

### Install the `mlagents` Python package

Installing the `mlagents` Python package involves installing other Python
packages that `mlagents` depends on. So you may run into installation issues if
your machine has older versions of any of those dependencies already installed.
Consequently, the supported path for installing `mlagents` is to leverage Python
Virtual Environments. Virtual Environments provide a mechanism for isolating the
dependencies for each project and are supported on Mac / Windows / Linux.

#### (Windows) Installing PyTorch

On Windows, you'll have to install the PyTorch package separately prior to
installing ML-Agents. Activate your virtual environment and run from the command line:

```sh
pip3 install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
```

Note that on Windows, you may also need Microsoft's
[Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
if you don't have it already. See the [PyTorch installation guide](https://pytorch.org/get-started/locally/)
for more installation options and versions.

#### Installing `mlagents`

To install the `mlagents` Python package, activate your virtual environment and
run from the command line:

```sh
python -m pip install mlagents==0.26.0
```

Note that this will install `mlagents` from PyPi, _not_ from the cloned
repository. If you installed this correctly, you should be able to run
`mlagents-learn --help`, after which you will see the command
line parameters you can use with `mlagents-learn`.

By installing the `mlagents` package, the dependencies listed in the
[setup.py file](../ml-agents/setup.py) are also installed. These include
[PyTorch](Background-PyTorch.md) (Requires a CPU w/ AVX support).

# Getting Started Guide

This guide walks through the end-to-end process of opening one of my
example environments in Unity, training an Agent in it, and embedding 
the trained model into the Unity environment. After
reading this tutorial, you should be able to train any of the example
environments. 

For this guide, I'll use the **MAPOCA Agent** environment which contains a
number of agent cubes and balls (which are mostly copies of each other). Each agent
cube tries to maximise the total rewards by moving and rotating in a fixed area. 
In this environment, an agent cube is an **Agent** that receives a
reward for every ball eaten. 

An agent that eats a ball of the same colour will be awarded with 2 group points, 
or 1 group point otherwise. If all balls in the area
are eaten by the time limit, an additional 3 group points will be rewarded.
The goal of the training process is to have the agents learn cooperative behaviours
and sacrifice personal gains for group success.

Let's get started!

## Installating necessary components

If you haven't already, follow the [installation instructions](#Installation).

1. In Unity Hub, click on `Add` and open `Cooperative-Agents/Projects`
1. In the **Project** window, go to the
   `Assets/ML-Agents/Examples/MAPOCA Agent/Scenes` folder and open the `Poca Agent` scene
   file.

## Understanding a Unity Environment

An agent is an autonomous actor that observes and interacts with an
_environment_. In the context of Unity, an environment is a scene containing one
or more Agent objects, and, of course, the other entities that an agent
interacts with.


**Note:** In Unity, the base object of everything in a scene is the
_GameObject_. The GameObject is essentially a container for everything else,
including behaviors, graphics, physics, etc. To see the components that make up
a GameObject, select the GameObject in the Scene window, and open the Inspector
window. The Inspector shows every component on a GameObject.

The first thing you may notice after opening the MAPOCA Agent scene is that
it contains not one, but several play areas. Each area in the scene is an
independent area, but they all share the same Behavior. My Unity scene does
this to speed up training since all eight agents contribute to training in
parallel.

### Agent

The Agent is the actor that observes and takes actions in the environment. In
the MAPOCA Agent environment, the Agent components are placed on the sixteen
"Agent" GameObjects. The base Agent object has a few properties that affect its
behavior:

- **Behavior Parameters** — Every Agent must have a Behavior. The Behavior
  determines how an Agent makes decisions.
- **Max Step** — Defines how many simulation steps can occur before the Agent's
  episode ends. In MAPOCA Agent, the value 0 means that there is an unlimited
  number of steps, since the number of steps needs to be declared in the
  environment controller in multi-agent MAPOCA training.

#### Behavior Parameters : Vector Observation Space

Before making a decision, an agent collects its observation about its state in
the world. The vector observation is a vector of floating point numbers which
contain relevant information for the agent to make decisions.

The Behavior Parameters of the MAPOCA Agent example uses a `Space Size` of 3.
This means that the feature vector containing the Agent's observations contains
three elements: the `x` and `z` components of the agent cube's velocity and 
whether the ball is blue or red in colour.

In addition to Vector Observations, I also made use of visual sensors to train
the model, this can be done using methods such as grid sensors or attached cameras.
I adopted Raycast Perception Sensors, which casts Rays in all directions to detect
observe its environment. This method has shown to be efficient in practice due to
its reduced observation space.

#### Behavior Parameters : Actions

An Agent is given instructions in the form of actions.
ML-Agents Toolkit classifies actions into two types: continuous and discrete.
The MAPOCA Agent example is programmed to use both continuous, which
are a vector of floating-point numbers that can vary continuously. More specifically,
it uses a `Space Size` of 2 to control the amount of `x` and `z` forces to apply to
itself to adjust its motion in the play area. It also uses a discrete action, which is
a 'boolean' `0` or `1` to determine whether it should shoot a laser in the forward direction 
(that freezes the opposing agent) in the current time step.

## Running a pre-trained model

I have included pre-trained models for my agents (`.onnx` files) and I use the
Unity Inference Engine to run these models inside Unity. In this section, I will 
use the pre-trained model for the MAPOCA Agent example.

1. In the **Project** window, go to the
   `Assets/ML-Agents/Examples/MAPOCA Agent/Prefabs` folder. Click on the `PocaArea` 
   prefab. You should see the `PocaArea` prefab in the **Inspector** window.

   **Note**: The platforms in the `MAPOCA Area` scene were created using the `PocaArea`
   prefab. Instead of updating all 12 platforms individually, you can update the
   `PocaArea` prefab instead.

1. In the **Project** window, drag the **Poca Collector-10000068** Model located in
   `Assets/ML-Agents/Examples/MAPOCA Agent/TFModels` into the `Model` property under
   `Behavior Parameters (Script)` component in the Agent GameObject `Agent Blue` and
   `Agent Red`'s **Inspector** window. The number `10000068` means that the neural
   network has been trained for a total of about 10 million time steps.


1. You should notice that each `Agent Blue` and `Agent Red` under each `PocaArea` in the **Hierarchy**
   windows now contains **Poca Collector-10000068** as `Model` on the `Behavior Parameters`.
   **Note** : You can modify multiple game objects in a scene by selecting them
   all at once using the search bar in the Scene Hierarchy.
1. Set the **Inference Device** to use for this model as `CPU`.
1. Click the **Play** button in the Unity Editor and you will see the agents working
   together to play the game optimally.

## Training a new model with Reinforcement Learning

While I provide pre-trained models for the agents in this environment, any
environment you make yourself will require training agents from scratch to
generate a new model file. In this section I will demonstrate how to use the
reinforcement learning algorithms that are part of the ML-Agents Python package
to accomplish this. Unity has provided a convenient command `mlagents-learn` which
accepts arguments used to configure both training and inference phases.

### Training the environment

1. Open a command or terminal window.
1. Navigate to the folder where you cloned my `cooperative-agents` repository. 
   **Note**: If you followed the default installation, then you should
   be able to run `mlagents-learn` from any directory.
1. Run `mlagents-learn config/poca/PocaAgent.yaml --run-id=PocaAgentV1`.
   - `config/poca/PocaAgent.yaml` is the path to a default training
     configuration file that I have provide. The `config/poca` folder includes training configuration
     files for many of Unity's example environments.
   - `run-id` is a unique name for this training session.
1. When the message _"Start training by pressing the Play button in the Unity
   Editor"_ is displayed on the screen, you can press the **Play** button in
   Unity to start training in the Editor.

If `mlagents-learn` runs correctly and starts training, you should see something
like this:

```console
[INFO] Connected to Unity environment with package version 2.0.0-exp.1 and communication version 1.5.0
[INFO] Connected new brain: PocaCollector?team=0
[INFO] Hyperparameters for behavior name PocaCollector:
        trainer_type:   poca
        hyperparameters:
          batch_size:   2048
          buffer_size:  20480
          learning_rate:        0.0003
          beta: 0.01
          epsilon:      0.1
          lambd:        0.95
          num_epoch:    3
          learning_rate_schedule:       linear
        network_settings:
          normalize:    False
          hidden_units: 512
          num_layers:   2
          vis_encode_type:      simple
          memory:       None
          goal_conditioning_type:       hyper
        reward_signals:
          extrinsic:
            gamma:      0.995
            strength:   1.0
            network_settings:
              normalize:        False
              hidden_units:     128
              num_layers:       2
              vis_encode_type:  simple
              memory:   None
              goal_conditioning_type:   hyper
        init_path:      None
        keep_checkpoints:       5
        checkpoint_interval:    2000000
        max_steps:      10000000
        time_horizon:   64
        summary_freq:   50000
        threaded:       False
        self_play:      None
        behavioral_cloning:     None

[INFO] PocaCollector. Step: 50000. Time Elapsed: 171.361 s. Mean Reward: 0.000. Mean Group Reward: 0.875. Training.
[INFO] PocaCollector. Step: 100000. Time Elapsed: 337.445 s. Mean Reward: 0.000. Mean Group Reward: 2.938. Training.
[INFO] PocaCollector. Step: 150000. Time Elapsed: 551.020 s. Mean Reward: 0.000. Mean Group Reward: 0.625. Training.
[INFO] PocaCollector. Step: 200000. Time Elapsed: 733.952 s. Mean Reward: 0.000. Mean Group Reward: 4.688. Training.
[INFO] PocaCollector. Step: 250000. Time Elapsed: 897.273 s. Mean Reward: 0.000. Mean Group Reward: 4.750. Training.
[INFO] PocaCollector. Step: 300000. Time Elapsed: 1124.174 s. Mean Reward: 0.000. Mean Group Reward: 6.875. Training.
[INFO] PocaCollector. Step: 350000. Time Elapsed: 1296.552 s. Mean Reward: 0.000. Mean Group Reward: 8.750. Training.
```

Note how the `Mean Reward` value printed to the screen increases as training
progresses. This is a positive sign that training is succeeding.

### Observing Training Progress

Once you start training using `mlagents-learn` in the way described in the
previous section, the `ml-agents` directory will contain a `results`
directory. In order to observe the training process in more detail, you can use
TensorBoard. From the command line run:

```sh
tensorboard --logdir results
```

Then navigate to `localhost:6006` in your browser to view the TensorBoard
summary statistics as shown below. For the purposes of this section, the most
important statistic is `Environment/Group Cumulative Reward` which should increase
throughout training, eventually converging close to about `100` which is the maximum
reward the agent can accumulate.

## Embedding the model into the Unity Environment

Once the training process completes, and the training process saves the model
(denoted by the `Saved Model` message) you can add it to the Unity project and
use it with compatible Agents (the Agents that generated the model). **Note:**
Do not just close the Unity Window once the `Saved Model` message appears.
Either wait for the training process to close the window or press `Ctrl+C` at
the command-line prompt. If you close the window manually, the `.onnx` file
containing the trained model is not exported into the ml-agents folder.

If you've quit the training early using `Ctrl+C` and want to resume training,
run the same command again, appending the `--resume` flag:

```sh
mlagents-learn config/poca/PocaAgent.yaml --run-id=PocaAgentV1 --resume
```

Your trained model will be at `results/<run-identifier>/<behavior_name>.onnx` where
`<behavior_name>` is the name of the `Behavior Name` of the agents corresponding
to the model. This file corresponds to your model's latest checkpoint. You can
now embed this trained model into your Agents by following the steps below,
which is similar to the steps described [above](#running-a-pre-trained-model).

1. Move your model file into
   `Project/Assets/ML-Agents/Examples/MAPOCA Agent/TFModels/`.
1. Open the Unity Editor, and select the **POCA Agent** scene as described above.
1. Select the **PocaArea** prefab Agent object.
1. Drag the `<behavior_name>.onnx` file from the Project window of the Editor to
   the **Model** placeholder in the **Agent Blue** and **Agent Red** inspector window.
1. Press the **Play** button at the top of the Editor.
