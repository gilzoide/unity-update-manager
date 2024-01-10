# Changelog
## [Unreleased](https://github.com/gilzoide/unity-update-manager/compare/1.5.1...HEAD)
### Changed
- `UpdateManager` now disables itself while no updatable objects are registered.
- `UpdateManager` is now supported in edit mode.
  The singleton update manager created in edit mode will be destroyed when entering play mode.
- Enabled `AManagedBehaviour` objects with `[ExecuteInEditMode]` or `[ExecuteAlways]` attributes have their `ManagedUpdate` and `ManagedLateUpdate` methods run in edit mode.


## [1.5.1](https://github.com/gilzoide/unity-update-manager/releases/tag/1.5.1)
### Added
- Support for editing multiple objects in `AJobBehaviourEditor`


## [1.5.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.5.0)
### Added
- `IBurstUpdateJob<>` and `IBurstUpdateTransformJob<>` interfaces to be used in place of `IUpdateJob` and `IUpdateTransformJob` when Burst compilation is desired.
  Pass `BurstUpdateJob<...>` and `BurstUpdateTransformJob<...>` to their type parameters, so that Burst can compile the concrete job types.

### Deprecated
- `AJobBehaviour<,>`, `IJobUpdatable<,>` and `ITransformJobUpdatable<,>`.
  Use the versions with a single type parameter instead.
  Now that Burst support was moved to `IBurstUpdateJob<>` and `IBurstUpdateTransformJob<>`, specified at job struct definition, there is no need to specify the second parameter in job updatable types.


## [1.4.1](https://github.com/gilzoide/unity-update-manager/releases/tag/1.4.1)
### Fixed
- Removed usage of `ConditionalWeakTable` from `ProfilerMarkerMap` implementation, which broke iOS + IL2CPP builds for some reason.


## [1.4.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.4.0)
### Added
- `BurstUpdateJob<>`/`BurstUpdateTransformJob<>`: Burst-compiled versions of `UpdateJob<>`/`UpdateTransformJob<>`
- Update job managers now support job data synchronization.
  Implement `IJobDataSynchronizer<>` in job providers to synchronize data from registered jobs.
  Data synchronization occurs right after completing jobs, but before scheduling the new batch, so it is safe to modify their data.
  One must explicitly register objects for data synchronization by calling `SynchronizeJobDataOnce` for a one-time data synchronization next frame, or by passing true to `RegisterInManager` to sync data every frame.

### Changed
- `UpdateJob<>`/`UpdateTransformJob<>` are not marked for compilation with Burst anymore.
  This avoids warnings about the job being marked for compilation, but no concrete version of the type being defined.
  Use `BurstUpdateJob<>`/`BurstUpdateTransformJob<>` instead if you want Burst-compiled update jobs.
- `AUpdateJobManager.IsRegistered` now checks for pending additions/removals.
  It now returns true if provider is already pending addition and returns false if provider is present in map, but also pending removal.


## [1.3.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.3.0)
### Added
- [Follow Target](Samples~/FollowTarget) sample.
- `AUpdateJobManager.IsRegistered` method to check if an object is registered for updates.
  Extension methods `IJobUpdatable.IsRegisteredInManager` and `ITransformJobUpdatable.IsRegisteredInManager` were also added.
- Profiler markers for methods managed by `UpdateManager`.

### Changed
- Only reallocate job data arrays when growing their capacity.
  This avoids CPU spikes when rapidly removing/adding objects in update job managers.
- Hold Burst shared static in a variable instead of calling `GetOrCreate` everytime `UpdateJobTime` is accessed.
- Use a custom `SortedList` implementation instead of `SortedSet`.
  This avoids heap allocations while iterating over the values.

### Fixed
- Dispose of dependency job handles array before reallocating it.
- Reset job data when removing and re-adding an object in the same frame.
  This avoids maintaining the wrong state between registrations.


## [1.2.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.2.0)
### Added
- Support for managed `LateUpdate` and `FixedUpdate` methods in `UpdateManager`.
  Similar to managed `Update` methods, all you need to do is inherit the `ILateUpdatable` or `IFixedUpdatable` interface and register the object in the manager using the `RegisterInManager()` extension method.

### Deprecated
- `AUpdateManagerBehaviour`: Prefer inheriting `AManagedBehaviour` and implementing the `IUpdatable`/`ILateUpdatable`/`IFixedUpdatable` interfaces directly.

