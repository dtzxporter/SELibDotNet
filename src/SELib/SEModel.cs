using SELib.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///
///   SEModel.cs
///   Author: DTZxPorter
///   Written for the SE Format Project
///   Follows SEModel specification v1.0
///   https://github.com/dtzxporter/SEModel-Docs/blob/master/spec.md
///

namespace SELib
{
    #region SEModel Enums

    /// <summary>
    /// Specified the type of model, this may be user implemented and is subject to change
    /// </summary>
    public enum ModelTypes : byte
    {
        /// <summary>
        /// This model data is static geometry
        /// </summary>
        Static = 0,
        /// <summary>
        /// This model data is an animated model
        /// </summary>
        Dynamic = 1,
        /// <summary>
        /// This model data is used for collisions
        /// </summary>
        Collision = 2,

        /// <summary>
        /// This was an unknown model type value
        /// </summary>
        Unknown = 0xFF
    }

    /// <summary>
    /// Specifies what type of bone positioning to use in the model
    /// </summary>
    public enum ModelBoneSupportTypes : byte
    {
        /// <summary>
        /// The model will be saved with local bone matricies
        /// </summary>
        SupportsLocals,
        /// <summary>
        /// The model will be saved with global bone matricies
        /// </summary>
        SupportsGlobals,
        /// <summary>
        /// The model will be saved with both global and local matricies
        /// </summary>
        SupportsBoth
    }

    /// <summary>
    /// Specifies the data present for the model
    /// </summary>
    internal enum SEModel_DataPresenceFlags : byte
    {
        // Whether or not this model contains a bone block
        SEMODEL_PRESENCE_BONE = 1 << 0,
        // Whether or not this model contains submesh blocks
        SEMODEL_PRESENCE_MESH = 1 << 1,
        // Whether or not this model contains collision data blocks
        SEMODEL_PRESENCE_COLLISION = 1 << 2,

        // Whether or not this model contains a material reference block
        SEMODEL_PRESENCE_MATERIALS = 1 << 3,

        // RESERVED_0		= 1 << 4, // ALWAYS FALSE
        // RESERVED_1		= 1 << 5, // ALWAYS FALSE
        // RESERVED_2		= 1 << 6, // ALWAYS FALSE
        // RESERVED_3		= 1 << 7, // ALWAYS FALSE
    }

    /// <summary>
    /// Specifies the data present for each bone in the model
    /// </summary>
    internal enum SEModel_BoneDataPresenceFlags : byte
    {
        // Whether or not bones contain global matricies
        SEMODEL_PRESENCE_GLOBAL_MATRIX = 1 << 0,
        // Whether or not bones contain local matricies
        SEMODEL_PRESENCE_LOCAL_MATRIX = 1 << 1,

        // Whether or not bones contain scales
        SEMODEL_PRESENCE_SCALES = 1 << 2,

        // RESERVED_0		= 1 << 3, // ALWAYS FALSE
        // RESERVED_1		= 1 << 4, // ALWAYS FALSE
        // RESERVED_2		= 1 << 5, // ALWAYS FALSE
        // RESERVED_3		= 1 << 6, // ALWAYS FALSE
        // RESERVED_4		= 1 << 7, // ALWAYS FALSE
    }

    /// <summary>
    /// Specifies the data present for each vertex in the model
    /// </summary>
    internal enum SEModel_MeshDataPresenceFlags : byte
    {
        // Whether or not meshes contain at least 1 uv layer
        SEMODEL_PRESENCE_UVSET = 1 << 0,

        // Whether or not meshes contain vertex normals
        SEMODEL_PRESENCE_NORMALS = 1 << 1,

        // Whether or not meshes contain vertex colors
        SEMODEL_PRESENCE_COLOR = 1 << 2,

        // Whether or not meshes contain at least 1 weight set
        SEMODEL_PRESENCE_WEIGHTS = 1 << 3,

        // RESERVED_0		= 1 << 4, // ALWAYS FALSE
        // RESERVED_1		= 1 << 5, // ALWAYS FALSE
        // RESERVED_2		= 1 << 6, // ALWAYS FALSE
        // RESERVED_3		= 1 << 7, // ALWAYS FALSE
    }

