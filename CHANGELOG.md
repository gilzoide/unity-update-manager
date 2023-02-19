# Changelog
## [Unreleased](https://github.com/gilzoide/unity-update-manager/compare/1.0.0...HEAD)
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