### Fixed
- `UpdateJobTime.frameCount` type to be `int` instead of `float`.
  This makes it compatible with `UnityEngine.Time.frameCount`.


## [1.1.2](https://github.com/gilzoide/unity-update-manager/releases/tag/1.1.2)
### Changed
- `UpdateManager` now uses try/catch blocks for each `ManagedUpdate` call.
  This way, if any managed update call fails, the other ones are not affected.


## [1.1.1](https://github.com/gilzoide/unity-update-manager/releases/tag/1.1.1)
### Changed
- `UpdateJob<>` and `UpdateTransformJob<>` are now marked for Burst compilation.
  Simply use concrete types and Burst will compile the jobs.
- `AJobBehaviour<,>` now inherits `AJobBehaviour<>` and not the other way around.
  The same is true for `IJobUpdatable<,>` and `ITransformJobUpdatable<,>`.

### Removed
- `BurstUpdateJob<>` and `BurstUpdateTransformJob<>` types.
  Simply use `UpdateJob<>` and `UpdateTransformJob<>` instead.
- `UpdateJobManager<,>` and `UpdateTransformJobManager<,>`, which are not necessary anymore.

### Fixed
- Managed jobs with dependencies now work for Burst-compiled jobs.


## [1.1.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.1.0)
### Added
- `AJobBehaviour` custom editor that shows current job data in a JSON format while inspecting the object.
- Added support for Burst-compiled jobs by specifying `BurstUpdateJob<>` or `BurstUpdateTransformJob<>` as the second type parameter to the generic interfaces `IJobUpdatable<,>` and `ITransformJobUpdatable<,>` as well as `AJobBehaviour<,>`.
- Added support for dependencies between managed jobs by using the `[DependsOn(...)]` attribute.
  For now, no dependency cycle detection is performed, so job runners may get deadlocked if you misuse it.
- Added support for native collections as fields in managed jobs.
  For now, the thread safety system provided by Unity is not applied to managed jobs native containers, so use them with care!
- `[JobBatchSize(...)]` attribute for specifying the parallel job batch sizes for each job type.
- `[ReadOnlyTransformAccess]` attribute for marking `IUpdateTransformJob`s with read-only transform access.
- `UpdateJobTime` now has static properties with names equal to the ones in `UnityEngine.Time`, no need to access using `UpdateJobTime.Instance` anymore.

### Deprecated
- Deprecated the `[UpdateJobOptions(...)]` attribute, use `[JobBatchSize(...)]` and `[ReadOnlyTransformAccess]` instead.
  They are clearer to read in code.

### Changed
- Deduplicated code in `UpdateJobManager` and `UpdateTransformJobManager` by using the common base class `AUpdateJobManager`.
- `UpdateManager.Unregister` complexity is now O(1) instead of O(N).
  This is due to using an internal Dictionary for caching objects' indices and always removing objects with swap back, the same technique used by `AUpdateJobManager`.
- `UpdateJobTime` is now a `struct` instead of `class` and its usage is now supported in Burst-compiled jobs.

### Fixed
- Calling `UpdateManager.Register` with an already registered object is now a no-op instead of registering duplicated entries.


## [1.0.0](https://github.com/gilzoide/unity-update-manager/releases/tag/1.0.0)
### Added
- `UpdateManager` singleton class that runs managed update methods in registered objects.
  Supports any C# object that implements `IUpdatable`, including MonoBehaviours, pure C# classes and structs.
- `AUpdateManagerBehaviour`, a MonoBehaviour abstract subclass that automatically registers/unregisters itself in `UpdateManager`.
- `UpdateJobManager<>` singleton class that schedules jobs with data provided by registered objects.
  Supports any C# object that implements `IJobUpdatable<>`.
- `UpdateTransformJobManager<>` singleton class that schedules `TransformAccess`-enabled jobs with data provided by registered objects.
  Supports any C# object that implements `ITransformJobUpdatable<>`.
- `UpdateJobTime` singleton class for accessing `UnityEngine.Time` properties from jobs.
- `AJobBehaviour<>`, a MonoBehaviour abstract subclass that automatically registers/unregisters itself in `UpdateTransformJobManager<>`.
- `[UpdateJobOptions(...)]` attribute that specifies parallel job batch sizes and whether transforms are read-only.