    #endregion

    #region SEModel Bone

    /// <summary>
    /// Contains information for a specific bone
    /// </summary>
    public class SEModelBone
    {
        /// <summary>
        /// Whether or not this bone is a root bone (No parent)
        /// </summary>
        public bool RootBone { get { return (BoneParent <= -1); } }

        /// <summary>
        /// Get or set the name of this bone
        /// </summary>
        public string BoneName { get; set; }
        /// <summary>
        /// Get or set the parent index of this bone, -1 for a root bone
        /// </summary>
        public int BoneParent { get; set; }

        /// <summary>
        /// Get or set the global position of this bone
        /// </summary>
        public Vector3 GlobalPosition { get; set; }
        /// <summary>
        /// Get or set the global rotation of this bone
        /// </summary>
        public Quaternion GlobalRotation { get; set; }

        /// <summary>
        /// Get or set the local position of this bone
        /// </summary>
        public Vector3 LocalPosition { get; set; }
        /// <summary>
        /// Get or set the local rotation of this bone
        /// </summary>
        public Quaternion LocalRotation { get; set; }

        /// <summary>
        /// Get or set the scale of this bone
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Creates a new SEModelBone with the default settings
        /// </summary>
        public SEModelBone()
        {
            BoneName = string.Empty;
            BoneParent = -1;
            GlobalPosition = new Vector3() { X = 0, Y = 0, Z = 0 };
            LocalPosition = new Vector3() { X = 0, Y = 0, Z = 0 };
            GlobalRotation = new Quaternion() { X = 0, Y = 0, Z = 0, W = 1 };
            LocalRotation = new Quaternion() { X = 0, Y = 0, Z = 0, W = 1 };
            Scale = new Vector3() { X = 1, Y = 1, Z = 1 };
        }
    }

    #endregion

    #region SEModel Mesh

    public class SEModelMesh
    {
        /// <summary>
        /// Returns the count of verticies in the mesh
        /// </summary>
        public uint VertexCount { get { return (uint)Verticies.Count; } }
        /// <summary>
        /// Returns the count of faces in the mesh
        /// </summary>
        public uint FaceCount { get { return (uint)Faces.Count; } }

        /// <summary>
        /// A list of verticies in the mesh
        /// </summary>
        public List<SEModelVertex> Verticies { get; set; }
        /// <summary>
        /// A list of faces in the mesh, faces match D3DPT_TRIANGLELIST (DirectX) and VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST (Vulkan)
        /// </summary>
        public List<SEModelFace> Faces { get; set; }

        /// <summary>
        /// Creates a new mesh with default settings
        /// </summary>
        public SEModelMesh()
        {
            Verticies = new List<SEModelVertex>();
            Faces = new List<SEModelFace>();
        }

        /// <summary>
        /// Adds the given vertex to the mesh
        /// </summary>
        /// <param name="Vertex">The vertex to add to the mesh</param>
        public void AddVertex(SEModelVertex Vertex)
        {
            // Add it
            Verticies.Add(Vertex);
        }

        /// <summary>
        /// Adds a new face to the mesh with the given indicies
        /// </summary>
        public void AddFace(uint Index1, uint Index2, uint Index3)
        {
            // Add new face
            Faces.Add(new SEModelFace(Index1, Index2, Index3));
        }
    }

    public class SEModelFace
    {
        /* Vertex indicies for this face */

        public uint FaceIndex1;
        public uint FaceIndex2;
        public uint FaceIndex3;

        /// <summary>
        /// Creates a new face with default settings
        /// </summary>
        public SEModelFace(uint Index1, uint Index2, uint Index3)
        {
            FaceIndex1 = Index1;
            FaceIndex2 = Index2;
            FaceIndex3 = Index3;
        }
    }

    public class SEModelVertex
    {
        /// <summary>
        /// Returns the amount of UVSets for this vertex
        /// </summary>
        public uint UVSetCount { get { return (uint)UVSets.Count; } }
        /// <summary>
        /// Returns the amount of skin influences for this vertex
        /// </summary>
        public uint WeightCount { get { return (uint)Weights.Count; } }

