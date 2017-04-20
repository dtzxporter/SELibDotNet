using SELib.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///
///   SEAnim.cs
///   Author: DTZxPorter
///   Written for the SE Format Project
///   Follows SEAnim specification v1.0.1
///   https://github.com/SE2Dev/SEAnim-Docs/blob/master/spec.md
///

namespace SELib
{
    #region Math Containers

    /// <summary>
    /// Generic interface for all key data
    /// </summary>
    public interface KeyData
    {
        // A generic interface for a container
    }

    /// <summary>
    /// A container for a vector (Normalized)
    /// </summary>
    public class Vector3 : KeyData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    /// A container for a quaternion rotation (Normalized)
    /// </summary>
    public class Quaternion : Vector3
    {
        public double W { get; set; }
    }

    #endregion

    #region SEAnim Enums

    /// <summary>
    /// Specifies how the data is interpreted by the importer
    /// </summary>
    public enum AnimationType : int
    {
        /// <summary>
        /// Animation translations are set to this exact value each frame
        /// </summary>
        Absolute = 0,
        /// <summary>
        /// This animation is applied to existing animation data in the scene
        /// </summary>
        Additive = 1,
        /// <summary>
        /// Animation translations are based on rest position (Frame 0)
        /// </summary>
        Relative = 2,
        /// <summary>
        /// This animation is relative and contains delta data (Whole model movement) Delta tag name must be set!
        /// </summary>
        Delta = 3
    }

    /// <summary>
    /// Specifies the data present for each frame of every bone (Internal use only, matches specification v1.0.1)
    /// </summary>
    internal enum DataPresenceFlags : byte
    {
        // These describe what type of keyframe data is present for the bones
        SEANIM_BONE_LOC = 1 << 0,
        SEANIM_BONE_ROT = 1 << 1,
        SEANIM_BONE_SCALE = 1 << 2,

        // If any of the above flags are set, then bone keyframe data is present, thus this comparing against this mask will return true
        SEANIM_PRESENCE_BONE = SEANIM_BONE_LOC | SEANIM_BONE_ROT | SEANIM_BONE_SCALE,

        // The file contains notetrack data
        SEANIM_PRESENCE_NOTE = 1 << 6,
        // The file contains a custom data block
        SEANIM_PRESENCE_CUSTOM = 1 << 7,
    }

    #endregion

    #region SEAnim Key

    /// <summary>
    /// Contains information for a specific keyframe
    /// </summary>
    public class SEAnimFrame
    {
        /// <summary>
        /// Get or set the frame of this animation key
        /// </summary>
        public int Frame { get; set; }
        /// <summary>
        /// Get or set the frame data for this animation key
        /// </summary>
        public KeyData Data { get; set; }
    }

    #endregion

    /// <summary>
    /// Represents a SEAnim file, allows for reading and writing animation data
    /// </summary>
    public class SEAnim
    {

        /* Animation key data */

        /// <summary>
        /// A list of animation keys, by bone, for positions
        /// </summary>
        private Dictionary<string, List<SEAnimFrame>> AnimationPositionKeys;
        /// <summary>
        /// A list of animation keys, by bone, for rotations
        /// </summary>
        private Dictionary<string, List<SEAnimFrame>> AnimationRotationKeys;
        /// <summary>
        /// A list of animation keys, by bone, for scales
        /// </summary>
        private Dictionary<string, List<SEAnimFrame>> AnimationScaleKeys;
        /// <summary>
        /// A list of animation keys, for notetracks
        /// </summary>
        private Dictionary<string, List<SEAnimFrame>> AnimationNotetracks;
        /// <summary>
        /// A list of animation modifiers, by bone
        /// </summary>
        private Dictionary<string, AnimationType> AnimationBoneModifiers;

        /* Animation properties */

