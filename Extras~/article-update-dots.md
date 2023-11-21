# Update objects using DOTS without ECS
[DOTS](https://unity.com/dots) - Unity's Data-Oriented Technology Stack - is a combination of technologies that enables building games with a data-oriented approach.
When used correctly, processing can be scaled in a highly performant manner, taking advantage of parallelized computations, compilation from C# to highly optimized native code using the [Burst](https://docs.unity3d.com/Packages/com.unity.burst@latest?subfolder=/manual/index.html) compiler, and more.

The official DOTS solution for executing update methods every frame is by using the [Entities](https://docs.unity3d.com/Packages/com.unity.entities@latest?subfolder=/manual/index.html) package - a framework that implements the [Entity Component System (ECS)](https://unity.com/ecs) architecture.
As described in the [ECS workflow](https://docs.unity3d.com/Packages/com.unity.entities@1.2/manual/ecs-workflow-intro.html) documentation, the default way of working with ECS is by creating a subscene, authoring normally with GameObjects and MonoBehaviours, then creating "baker" classes that will bake the regular components to ECS components.
After baking, there are no GameObjects and MonoBehaviours anymore, so lots of development workflows and packages that we are used to use in Unity are not supported when interfacing with ECS objects.

If you are not using ECS in your project yet, it might be very difficult to refactor your gameplay logic to fit the ECS framework.
Sometimes, all you want is to run your MonoBehaviour's Update logic in background threads in parallel, but still work with GameObjects.
For these cases, like moving hundreds of bullets forward in a Shoot 'em Up game, and lots of other ones, using the [Update Manager](https://github.com/gilzoide/unity-update-manager) package and its Job System support is much simpler than going the ECS route, mainly if you are working with components that already exist, rather then implementing them from scratch.


## Moving forward (using C# Jobs)
Using Update Jobs is quite simple.
After installing the [Update Manager](https://github.com/gilzoide/unity-update-manager) package, the basic workflow is:
1. Create a job struct type that implements `IUpdateTransformJob`.
   Transform jobs have access to the `Transform` being updated via a `TransformAccess` instance.
2. Inherit `AJobBehaviour<>` instead of `MonoBehaviour`: the only required override is the `InitialJobData` property

First, let's create a job that moves an object forward:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

// 1. Create a job struct type that implements `IUpdateTransformJob`
public struct MoveForwardJob : IUpdateTransformJob
{
    // 2. (optional) Define any parameters you may need
    public float Speed;

    // 3. Implement the `Execute` method 
    public void Execute(TransformAccess transformAccess)
    {
        // Background threads cannot access Time.deltaTime,
        // so we need to use use `UpdateJobTime.deltaTime` instead
        float deltaTime = UpdateJobTime.deltaTime;

        // TransformAccess doesn't have the Translate method,
        // so we need to update its `position` directly
        Vector3 direction = transformAccess.rotation * Vector3.forward;
        transformAccess.position += Speed * deltaTime * direction;
    }
}
```

Last, but not least, let's define a component that runs our `MoveForwardJob` every frame as its update method:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

// 1. Inherit `AJobBehaviour<>` and your component will
// automatically update using the Job System while enabled.
public class MoveForwardComponent : AJobBehaviour<MoveForwardJob>
{
    // 2. (optional) Declare any fields you may need
    public float Speed = 1;

    // 3. Define the job data used when initializing a new job
    public override MoveForwardJob InitialJobData => new MoveForwardJob
    {
        Speed = Speed,
    };
}
```

That's it, the basic workflow is done: when `MoveForwardComponent` is enabled, it will be updated every frame using Unity's C# Job System.
When disabled, the update job will stop running.
Attach this component to a GameObject in your scene, enter Play Mode and you should see it moving forward.


## Parallelization
The Update Manager package runs all instances of your job struct at the same time in a single parallel job.
There is no guarantee that the Job System will actually execute them in parallel, as there are a limited number of preallocated threads for running jobs.
Also, depending on the number of objects, it is possible that all of them end up being processed by a single thread in a single batch.

For jobs with a `TransformAccess`, if all of the transforms are in different hierarchies, that is, they do not share a common root transform, the jobs may always run in parallel.
If you only read values from the `TransformAccess` but never modify them, you can mark the job struct type with the `[ReadOnlyTransformAccess]` attribute, so that even transforms in the same hierarchy may be processed in parallel as well.


## Synchronizing data
Now that our update job is a different instance in memory than the MonoBehaviour that created it, their data might desynchronize.
For example, if we change the value of the `Speed` variable from a `MoveForwardComponent` that is already enabled and running, the job will still use the old speed by default.

To be able to synchronize data between update jobs and their corresponding updatable instance, we need to implement the `IJobDataSynchronizer<>` interface.
This interface declares a single method, `void SyncJobData(ref T jobData)`.
Notice that the parameter has `ref T` type, which means that we can modify the job data, even if it is of a struct type.

Let's implement data synchronization in our `MoveForwardComponent`, so that it writes the new value of Speed when we update it in the inspector:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

public class MoveForwardComponent : AJobBehaviour<MoveForwardJob>,
    // 1. Declare that we implement IJobDataSynchronizer<>
    IJobDataSynchronizer<MoveForwardJob>
{
    public float Speed = 1;

    public override MoveForwardJob InitialJobData => new MoveForwardJob
    {
        Speed = Speed,
    };

    // 2. Implement data synchronization
    // In this case, just copy the value of `Speed`
    public void SyncJobData(ref MoveForwardJob jobData)
    {
        jobData.Speed = Speed;
    }
}
```

All right, that's it.
Now, the object will synchronize its job data automatically whenever we modify the `Speed` value in the inspector, using its `OnValidate` message.
To trigger a manual data synchronization by code, just call `this.SynchronizeJobDataOnce()`.
If you want data synchronization to be called every frame, call `this.RegisterInManager(true)` instead.

For example, to modify `Speed` in code while the job is running, you can use the following `SetSpeed` method:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

public class MoveForwardComponent : AJobBehaviour<MoveForwardJob>,
    IJobDataSynchronizer<MoveForwardJob>
{
    public float Speed = 1;

    public override MoveForwardJob InitialJobData => new MoveForwardJob
    {
        Speed = Speed,
    };

    public void SyncJobData(ref MoveForwardJob jobData)
    {
        jobData.Speed = Speed;
    }

    // Sets a new value for `Speed`
    // If the value is different from previous one, synchronize job data
    public void SetSpeed(float speed)
    {
        if (speed != Speed)
        {
            Speed = speed;
            this.SynchronizeJobDataOnce();
        }
    }
}
```


## Using Burst
To use the Burst compiler and have update jobs compiled to highly optimized native code, all we need to do is change the job struct definition to inherit `IBurstUpdateTransformJob<>` instead of `IUpdateTransformJob`:

```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct MoveForwardJob : IBurstUpdateTransformJob<BurstUpdateTransformJob<MoveForwardJob>>
                       // here ^
{
    public float Speed;

    public void Execute(TransformAccess transformAccess)
    {
        float deltaTime = UpdateJobTime.deltaTime;
        Vector3 direction = transformAccess.rotation * Vector3.forward;
        transformAccess.position += Speed * deltaTime * direction;
    }
}
```

Notice that it is required to actually write `BurstUpdateTransformJob<MoveForwardJob>` in the project's code for the Burst compiler to be able to compile the job.
This is why the interface is `IBurstUpdateTransformJob<>` instead of `IBurstUpdateTransformJob`, just to fulfill this formality.


## Other nice features
The Update Manager has other nice features related to DOTS/C# Jobs:
- Any C# class can be updated using jobs, it doesn't need to be a MonoBehaviour.
  Implement `IJobUpdatable` or `ITransformJobUpdatable`, start running updates with `this.RegisterInManager()` and that's it.
  Don't forget to stop running updates with `this.UnregisterInManager()` when necessary.
- You can implement jobs without a `TransformAccess`, just implement `IUpdateJob` instead of `IUpdateTransformJob`.
- You can define dependencies between job types, so that one job only executes after the other has finished.
  Check out the [Follow Target sample](../Samples~/FollowTarget/) for an example of usage.

For an example of pure C# classes being updated every frame with jobs without any `TransformAccess`, check out the [Tween Jobs](https://github.com/gilzoide/unity-tween-jobs) package.