        /// <summary>
        /// The position of the vertex
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The uv sets for this vertex
        /// </summary>
        public List<SEModelUVSet> UVSets { get; set; }

        /// <summary>
        /// The vertex normal
        /// </summary>
        public Vector3 VertexNormal { get; set; }
        /// <summary>
        /// The vertex coloring
        /// </summary>
        public Color VertexColor { get; set; }

        /// <summary>
        /// A list of skin weights for this vertex
        /// </summary>
        public List<SEModelWeight> Weights { get; set; }

        /// <summary>
        /// Creates a new SEModelVertex using default settings
        /// </summary>
        public SEModelVertex()
        {
            Position = Vector3.Zero;
            UVSets = new List<SEModelUVSet>();
            VertexNormal = Vector3.Zero;
            VertexColor = Color.White;
            Weights = new List<SEModelWeight>();
        }
    }

    public class SEModelWeight
    {
        /// <summary>
        /// The bone index for this weight
        /// </summary>
        public uint BoneIndex { get; set; }
        /// <summary>
        /// The weight value, from 0.0 to 1.0 for this weight
        /// </summary>
        public float BoneWeight { get; set; }

        /// <summary>
        /// Creates a new SEModelWeight with default settings
        /// </summary>
        public SEModelWeight() { BoneIndex = 0; BoneWeight = 1.0f; }
    }

    public class SEModelUVSet
    {
        /// <summary>
        /// The UV coords for this UVSet
        /// </summary>
        public Vector2 UVCoord { get; set; }
        /// <summary>
        /// The material index of the UV, the index is within the models materials
        /// </summary>
        public uint MaterialIndex { get; set; }

        /// <summary>
        /// Creates a new SEModelUVSet with default settings
        /// </summary>
        public SEModelUVSet() { UVCoord = new Vector2(); MaterialIndex = 0; }
    }

    #endregion

    /// <summary>
    /// Represents a SEModel file, allows for reading and writing model data
    /// </summary>
    public class SEModel
    {
        /* Model data */

        /// <summary>
        /// A list of bones, in order by index
        /// </summary>
        public List<SEModelBone> Bones { get; private set; }
        /// <summary>
        /// A list of meshes, in order
        /// </summary>
        public List<SEModelMesh> Meshes { get; private set; }

        /// <summary>
        /// Gets the SEModel specification version this library supports
        /// </summary>
        public string APIVersion { get { return "v1.0.0"; } }

        /* Model properties */

