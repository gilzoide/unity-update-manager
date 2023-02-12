# Update Manager
Simple Update Manager pattern for Unity + simple to use Jobified Update for `MonoBehaviour`s and pure C# classes alike.

Using these may improve your game's CPU usage if you have thousands of objects updating every frame.

More info on Update Manager vs traditional Update: https://github.com/Menyus777/Game-engine-specific-optimization-techniques-for-Unity


## Features
- Use `UpdateManager` to call objects' `ManagedUpdate` method, bypassing Unity's native <-> C# interop
- Both `MonoBehaviour` and pure C# classes are supported, just implement `IUpdatable` interface and register the object to be updated using `UpdateManager.Instance.RegisterUpdatable`.
  
  Remember to unregister the objects with `UpdateManager.Instance.UnregisterUpdatable` when necessary.
- Inherit `AUpdateManagerBehaviour` to automatically register/unregister MonoBehaviours in `UpdateManager` in their `OnEnable`/`OnDisable` messages
- Use `UpdateJobManager` to run jobs every frame using Unity's Job system
- Jobs use `TransformAccess` so you can change your objects' transforms directly
- Inherit `AJobBehaviour` to automatically register/unregister MonoBehaviours in `UpdateJobManager` in their `OnEnable`/`OnDisable` messages
- Job data may be modified from within jobs and fetched later
- `UpdateJobTime` singleton class with information from Unity's `Time` class that you can access from within jobs (`deltaTime`, `time`, etc...)


## How to install
Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
with the following URL:

```
https://github.com/gilzoide/unity-update-manager.git
```


## How to use
- `UpdateManager` + `MonoBehaviour`:
  ```cs
  using Gilzoide.UpdateManager;
  using UnityEngine;

  public class MyManagedUpdatableBehaviour : AUpdateManagerBehaviour
  {
      public override void ManagedUpdate()
      {
          Debug.Log("This will be called every frame!");
      }
  }
  ```
- `UpdateManager` with pure C# class:
  ```cs
  using Gilzoide.UpdateManager;
  using UnityEngine;

  public class MyUpdatable : IUpdatable
  {
      public override void ManagedUpdate()
      {
          Debug.Log("This will also be called every frame!");
      }

      // call this when you want Updates to start running
      public void StartUpdating()
      {
          UpdateManager.Instance.RegisterUpdatable(this);
      }

      // call this when necessary to stop the updates
      public void StopUpdating()
      {
          UpdateManager.Instance.UnregisterUpdatable(this);
      }
  }
  ```
- `UpdateJobManager` + `MonoBehaviour`
  ```cs
  using Gilzoide.UpdateManager.Jobs;
  using UnityEngine;

  // 1. Create the Job struct
  public struct MyMoveJob : IUpdateJob
  {
      public Vector3 Direction;
      public float Speed;
      public bool SomethingHappened;

      public void Process(TransformAccess transform)
      {
          Debug.Log("This will be called every frame using Unity's Job system");
          // This runs outside of the Main Thread, so
          // we need to use `UpdateJobTime` instead of `Time`
          float deltaTime = UpdateJobTime.deltaTime;
          // You can modify the Transform in jobs!
          transform.localPosition += Direction * Speed * deltaTime;
          // You can modify the struct's value and fetch them later!
          SomethingHappened = true;
      }
  }

  public class MyJobifiedBehaviour : AJobBehaviour<MyMoveJob>
  {
      // set the parameters in Unity's Inspector
      public Vector3 Direction;
      public float Speed;

      // (optional) Set the data passed to the first job run
      public override RotateJob InitialJobData => new MyMoveJob
      {
          Direction = Direction,
          Speed = Speed,
      };

      IEnumerator Start()
      {
          // wait a frame to see if something happened
          yield return null;
          // use the `JobData` property to fetch the current data
          MyMoveJob currentData = JobData;
          // should print "Something happened: true"
          Debug.Log("Something happened: " + currentData.SomethingHappened);
      }
  }
  ```
- `UpdateJobManager` + pure C# class
  
  TODO


## Benchmarks
TODO