        /// <summary>
        /// The count of frames in the animation, this is automatically updated
        /// </summary>
        public int FrameCount { get { return CalculateFrameCount(); } }
        /// <summary>
        /// The count of bones in the animation, this is automatically updated
        /// </summary>
        public int BoneCount { get { return CalculateBoneCount(); } }
        /// <summary>
        /// The count of notifications in the animation, this is automatically updated
        /// </summary>
        public int NotificationCount { get { return CalculateNotetracksCount(); } }
        /// <summary>
        /// The animation type for this animation
        /// </summary>
        public AnimationType AnimType { get; set; }
        /// <summary>
        /// Whether or not the animation should loop
        /// </summary>
        public bool Looping { get; set; }
        /// <summary>
        /// The name of the delta tag of which to treat as the delta bone (If any)
        /// </summary>
        public string DeltaTagName;
        /// <summary>
        /// The framerate of the animation as a float (Defaults to 30.0)
        /// </summary>
        public float FrameRate { get; set; }
        
        /// <summary>
        /// Creates a new SEAnim using default settings
        /// </summary>
        public SEAnim()
        {
            // Setup defaults
            AnimationPositionKeys = new Dictionary<string, List<SEAnimFrame>>();
            AnimationRotationKeys = new Dictionary<string, List<SEAnimFrame>>();
            AnimationScaleKeys = new Dictionary<string, List<SEAnimFrame>>();
            AnimationNotetracks = new Dictionary<string, List<SEAnimFrame>>();
            AnimationBoneModifiers = new Dictionary<string, AnimationType>();
            // Default type
            AnimType = AnimationType.Absolute;
            // Non-looping
            Looping = false;
            // No delta
            DeltaTagName = string.Empty;
            // Frame fps
            FrameRate = 30.0f;
        }

        #region Writing

