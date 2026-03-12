using System.Collections.Generic;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

namespace Runtime.ReactiveWorld.Partitions
{
    /// <summary>
    /// Manages the spatial partitioning of the world.
    /// Maintains two independent registries:
    /// <list type="bullet">
    ///   <item><description><c>_areas</c> — named <see cref="AreaVolume"/> components found in the scene.</description></item>
    ///   <item><description><c>_reactorsByAreas</c> — <see cref="IReactor"/> instances grouped by area ID.</description></item>
    /// </list>
    /// Populated by <see cref="WorldManager"/> during <c>Awake</c> and whenever a reactor
    /// that implements <see cref="IAreaReactor"/> registers itself.
    /// </summary>
    public class PartitionManager
    {
        /// <summary>All known area volumes, keyed by their <see cref="AreaVolume.AreaId"/>.</summary>
        private Dictionary<string, AreaVolume> _areas = new();

        /// <summary>Reactors grouped by the area ID they are bound to.</summary>
        private Dictionary<string, List<IReactor>> _reactorsByAreas = new();

        /// <summary>Shared empty list returned when an area has no reactors, avoids allocations.</summary>
        private static readonly IReadOnlyList<IReactor> _emptyReactors = new List<IReactor>();

        /// <summary>
        /// Registers an <see cref="AreaVolume"/> so it can be queried by ID or position.
        /// Called automatically by <see cref="WorldManager"/> during <c>Awake</c>.
        /// </summary>
        public void RegisterArea(AreaVolume area)
        {
            if (!_areas.ContainsKey(area.AreaId))
            {
                _areas[area.AreaId] = area;
                Debug.Log($"[PartitionManager] Area '{area.AreaId}' registered.");
            }
            else
            {
                Debug.LogWarning($"[PartitionManager] Area '{area.AreaId}' is already registered.");
            }
        }

        /// <summary>
        /// Removes an <see cref="AreaVolume"/> from the registry.
        /// Logs a warning if reactors are still bound to the area at the time of removal.
        /// Also removes the associated reactor list for that area.
        /// </summary>
        public void UnregisterArea(AreaVolume area)
        {
            if (_areas.ContainsKey(area.AreaId))
            {
                if (_reactorsByAreas.TryGetValue(area.AreaId, out var reactors) && reactors.Count > 0)
                    Debug.Log($"[PartitionManager] Area '{area.AreaId}' unregistered with {reactors.Count} reactor(s) still bound.");

                _reactorsByAreas.Remove(area.AreaId);
                _areas.Remove(area.AreaId);
                Debug.Log($"[PartitionManager] Area '{area.AreaId}' unregistered.");
            }
            else
            {
                Debug.LogWarning($"[PartitionManager] Tried to unregister area '{area.AreaId}', but it was not found.");
            }
        }


        /// <summary>
        /// Binds <paramref name="reactor"/> to the given <paramref name="areaId"/>.
        /// Called automatically by <see cref="WorldManager.Register"/> when the reactor
        /// implements <see cref="IAreaReactor"/>.
        /// </summary>
        public void Register(string areaId, IReactor reactor)
        {
            if (!_reactorsByAreas.ContainsKey(areaId))
                _reactorsByAreas[areaId] = new List<IReactor>();
            
            if (_reactorsByAreas[areaId].Contains(reactor))
            {
                Debug.LogWarning($"[PartitionManager] Reactor '{reactor.Name}' is already registered in area '{areaId}'.");
                return;
            }

            _reactorsByAreas[areaId].Add(reactor);

            Debug.Log($"[PartitionManager] Registered to '{areaId}' (reactor: {reactor.Name}).");
        }

        /// <summary>
        /// Removes <paramref name="reactor"/> from the given <paramref name="areaId"/>.
        /// Called automatically by <see cref="WorldManager.Unregister"/> when the reactor
        /// implements <see cref="IAreaReactor"/>.
        /// </summary>
        public void Unregister(string areaId, IReactor reactor)
        {
            if (_reactorsByAreas.ContainsKey(areaId))
            {
                _reactorsByAreas[areaId].Remove(reactor);

                Debug.Log($"[PartitionManager] Unregistered from '{areaId}' (reactor: {reactor.Name}).");
            }
            else
            {
                Debug.LogWarning($"[PartitionManager] Tried to unregister from '{areaId}', but no area were found.");    
            }
        }

        /// <summary>Returns the <see cref="AreaVolume"/> for the given <paramref name="areaId"/>, or <c>null</c> if not found.</summary>
        public AreaVolume GetArea(string areaId)
        {
            return _areas.GetValueOrDefault(areaId, null);
        }

        /// <summary>
        /// Returns all reactors bound to <paramref name="areaId"/>.
        /// Returns an empty read-only list (no allocation) if the area has no reactors.
        /// </summary>
        public IReadOnlyList<IReactor> GetReactors(string areaId)
        {
            return _reactorsByAreas.TryGetValue(areaId, out var reactors) ? reactors : _emptyReactors;
        }

        /// <summary>Returns <c>true</c> if an <see cref="AreaVolume"/> with <paramref name="areaId"/> is registered.</summary>
        public bool HasArea(string areaId)
        {
            return _areas.ContainsKey(areaId);
        }

        /// <summary>
        /// Finds the area whose collider contains <paramref name="position"/>.
        /// Uses <see cref="Collider.ClosestPoint"/> — a point is inside when the closest point equals itself.
        /// </summary>
        /// <param name="position">World-space position to test.</param>
        /// <param name="areaVolume">The matching area, or <c>null</c> if none.</param>
        /// <returns><c>true</c> if a containing area was found.</returns>
        public bool TryGetAreaAtPosition(Vector3 position, out AreaVolume areaVolume)
        {
            foreach (var area in _areas)
            {
                var volume = area.Value;
                if (volume.Volume == null) continue;

                if (volume.Volume.ClosestPoint(position) == position)
                {
                    areaVolume = volume;
                    return true;
                }
            }

            areaVolume = null;
            return false;
        }
    }
}