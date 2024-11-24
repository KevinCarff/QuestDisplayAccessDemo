using AprilTag;
using System.Collections.Generic;
using UnityEngine;

namespace Trev3d.Quest.AprilTags
{
    public class TagFollowerMover : MonoBehaviour
    {
        public static TagFollowerMover Instance { get; private set; }
        public Dictionary<int, TagFollower> allFollowers = new();
        public float lerpSpeed = 5f; // Speed for position interpolation
        public float slerpSpeed = 5f; // Speed for rotation interpolation

        // Store target positions and rotations for each tag
        private Dictionary<int, Vector3> targetPositions = new();
        private Dictionary<int, Quaternion> targetRotations = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable() => QuestAprilTagTracker.Instance.OnDetectTags += OnDetectTags;
        private void OnDisable() => QuestAprilTagTracker.Instance.OnDetectTags -= OnDetectTags;

        private void OnDetectTags(IEnumerable<TagPose> tagPoses)
        {
            foreach (TagPose tagPose in tagPoses)
            {
                Debug.Log("Found tag: " + tagPose.ID);

                if (!allFollowers.ContainsKey(tagPose.ID))
                    continue;

                // Set target position and rotation for each detected tag
                targetPositions[tagPose.ID] = tagPose.Position;
                targetRotations[tagPose.ID] = tagPose.Rotation;
            }
        }

        private void FixedUpdate()
        {
            // Continuously move each follower towards its target position and rotation
            foreach (var entry in allFollowers)
            {
                int tagID = entry.Key;
                TagFollower follower = entry.Value;

                // Check if there is a target position and rotation for this tag
                if (targetPositions.ContainsKey(tagID) && targetRotations.ContainsKey(tagID))
                {
                    // Lerp position
                    follower.transform.position = Vector3.Lerp(
                        follower.transform.position,
                        targetPositions[tagID],
                        Time.deltaTime * lerpSpeed
                    );

                    // Slerp rotation
                    follower.transform.rotation = Quaternion.Slerp(
                        follower.transform.rotation,
                        targetRotations[tagID],
                        Time.deltaTime * slerpSpeed
                    );
                }
            }
        }
    }
}