        /// <summary>
        /// Saves the SEAnim to a file (Overwriting if exists), following the current specification version, using the provided data
        /// </summary>
        /// <param name="FileName">The file name to save the animation to</param>
        /// <param name="HighPrecision">Whether or not to use doubles or floats (Defaults to floats)</param>
        public void Write(string FileName, bool HighPrecision = false)
        {
            // Open up a binary writer
            using (ExtendedBinaryWriter writeFile = new ExtendedBinaryWriter(File.Create(FileName)))
            {
                // Write magic
                writeFile.Write(new char[] { 'S', 'E', 'A', 'n', 'i', 'm' });
                // Write version
                writeFile.Write((short)0x1);
                // Write header size
                writeFile.Write((short)0x1C);
                // Write animation type
                writeFile.Write((byte)AnimType);
                // Write flags (Looped is the only flag for now)
                writeFile.Write((byte)(Looping ? (1 << 0) : 0));
                // Build data present flags
                {
                    // Buffer
                    byte DataPresentFlags = 0x0;
                    // Check for translations
                    if (AnimationPositionKeys.Count > 0)
                    {
                        DataPresentFlags |= (byte)DataPresenceFlags.SEANIM_BONE_LOC;
                    }
                    // Check for rotations
                    if (AnimationRotationKeys.Count > 0)
                    {
                        DataPresentFlags |= (byte)DataPresenceFlags.SEANIM_BONE_ROT;
                    }
                    // Check for scales
                    if (AnimationScaleKeys.Count > 0)
                    {
                        DataPresentFlags |= (byte)DataPresenceFlags.SEANIM_BONE_SCALE;
                    }
                    // Check for notetracks
                    if (AnimationNotetracks.Count > 0)
                    {
                        DataPresentFlags |= (byte)DataPresenceFlags.SEANIM_PRESENCE_NOTE;
                    }
                    // Write it
                    writeFile.Write((byte)DataPresentFlags);
                }
                // Write data property flags (Precision is the only one for now)
                writeFile.Write((byte)(HighPrecision ? (1 << 0) : 0));
                // Write reserved bytes
                writeFile.Write(new byte[2] { 0x0, 0x0 });
                // Write framerate
                writeFile.Write((float)FrameRate);
                // The framecount buffer
                int FrameCountBuffer = FrameCount;
                // The bonecount buffer
                int BoneCountBuffer = BoneCount;
                // The notification buffer
                int NotificationBuffer = NotificationCount;
                // Write count of frames
                writeFile.Write((int)FrameCountBuffer);
                // Write count of bones
                writeFile.Write((int)BoneCountBuffer);
                // Write modifier count
                writeFile.Write((byte)AnimationBoneModifiers.Count);
                // Write 3 reserved bytes
                writeFile.Write(new byte[3] { 0x0, 0x0, 0x0 });
                // Write notification count
                writeFile.Write((int)NotificationBuffer);
                // Build unique tag data, in the order we need
                {
                    // Get the bone tags in the order we need them
                    List<string> BoneTags = BuildUniqueOrderedBoneMap();
                    // Loop and write tags
                    foreach (string Tag in BoneTags)
                    {
                        // Write
                        writeFile.WriteNullTermString(Tag);
                    }
                    // Loop through modifiers
                    foreach (KeyValuePair<string, AnimationType> Modifier in AnimationBoneModifiers)
                    {
                        // Write the modifier
                        if (BoneCountBuffer <= 0xFF)
                        {
                            // Write as byte
                            writeFile.Write((byte)BoneTags.IndexOf(Modifier.Key));
                        }
                        else
                        {
                            // Write as short
                            writeFile.Write((short)BoneTags.IndexOf(Modifier.Key));
                        }
                        // Write modifier value
                        writeFile.Write((byte)Modifier.Value);
                    }
                    // We must write the key info in the order of bonetags
                    foreach (string Bone in BoneTags)
                    {
                        // Write bone flags (0 for now)
                        writeFile.Write((byte)0x0);
                        // Write translation keys first (if we have any)
                        #region Translation Keys

                        {
                            // Check if we have any
                            if (AnimationPositionKeys.Count > 0)
                            {
                                // Check if this bone has any translations
                                if (AnimationPositionKeys.ContainsKey(Bone))
                                {
                                    // Check length of frames to write count
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)AnimationPositionKeys[Bone].Count);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)AnimationPositionKeys[Bone].Count);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)AnimationPositionKeys[Bone].Count);
                                    }
                                    // Output keys
                                    foreach (SEAnimFrame Key in AnimationPositionKeys[Bone])
                                    {
                                        // Output frame number based on frame count
                                        if ((FrameCountBuffer - 1) <= 0xFF)
                                        {
                                            // Write as byte
                                            writeFile.Write((byte)Key.Frame);
                                        }
                                        else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                        {
                                            // Write as short
                                            writeFile.Write((short)Key.Frame);
                                        }
                                        else
                                        {
                                            // Write as int
                                            writeFile.Write((int)Key.Frame);
                                        }
                                        // Output the vector
                                        if (HighPrecision)
                                        {
                                            writeFile.Write((double)((Vector3)Key.Data).X);
                                            writeFile.Write((double)((Vector3)Key.Data).Y);
                                            writeFile.Write((double)((Vector3)Key.Data).Z);
                                        }
                                        else
                                        {
                                            writeFile.Write((float)((Vector3)Key.Data).X);
                                            writeFile.Write((float)((Vector3)Key.Data).Y);
                                            writeFile.Write((float)((Vector3)Key.Data).Z);
                                        }
                                    }
                                }
                                else
                                {
                                    // Check length of frames to write count (of 0 because no frames)
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)0x0);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)0x0);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)0x0);
                                    }
                                }
                            }
                        }

                        #endregion
                        // Write rotation keys next (if we have any)
                        #region Rotation Keys

                        {
                            // Check if we have any
                            if (AnimationRotationKeys.Count > 0)
                            {
                                // Check if this bone has any rotations
                                if (AnimationRotationKeys.ContainsKey(Bone))
                                {
                                    // Check length of frames to write count
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)AnimationRotationKeys[Bone].Count);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)AnimationRotationKeys[Bone].Count);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)AnimationRotationKeys[Bone].Count);
                                    }
                                    // Output keys
                                    foreach (SEAnimFrame Key in AnimationRotationKeys[Bone])
                                    {
                                        // Output frame number based on frame count
                                        if ((FrameCountBuffer - 1) <= 0xFF)
                                        {
                                            // Write as byte
                                            writeFile.Write((byte)Key.Frame);
                                        }
                                        else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                        {
                                            // Write as short
                                            writeFile.Write((short)Key.Frame);
                                        }
                                        else
                                        {
                                            // Write as int
                                            writeFile.Write((int)Key.Frame);
                                        }
                                        // Output the quat
                                        if (HighPrecision)
                                        {
                                            writeFile.Write((double)((Quaternion)Key.Data).X);
                                            writeFile.Write((double)((Quaternion)Key.Data).Y);
                                            writeFile.Write((double)((Quaternion)Key.Data).Z);
                                            writeFile.Write((double)((Quaternion)Key.Data).W);
                                        }
                                        else
                                        {
                                            writeFile.Write((float)((Quaternion)Key.Data).X);
                                            writeFile.Write((float)((Quaternion)Key.Data).Y);
                                            writeFile.Write((float)((Quaternion)Key.Data).Z);
                                            writeFile.Write((float)((Quaternion)Key.Data).W);
                                        }
                                    }
                                }
                                else
                                {
                                    // Check length of frames to write count (of 0 because no frames)
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)0x0);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)0x0);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)0x0);
                                    }
                                }
                            }
                        }

                        #endregion
                        // Write scale keys next (if we have any)
                        #region Scale Keys

                        {
                            // Check if we have any
                            if (AnimationScaleKeys.Count > 0)
                            {
                                // Check if this bone has any scales
                                if (AnimationScaleKeys.ContainsKey(Bone))
                                {
                                    // Check length of frames to write count
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)AnimationScaleKeys[Bone].Count);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)AnimationScaleKeys[Bone].Count);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)AnimationScaleKeys[Bone].Count);
                                    }
                                    // Output keys
                                    foreach (SEAnimFrame Key in AnimationScaleKeys[Bone])
                                    {
                                        // Output frame number based on frame count
                                        if ((FrameCountBuffer - 1) <= 0xFF)
                                        {
                                            // Write as byte
                                            writeFile.Write((byte)Key.Frame);
                                        }
                                        else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                        {
                                            // Write as short
                                            writeFile.Write((short)Key.Frame);
                                        }
                                        else
                                        {
                                            // Write as int
                                            writeFile.Write((int)Key.Frame);
                                        }
                                        // Output the vector
                                        if (HighPrecision)
                                        {
                                            writeFile.Write((double)((Vector3)Key.Data).X);
                                            writeFile.Write((double)((Vector3)Key.Data).Y);
                                            writeFile.Write((double)((Vector3)Key.Data).Z);
                                        }
                                        else
                                        {
                                            writeFile.Write((float)((Vector3)Key.Data).X);
                                            writeFile.Write((float)((Vector3)Key.Data).Y);
                                            writeFile.Write((float)((Vector3)Key.Data).Z);
                                        }
                                    }
                                }
                                else
                                {
                                    // Check length of frames to write count (of 0 because no frames)
                                    if ((FrameCountBuffer - 1) <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)0x0);
                                    }
                                    else if ((FrameCountBuffer - 1) <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((short)0x0);
                                    }
                                    else
                                    {
                                        // Write as int
                                        writeFile.Write((int)0x0);
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                }
                // Output notetracks, if any
                if (AnimationNotetracks.Count > 0)
                {
                    // We have notifications
                    foreach (KeyValuePair<string, List<SEAnimFrame>> Note in AnimationNotetracks)
                    {
                        // Write them
                        foreach (SEAnimFrame Key in Note.Value)
                        {
                            // Write the frame itself based on framecount
                            if ((FrameCountBuffer - 1) <= 0xFF)
                            {
                                // Write as byte
                                writeFile.Write((byte)Key.Frame);
                            }
                            else if ((FrameCountBuffer - 1) <= 0xFFFF)
                            {
                                // Write as short
                                writeFile.Write((short)Key.Frame);
                            }
                            else
                            {
                                // Write as int
                                writeFile.Write((int)Key.Frame);
                            }
                            // Write flag name
                            writeFile.WriteNullTermString(Note.Key);
                        }
                    }
                }
            }
        }

        #endregion

        #region Adding Keys

        /// <summary>
        /// Add a translation key for the specified bone, on the given frame, with a vector3, (Data must be in CM scale!)
        /// </summary>
        /// <param name="Bone">The bone tag to animate</param>
        /// <param name="Frame">The frame index to key on</param>
        /// <param name="X">X part of a vector (Normalized)</param>
        /// <param name="Y">Y part of a vector (Normalized)</param>
        /// <param name="Z">Z part of a vector (Normalized)</param>
        public void AddTranslationKey(string Bone, int Frame, double X, double Y, double Z)
        {
            // We should add this key
            if (!AnimationPositionKeys.ContainsKey(Bone))
            {
                // Set it up
                AnimationPositionKeys.Add(Bone, new List<SEAnimFrame>());
            }
            // Add the key itself
            AnimationPositionKeys[Bone].Add(new SEAnimFrame() { Frame = Frame, Data = new Vector3() { X = X, Y = Y, Z = Z } });
        }

        /// <summary>
        /// Add a rotation key for the specified bone, on the given frame, with a quaternion
        /// </summary>
        /// <param name="Bone">The bone tag to animate</param>
        /// <param name="Frame">The frame index to key on</param>
        /// <param name="X">X part of a quaternion (Normalized)</param>
        /// <param name="Y">Y part of a quaternion (Normalized)</param>
        /// <param name="Z">Z part of a quaternion (Normalized)</param>
        /// <param name="W">W part of a quaternion (Normalized)</param>
        public void AddRotationKey(string Bone, int Frame, double X, double Y, double Z, double W)
        {
            // We should add this key
            if (!AnimationRotationKeys.ContainsKey(Bone))
            {
                // Set it up
                AnimationRotationKeys.Add(Bone, new List<SEAnimFrame>());
            }
            // Add the key itself
            AnimationRotationKeys[Bone].Add(new SEAnimFrame() { Frame = Frame, Data = new Quaternion() { X = X, Y = Y, Z = Z, W = W } });
        }

        /// <summary>
        /// Add a scale key for the specified bone, on the given frame, with a vector
        /// </summary>
        /// <param name="Bone">The bone tag to animate</param>
        /// <param name="Frame">The frame index to key on</param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public void AddScaleKey(string Bone, int Frame, double X, double Y, double Z)
        {
            // We should add this key
            if (!AnimationScaleKeys.ContainsKey(Bone))
            {
                // Set it up
                AnimationScaleKeys.Add(Bone, new List<SEAnimFrame>());
            }
            // Add the key itself
            AnimationScaleKeys[Bone].Add(new SEAnimFrame() { Frame = Frame, Data = new Vector3() { X = X, Y = Y, Z = Z } });
        }

        /// <summary>
        /// Adds a notification at the specified frame
        /// </summary>
        /// <param name="Notification">The notification name</param>
        /// <param name="Frame">The frame index to key on</param>
        public void AddNoteTrack(string Notification, int Frame)
        {
            // We should add this key
            if (!AnimationNotetracks.ContainsKey(Notification))
            {
                // Set it up
                AnimationNotetracks.Add(Notification, new List<SEAnimFrame>());
            }
            // Add the key itself
            AnimationNotetracks[Notification].Add(new SEAnimFrame() { Frame = Frame, Data = null });
        }

        /// <summary>
        /// Adds a bone modifier for this bone, overwriting if existing
        /// </summary>
        /// <param name="Bone">The bone tag to modify</param>
        /// <param name="Modifier">The new animation type for children</param>
        public void AddBoneModifier(string Bone, AnimationType Modifier)
        {
            // Check if it exists
            if (AnimationBoneModifiers.ContainsKey(Bone))
            {
                // Set it
                AnimationBoneModifiers[Bone] = Modifier;
            }
            else
            {
                // Add it
                AnimationBoneModifiers.Add(Bone, Modifier);
            }
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Calculates a unique bone map ordering, moving the delta bone if need be to the front
        /// </summary>
        /// <returns></returns>
        private List<string> BuildUniqueOrderedBoneMap()
        {
            // Build the list
            List<string> BoneMap = new List<string>();
            // Loop and add all of the bone tags
            #region Iterate

            foreach (string Bone in AnimationPositionKeys.Keys)
            {
                // Add it, we don't care if it fails
                if (!BoneMap.Contains(Bone))
                {
                    BoneMap.Add(Bone);
                }
            }
            foreach (string Bone in AnimationRotationKeys.Keys)
            {
                // Add it, we don't care if it fails
                if (!BoneMap.Contains(Bone))
                {
                    BoneMap.Add(Bone);
                }
            }
            foreach (string Bone in AnimationScaleKeys.Keys)
            {
                // Add it, we don't care if it fails
                if (!BoneMap.Contains(Bone))
                {
                    BoneMap.Add(Bone);
                }
            }

            #endregion
            // Check delta tag
            if (AnimType == AnimationType.Delta)
            {
                // Check for and remove the tag, insert back at the top
                if (BoneMap.Contains(DeltaTagName))
                {
                    // Remove
                    BoneMap.Remove(DeltaTagName);
                    // Insert
                    BoneMap.Insert(0, DeltaTagName);
                }
            }
            // Return result
            return BoneMap;
        }

        /// <summary>
        /// Calculates the notification count in accordance with the specifications
        /// </summary>
        private int CalculateNotetracksCount()
        {
            // The total count
            int TotalCount = 0;
            // Loop for all keys and add
            foreach (List<SEAnimFrame> NotificationList in AnimationNotetracks.Values)
            {
                // Append
                TotalCount += NotificationList.Count;
            }
            // Return result
            return TotalCount;
        }

        /// <summary>
        /// Calculates the frame count in accordance with the specifications
        /// </summary>
        private int CalculateFrameCount()
        {
            // The maximum frame index
            int MaxFrame = 0;
            // We must iterate through all key types and compare the max keyed frame against the current max frame
            #region Key Iteration

            foreach (List<SEAnimFrame> Frames in AnimationPositionKeys.Values)
            {
                // Iterate
                foreach (SEAnimFrame Frame in Frames)
                {
                    // Compare
                    MaxFrame = Math.Max(MaxFrame, Frame.Frame);
                }
            }
            foreach (List<SEAnimFrame> Frames in AnimationRotationKeys.Values)
            {
                // Iterate
                foreach (SEAnimFrame Frame in Frames)
                {
                    // Compare
                    MaxFrame = Math.Max(MaxFrame, Frame.Frame);
                }
            }
            foreach (List<SEAnimFrame> Frames in AnimationScaleKeys.Values)
            {
                // Iterate
                foreach (SEAnimFrame Frame in Frames)
                {
                    // Compare
                    MaxFrame = Math.Max(MaxFrame, Frame.Frame);
                }
            }
            foreach (List<SEAnimFrame> Frames in AnimationNotetracks.Values)
            {
                // Iterate
                foreach (SEAnimFrame Frame in Frames)
                {
                    // Compare
                    MaxFrame = Math.Max(MaxFrame, Frame.Frame);
                }
            }

            #endregion
            // Frame count represents the length of the animation in frames
            // Since all animations start at frame 0, we grab the max number and
            // Add 1 to the result
            return MaxFrame + 1;
        }

        /// <summary>
        /// Calculates the bone count in accordance with the specifications
        /// </summary>
        private int CalculateBoneCount()
        {
            // Make a hashset of unique bone names
            HashSet<string> UniqueBoneNames = new HashSet<string>();
            // We must append bone names from all bone type animation keys, then simply return the result of the hash set
            #region Key Iteration

            foreach (string Bone in AnimationPositionKeys.Keys)
            {
                // Add it, we don't care if it fails
                UniqueBoneNames.Add(Bone);
            }
            foreach (string Bone in AnimationRotationKeys.Keys)
            {
                // Add it, we don't care if it fails
                UniqueBoneNames.Add(Bone);
            }
            foreach (string Bone in AnimationScaleKeys.Keys)
            {
                // Add it, we don't care if it fails
                UniqueBoneNames.Add(Bone);
            }

            #endregion
            // Return result
            return UniqueBoneNames.Count;
        }

        #endregion
    }
}
