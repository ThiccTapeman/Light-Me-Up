using UnityEngine;

namespace Runtime.ReactiveWorld.Partitions
{
    /// <summary>
    /// Scene component that defines a named physical area in the world.
    /// Place this on any GameObject with a <see cref="Collider"/> to declare an area boundary.
    /// The <see cref="PartitionManager"/> uses the collider to determine which area
    /// a given world-space position falls into via <see cref="PartitionManager.TryGetAreaAtPosition"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AreaVolume : MonoBehaviour
    {
        /// <summary>Unique identifier for this area, set in the Inspector.</summary>
        [SerializeField] private string areaId;

        /// <summary>The collider that defines the physical bounds of this area.</summary>
        [SerializeField] private Collider volume;

        /// <summary>Unique identifier for this area.</summary>
        public string AreaId => areaId;

        /// <summary>The collider that defines the physical bounds of this area.</summary>
        public Collider Volume => volume;
    }
}

