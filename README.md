# Update Manager
Simple to use Update Manager pattern for Unity + Jobified Update for `MonoBehaviour`s and pure C# classes alike.

Using these may improve your game's CPU usage if there are thousands of objects updating every frame.

More info on Update Manager vs traditional Update: https://github.com/Menyus777/Game-engine-specific-optimization-techniques-for-Unity


## Features
- Use `UpdateManager` to call objects' `ManagedUpdate` method, bypassing Unity's native <-> C# interop
- Both `MonoBehaviour` and pure C# classes are supported, just implement `IUpdatable` interface and register the object to be updated using `UpdateManager.Instance.Register`.
  
  Remember to unregister the objects with `UpdateManager.Instance.Unregister` when necessary.
- Inherit `AUpdateManagerBehaviour` to automatically register/unregister MonoBehaviours in `UpdateManager` in their `OnEnable`/`OnDisable` messages
- Use `UpdateJobManager<>` to run jobs every frame using Unity's Job system
- Use `UpdateTransformJobManager<>` to run jobs with `TransformAccess` every frame using Unity's Job system, so you can change your objects' transforms from jobs
- Job data may be modified from within jobs and fetched anytime.
  
  This package uses double buffering to let you read values even while jobs are running and modifying data.
- Both `MonoBehaviour` and pure C# classes are supported, just implement `IJobUpdatable<>` or `ITransformJobUpdatable<>` interface and register the object to be updated using `UpdateJobManager<>.Instance.Register` or `UpdateTransformJobManager<>.Instance.Register`.
  
  Remember to unregister the objects with `UpdateJobManager<>.Instance.Unregister` or `UpdateTransformJobManager<>.Instance.Unregister` when necessary.
- Inherit `AJobBehaviour<>` to automatically register/unregister MonoBehaviours in `UpdateTransformJobManager<>` in their `OnEnable`/`OnDisable` messages
- `UpdateJobTime` singleton class with information from Unity's `Time` class that you can access from within jobs (`deltaTime`, `time`, etc...)
- Configurable job batch size using `[UpdateJobOptions(BatchSize = ...)]` attribute in job structs


## Caveats
- `UpdateManager` doesn't have the concept of script execution order like Unity MonoBehaviours, so don't rely on execution order
- `UpdateJobManager<>` and `UpdateTransformJobManager<>` use `NativeArray`s to manage all updatables' data.
  Since Unity doesn't support nested `NativeArray`s, you cannot use `NativeArray`s in managed jobs.
- Read-write transform jobs are only parallelized if the objects live in hierarchies with different root objects.
  This is a limitation of Unity's job system.

  Read-only transform jobs, marked by the `[UpdateJobOptions(ReadOnlyTransforms = true)]` attribute, don't have this restriction.


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
      public void ManagedUpdate()
      {
          Debug.Log("This will also be called every frame!");
      }

      // call this when you want Updates to start running
      public void StartUpdating()
      {
          UpdateManager.Instance.Register(this);
          // Alias: `this.RegisterInManager()`
      }

      // call this when necessary to stop the updates
      public void StopUpdating()
      {
          UpdateManager.Instance.Unregister(this);
          // Alias: `this.UnregisterInManager()`
      }
  }
  ```
- `UpdateTransformJobManager` + `MonoBehaviour`
  ```cs
  using System.Collections;
  using Gilzoide.UpdateManager.Jobs;
  using UnityEngine;
  using UnityEngine.Jobs;

  // 1. Create the Job struct
  public struct MyMoveJob : IUpdateTransformJob
  {
      public Vector3 Direction;
      public float Speed;
      public bool SomethingHappened;

      public void Execute(TransformAccess transform)
      {
          Debug.Log("This will be called every frame using Unity's Job system");
          // This runs outside of the Main Thread, so
          // we need to use `UpdateJobTime` instead of `Time`
          float deltaTime = UpdateJobTime.Instance.deltaTime;
          // You can modify the Transform in jobs!
          transform.localPosition += Direction * Speed * deltaTime;
          // You can modify the struct's value and fetch them later!
          SomethingHappened = true;
      }
  }

  // 2. Create the job-updated behaviour
  public class MyJobifiedBehaviour : AJobBehaviour<MyMoveJob>
  {
      // set the parameters in Unity's Inspector
      public Vector3 Direction;
      public float Speed;

      // (optional) Set the data passed to the first job run
      public override MyMoveJob InitialJobData => new MyMoveJob
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
  ```cs
  using Gilzoide.UpdateManager.Jobs;
  using UnityEngine;

  // 1. Create the Job struct
  public struct MyCountJob : IUpdateJob
  {
      public int Count;

      public void Execute()
      {
          Debug.Log("This will be called every frame using Unity's Job system");
          Count++;
      }
  }

  // 2. Create the job-updated class
  public class MyJobifiedBehaviour : IJobUpdatable<MyCountJob>
  {
      // Set the data passed to the first job run
      public MyCountJob InitialJobData => default;

      // call this when you want Updates to start running
      public void StartUpdating()
      {
          UpdateJobManager<MyCountJob>.Instance.Register(this);
          // Alias: `this.RegisterInManager()`
      }

      // call this when necessary to stop the updates
      public void StopUpdating()
      {
          UpdateJobManager<MyCountJob>.Instance.Unregister(this);
          // Alias: `this.UnregisterInManager()`
      }

      // fetch current data using `this.GetJobData`
      public int CurrentCount => this.GetJobData().Count;
  }
  ```


## Benchmarks
TODO