        /// <summary>
        /// The model type for this model
        /// </summary>
        public ModelTypes ModelType { get; set; }
        /// <summary>
        /// Specifies what data the model should use when using bones, defaults to local matricies.
        /// </summary>
        public ModelBoneSupportTypes ModelBoneSupport { get; set; }
        /// <summary>
        /// Implementation defined flags for the model
        /// </summary>
        public ushort ModelFlags { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of bones that can influence a skinned vertex, defaults to 4
        /// </summary>
        public byte MaxSkinInfluence { get; set; }
        /// <summary>
        /// Returns the number of bones in this model, this is automatically updated
        /// </summary>
        public uint BoneCount { get { return (uint)Bones.Count; } }
        /// <summary>
        /// Returns the number of meshes in this model, this is automatically updated
        /// </summary>
        public uint MeshCount { get { return (uint)Meshes.Count; } }

        /// <summary>
        /// Creates a new SEModel using default settings
        /// </summary>
        public SEModel()
        {
            Bones = new List<SEModelBone>();
            Meshes = new List<SEModelMesh>();
            ModelBoneSupport = ModelBoneSupportTypes.SupportsLocals;
            ModelFlags = 0;
            MaxSkinInfluence = 4;
        }

        /* Functions and utilities */

        #region Writing

        /// <summary>
        /// Saves the SEModel to a stream, following the current specification version, using the provided data
        /// </summary>
        /// <param name="Stream">The file stream to write to</param>
        public void Write(Stream Stream)
        {
            // Open up a binary writer
            using (ExtendedBinaryWriter writeFile = new ExtendedBinaryWriter(Stream))
            {
                // Write magic
                writeFile.Write(new char[] { 'S', 'E', 'M', 'o', 'd', 'e', 'l' });
                // Write version
                writeFile.Write((short)0x1);
                // Write header size
                writeFile.Write((short)0x1C);
                // Write model type
                writeFile.Write((byte)ModelType);
                // Write model flags, implemtation defined
                writeFile.Write((ushort)ModelFlags);
                // Build data present flags
                {
                    // Buffer
                    byte DataPresentFlags = 0x0;
                    // Check for bones
                    if (Bones.Count > 0)
                    {
                        DataPresentFlags |= (byte)SEModel_DataPresenceFlags.SEMODEL_PRESENCE_BONE;
                    }
                    // Check for meshes
                    if (Meshes.Count > 0)
                    {
                        DataPresentFlags |= (byte)SEModel_DataPresenceFlags.SEMODEL_PRESENCE_MESH;
                    }
                    // TODO: Collision data present flags go here

                    // Check for materials

                    // Write it
                    writeFile.Write((byte)DataPresentFlags);
                }

                // Flags for use when writing bone data to the buffer
                bool HasScales = false;

                // Build bone data present flags
                {
                    // Buffer
                    byte BoneDataPresentFlags = 0x0;

                    // Only test / apply if we have bones
                    if (Bones.Count > 0)
                    {
                        // Iterate over the bones
                        foreach (var Bone in Bones)
                        {
                            // Check for scales
                            if (Bone.Scale != Vector3.One) { HasScales = true; break; }
                        }

                        // Apply scales
                        if (HasScales) { BoneDataPresentFlags |= (byte)SEModel_BoneDataPresenceFlags.SEMODEL_PRESENCE_SCALES; }

                        // Apply globals
                        if (ModelBoneSupport == ModelBoneSupportTypes.SupportsGlobals || ModelBoneSupport == ModelBoneSupportTypes.SupportsBoth) { BoneDataPresentFlags |= (byte)SEModel_BoneDataPresenceFlags.SEMODEL_PRESENCE_GLOBAL_MATRIX; }
                        // Apply locals
                        if (ModelBoneSupport == ModelBoneSupportTypes.SupportsLocals || ModelBoneSupport == ModelBoneSupportTypes.SupportsBoth) { BoneDataPresentFlags |= (byte)SEModel_BoneDataPresenceFlags.SEMODEL_PRESENCE_LOCAL_MATRIX; }
                    }

                    // Write it
                    writeFile.Write((byte)BoneDataPresentFlags);
                }

                // TODO: Auto average weights when a vertex has > max influences!

                // Flags for use when writing mesh data to the buffer
                bool HasUVSets = false;
                bool HasNormals = false;
                bool HasColor = false;
                bool HasWeights = false;

                // Build mesh data present flags
                {
                    // Buffer
                    byte MeshDataPresentFlags = 0x0;

                    // Only test / apply if we have bones
                    if (Meshes.Count > 0)
                    {
                        // Iterate over meshes
                        Parallel.ForEach<SEModelMesh>(Meshes, (mesh, LoopState) =>
                        {
                            // Iterate verticies
                            foreach (var Vertex in mesh.Verticies)
                            {
                                // Check for UVSets
                                if (Vertex.UVSetCount > 0) { HasUVSets = true; }
                                // Check for normals
                                if (Vertex.VertexNormal != Vector3.Zero) { HasNormals = true; }
                                // Check for color
                                if (Vertex.VertexColor != Color.White) { HasColor = true; }
                                // Check for weights
                                if (Vertex.WeightCount > 0) { HasWeights = true; }

                                // Has all break
                                if (HasUVSets && HasNormals && HasColor && HasWeights) { LoopState.Break(); }
                            }
                        });
                    }

                    // Apply UV
                    if (HasUVSets) { MeshDataPresentFlags |= (byte)SEModel_MeshDataPresenceFlags.SEMODEL_PRESENCE_UVSET; }
                    // Apply normals
                    if (HasNormals) { MeshDataPresentFlags |= (byte)SEModel_MeshDataPresenceFlags.SEMODEL_PRESENCE_NORMALS; }
                    // Apply color
                    if (HasColor) { MeshDataPresentFlags |= (byte)SEModel_MeshDataPresenceFlags.SEMODEL_PRESENCE_COLOR; }
                    // Apply weights
                    if (HasWeights) { MeshDataPresentFlags |= (byte)SEModel_MeshDataPresenceFlags.SEMODEL_PRESENCE_WEIGHTS; }

                    // Write it
                    writeFile.Write((byte)MeshDataPresentFlags);
                }

                // Write reserved bytes
                writeFile.Write(new byte[2] { 0x0, 0x0 });

                // Write bone count
                writeFile.Write((uint)Bones.Count);
                // Write mesh count
                writeFile.Write((uint)Meshes.Count);
                // Write collision count
                writeFile.Write((uint)0x0);
                // Write material count
                writeFile.Write((uint)0x0);

                // Write max skin influence
                writeFile.Write((Meshes.Count > 0) ? (byte)MaxSkinInfluence : (byte)0);

                // Write 3 reserved bytes
                writeFile.Write(new byte[3] { 0x0, 0x0, 0x0 });

                // Prepare to write bone data
                {
                    // Loop and write tag names
                    foreach (var Bone in Bones)
                    {
                        writeFile.WriteNullTermString(Bone.BoneName);
                    }
                    // Loop and write data
                    foreach (var Bone in Bones)
                    {
                        // Write flags, 0 for now
                        writeFile.Write((byte)0x0);
                        // Write parent
                        writeFile.Write((int)Bone.BoneParent);

                        // Check presence flags and write matricies
                        // Globals are before locals, position before rotation

                        if (ModelBoneSupport == ModelBoneSupportTypes.SupportsGlobals || ModelBoneSupport == ModelBoneSupportTypes.SupportsBoth)
                        {
                            writeFile.Write((float)Bone.GlobalPosition.X);
                            writeFile.Write((float)Bone.GlobalPosition.Y);
                            writeFile.Write((float)Bone.GlobalPosition.Z);

                            writeFile.Write((float)Bone.GlobalRotation.X);
                            writeFile.Write((float)Bone.GlobalRotation.Y);
                            writeFile.Write((float)Bone.GlobalRotation.Z);
                            writeFile.Write((float)Bone.GlobalRotation.W);
                        }

                        if (ModelBoneSupport == ModelBoneSupportTypes.SupportsLocals || ModelBoneSupport == ModelBoneSupportTypes.SupportsBoth)
                        {
                            writeFile.Write((float)Bone.LocalPosition.X);
                            writeFile.Write((float)Bone.LocalPosition.Y);
                            writeFile.Write((float)Bone.LocalPosition.Z);

                            writeFile.Write((float)Bone.LocalRotation.X);
                            writeFile.Write((float)Bone.LocalRotation.Y);
                            writeFile.Write((float)Bone.LocalRotation.Z);
                            writeFile.Write((float)Bone.LocalRotation.W);
                        }

                        // Check scale flags
                        if (HasScales)
                        {
                            writeFile.Write((float)Bone.Scale.X);
                            writeFile.Write((float)Bone.Scale.Y);
                            writeFile.Write((float)Bone.Scale.Z);
                        }
                    }
                }

                // Prepare to write mesh data
                {
                    // Loop and write data
                    foreach (var Mesh in Meshes)
                    {
                        // Write flags, 0 for now
                        writeFile.Write((byte)0x0);

                        // Write UVSetCount
                        writeFile.Write((byte)0x0);
                        
                        // Write vertex count
                        writeFile.Write((uint)Mesh.VertexCount);
                        // Write face count
                        writeFile.Write((uint)Mesh.FaceCount);

                        // Loop and write verticies
                        foreach (var Vertex in Mesh.Verticies)
                        {
                            // Position
                            writeFile.Write((float)Vertex.Position.X);
                            writeFile.Write((float)Vertex.Position.Y);
                            writeFile.Write((float)Vertex.Position.Z);

                            // UVSets if we have any
                            if (HasUVSets)
                            {
                                // Write coords
                                foreach (var UV in Vertex.UVSets)
                                {
                                    writeFile.Write((float)UV.UVCoord.X);
                                    writeFile.Write((float)UV.UVCoord.Y);
                                }
                                // Write indicies
                                foreach (var UV in Vertex.UVSets)
                                {
                                    writeFile.Write((uint)UV.MaterialIndex);
                                }
                            }

                            // Normals if we have any
                            if (HasNormals)
                            {
                                // Write normals (halfs)
                                writeFile.Write((ushort)HalfHelper.SingleToHalf((float)Vertex.VertexNormal.X).value);
                                writeFile.Write((ushort)HalfHelper.SingleToHalf((float)Vertex.VertexNormal.Y).value);
                                writeFile.Write((ushort)HalfHelper.SingleToHalf((float)Vertex.VertexNormal.Z).value);
                            }

                            // Colors if we have any
                            if (HasColor)
                            {
                                // Write RGBA
                                writeFile.Write((byte)Vertex.VertexColor.R);
                                writeFile.Write((byte)Vertex.VertexColor.G);
                                writeFile.Write((byte)Vertex.VertexColor.B);
                                writeFile.Write((byte)Vertex.VertexColor.A);
                            }

                            // Weights if we have any
                            if (HasWeights)
                            {
                                // Write indicies
                                foreach (var Weight in Vertex.Weights)
                                {
                                    // Write it, depends on bone count
                                    if (BoneCount <= 0xFF)
                                    {
                                        // Write as byte
                                        writeFile.Write((byte)Weight.BoneIndex);
                                    }
                                    else if (BoneCount <= 0xFFFF)
                                    {
                                        // Write as short
                                        writeFile.Write((ushort)Weight.BoneIndex);
                                    }
                                    else
                                    {
                                        // Write as integer
                                        writeFile.Write((uint)Weight.BoneIndex);
                                    }
                                }
                                // Write values
                                foreach (var Weight in Vertex.Weights)
                                {
                                    // Write as half
                                    writeFile.Write((ushort)HalfHelper.SingleToHalf((float)Weight.BoneWeight).value);
                                }
                            }
                        }

                        // Loop and write faces
                        foreach (var Face in Mesh.Faces)
                        {
                            writeFile.Write((uint)Face.FaceIndex1);
                            writeFile.Write((uint)Face.FaceIndex2);
                            writeFile.Write((uint)Face.FaceIndex3);
                        }
                    }
                }

                // TODO: Collision data goes here, if any
                {

                }

                // Prepare to write material data
                {

                }
            }
        }

        /// <summary>
        /// Saves the SEModel to a file (Overwriting if exists), following the current specification version, using the provided data
        /// </summary>
        /// <param name="FileName">The file name to save the model to</param>
        public void Write(string FileName)
        {
            // Proxy off
            Write(File.Create(FileName));
        }

        #endregion

        #region Adding Data

        /// <summary>
        /// Adds a new bone to the model with the given information
        /// </summary>
        /// <param name="Name">The tag name of this bone</param>
        /// <param name="ParentIndex">The parent index of this bone, -1 for a root bone</param>
        /// <param name="GlobalPosition">The global space position of this bone</param>
        /// <param name="GlobalRotation">The global space rotation of this bone</param>
        /// <param name="LocalPosition">The local parent space position of this bone</param>
        /// <param name="LocalRotation">The local parent space rotation of this bone</param>
        /// <param name="Scale">The scale of this bone, 1.0 is default</param>
        public void AddBone(string Name, int ParentIndex, Vector3 GlobalPosition, Quaternion GlobalRotation, Vector3 LocalPosition, Quaternion LocalRotation, Vector3 Scale)
        {
            var Bone = new SEModelBone() { BoneName = Name, BoneParent = ParentIndex };

            // Set matricies
            Bone.GlobalPosition = GlobalPosition;
            Bone.GlobalRotation = GlobalRotation;
            Bone.LocalPosition = LocalPosition;
            Bone.LocalRotation = LocalRotation;

            // Set scale
            Bone.Scale = Scale;

            // Add
            Bones.Add(Bone);
        }

        /// <summary>
        /// Adds the given mesh to the model
        /// </summary>
        /// <param name="Mesh">The mesh to add</param>
        public void AddMesh(SEModelMesh Mesh)
        {
            // Add it
            Meshes.Add(Mesh);
        }

        #endregion
    }
}
