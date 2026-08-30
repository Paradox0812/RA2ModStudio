using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal readonly record struct Ra2MeshVector3(double X, double Y, double Z)
{
    internal bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public static Ra2MeshVector3 operator +(Ra2MeshVector3 left, Ra2MeshVector3 right) =>
        new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Ra2MeshVector3 operator -(Ra2MeshVector3 left, Ra2MeshVector3 right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Ra2MeshVector3 operator *(Ra2MeshVector3 value, double scalar) =>
        new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    internal static double Dot(Ra2MeshVector3 left, Ra2MeshVector3 right) =>
        (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

    internal static Ra2MeshVector3 Cross(Ra2MeshVector3 left, Ra2MeshVector3 right) => new(
        (left.Y * right.Z) - (left.Z * right.Y),
        (left.Z * right.X) - (left.X * right.Z),
        (left.X * right.Y) - (left.Y * right.X));
}

internal readonly record struct Ra2MeshTriangle(int A, int B, int C);

internal readonly record struct Ra2MeshBounds(Ra2MeshVector3 Minimum, Ra2MeshVector3 Maximum)
{
    internal Ra2MeshVector3 Extents => Maximum - Minimum;
}

internal readonly record struct Ra2MeshTopologyFacts(
    int VertexCount,
    int TriangleCount,
    int ComponentCount,
    int RepeatedIndexTriangleCount,
    int ZeroAreaTriangleCount,
    int BoundaryEdgeCount,
    int NonManifoldEdgeCount)
{
    internal bool IsWatertightSingleComponent =>
        ComponentCount == 1 &&
        RepeatedIndexTriangleCount == 0 &&
        ZeroAreaTriangleCount == 0 &&
        BoundaryEdgeCount == 0 &&
        NonManifoldEdgeCount == 0;
}

internal sealed class Ra2MeshSnapshot
{
    private readonly Ra2MeshVector3[] _positions;
    private readonly Ra2MeshTriangle[] _triangles;

    internal Ra2MeshSnapshot(
        IEnumerable<Ra2MeshVector3> positions,
        IEnumerable<Ra2MeshTriangle> triangles,
        string sourceHash)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(triangles);
        if (string.IsNullOrWhiteSpace(sourceHash) || sourceHash.Length != 64)
            throw new ArgumentException("A mesh source hash must be SHA-256 hex.", nameof(sourceHash));

        _positions = positions.ToArray();
        _triangles = triangles.ToArray();
        if (_positions.Length == 0 || _triangles.Length == 0 || _positions.Any(position => !position.IsFinite))
            throw new ArgumentException("A mesh snapshot requires finite geometry.");

        Positions = Array.AsReadOnly(_positions);
        Triangles = Array.AsReadOnly(_triangles);
        SourceHash = sourceHash.ToUpperInvariant();
        Bounds = ComputeBounds(_positions);
        Topology = ComputeTopology(_positions, _triangles);
    }

    internal IReadOnlyList<Ra2MeshVector3> Positions { get; }
    internal IReadOnlyList<Ra2MeshTriangle> Triangles { get; }
    internal string SourceHash { get; }
    internal Ra2MeshBounds Bounds { get; }
    internal Ra2MeshTopologyFacts Topology { get; }

    private static Ra2MeshBounds ComputeBounds(Ra2MeshVector3[] positions)
    {
        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double minZ = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;
        double maxZ = double.NegativeInfinity;
        foreach (Ra2MeshVector3 position in positions)
        {
            minX = Math.Min(minX, position.X);
            minY = Math.Min(minY, position.Y);
            minZ = Math.Min(minZ, position.Z);
            maxX = Math.Max(maxX, position.X);
            maxY = Math.Max(maxY, position.Y);
            maxZ = Math.Max(maxZ, position.Z);
        }

        return new Ra2MeshBounds(new(minX, minY, minZ), new(maxX, maxY, maxZ));
    }

    private static Ra2MeshTopologyFacts ComputeTopology(
        Ra2MeshVector3[] positions,
        Ra2MeshTriangle[] triangles)
    {
        int[] parents = Enumerable.Range(0, positions.Length).ToArray();
        byte[] ranks = new byte[positions.Length];
        bool[] used = new bool[positions.Length];
        ulong[] edges = new ulong[checked(triangles.Length * 3)];
        int edgeOffset = 0;
        int repeated = 0;
        int zeroArea = 0;

        foreach (Ra2MeshTriangle triangle in triangles)
        {
            if ((uint)triangle.A >= (uint)positions.Length ||
                (uint)triangle.B >= (uint)positions.Length ||
                (uint)triangle.C >= (uint)positions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(triangles));
            }

            used[triangle.A] = used[triangle.B] = used[triangle.C] = true;
            if (triangle.A == triangle.B || triangle.B == triangle.C || triangle.C == triangle.A)
            {
                repeated++;
                continue;
            }

            Ra2MeshVector3 edge1 = positions[triangle.B] - positions[triangle.A];
            Ra2MeshVector3 edge2 = positions[triangle.C] - positions[triangle.A];
            Ra2MeshVector3 cross = Ra2MeshVector3.Cross(edge1, edge2);
            if (Ra2MeshVector3.Dot(cross, cross) <= 1e-24)
                zeroArea++;

            Union(parents, ranks, triangle.A, triangle.B);
            Union(parents, ranks, triangle.B, triangle.C);
            edges[edgeOffset++] = PackEdge(triangle.A, triangle.B);
            edges[edgeOffset++] = PackEdge(triangle.B, triangle.C);
            edges[edgeOffset++] = PackEdge(triangle.C, triangle.A);
        }

        int components = Enumerable.Range(0, positions.Length)
            .Where(index => used[index])
            .Select(index => Find(parents, index))
            .Distinct()
            .Count();

        Array.Sort(edges, 0, edgeOffset);
        int boundaryEdges = 0;
        int nonManifoldEdges = 0;
        for (int index = 0; index < edgeOffset;)
        {
            int next = index + 1;
            while (next < edgeOffset && edges[next] == edges[index])
                next++;
            int incidence = next - index;
            if (incidence == 1)
                boundaryEdges++;
            else if (incidence > 2)
                nonManifoldEdges++;
            index = next;
        }

        return new Ra2MeshTopologyFacts(
            positions.Length,
            triangles.Length,
            components,
            repeated,
            zeroArea,
            boundaryEdges,
            nonManifoldEdges);
    }

    private static ulong PackEdge(int left, int right)
    {
        uint minimum = checked((uint)Math.Min(left, right));
        uint maximum = checked((uint)Math.Max(left, right));
        return ((ulong)minimum << 32) | maximum;
    }

    private static int Find(int[] parents, int value)
    {
        while (parents[value] != value)
        {
            parents[value] = parents[parents[value]];
            value = parents[value];
        }
        return value;
    }

    private static void Union(int[] parents, byte[] ranks, int left, int right)
    {
        int leftRoot = Find(parents, left);
        int rightRoot = Find(parents, right);
        if (leftRoot == rightRoot)
            return;
        if (ranks[leftRoot] < ranks[rightRoot])
            parents[leftRoot] = rightRoot;
        else if (ranks[leftRoot] > ranks[rightRoot])
            parents[rightRoot] = leftRoot;
        else
        {
            parents[rightRoot] = leftRoot;
            if (ranks[leftRoot] < byte.MaxValue)
                ranks[leftRoot]++;
        }
    }
}

internal sealed class Ra2MeshVoxelizationException : Exception
{
    internal Ra2MeshVoxelizationException(Ra2MeshVoxelizationFailureKind failureKind, string message)
        : base(message)
    {
        FailureKind = failureKind;
    }

    internal Ra2MeshVoxelizationFailureKind FailureKind { get; }
}

internal static class Ra2GlbMeshReader
{
    internal const int MaximumGlbBytes = 16 * 1024 * 1024;
    internal const int MaximumJsonBytes = 1024 * 1024;
    internal const int MaximumNodes = 64;
    internal const int MaximumNodeDepth = 16;
    internal const int MaximumMeshes = 16;
    internal const int MaximumPrimitives = 64;
    internal const int MaximumVertices = 500_000;
    internal const int MaximumTriangles = 1_000_000;
    internal const int MaximumBufferViews = 256;
    internal const int MaximumAccessors = 256;

    private const uint GlbMagic = 0x46546C67;
    private const uint JsonChunkType = 0x4E4F534A;
    private const uint BinChunkType = 0x004E4942;

    internal static Ra2MeshSnapshot Read(ReadOnlyMemory<byte> input, CancellationToken cancellationToken = default)
    {
        if (input.Length is < 28 or > MaximumGlbBytes)
            Fail(input.Length > MaximumGlbBytes ? Ra2MeshVoxelizationFailureKind.InputTooLarge : Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB length is invalid.");

        ReadOnlySpan<byte> bytes = input.Span;
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != GlbMagic ||
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) != 2 ||
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]) != input.Length)
        {
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB header is invalid.");
        }

        int jsonLength = ReadChunkLength(bytes, 12, JsonChunkType);
        if (jsonLength <= 0 || jsonLength > MaximumJsonBytes || (jsonLength & 3) != 0)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB JSON chunk is invalid.");
        int binHeaderOffset = checked(20 + jsonLength);
        if (binHeaderOffset > bytes.Length - 8)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB JSON chunk exceeds the container.");
        int binLength = ReadChunkLength(bytes, binHeaderOffset, BinChunkType);
        int binOffset = checked(binHeaderOffset + 8);
        if (binLength <= 0 || (binLength & 3) != 0 || binOffset + binLength != bytes.Length)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB BIN chunk is invalid or trailing data exists.");

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using JsonDocument document = JsonDocument.Parse(input.Slice(20, jsonLength));
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("asset", out JsonElement asset) ||
                !asset.TryGetProperty("version", out JsonElement version) ||
                version.GetString() is not string versionText ||
                !versionText.StartsWith("2.", StringComparison.Ordinal))
            {
                Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "glTF asset version is missing or unsupported.");
            }

            RejectRequiredExtensions(root);
            JsonElement.ArrayEnumerator buffers = RequireArray(root, "buffers", 1, 1).EnumerateArray();
            buffers.MoveNext();
            int declaredBufferLength = RequireInt(buffers.Current, "byteLength", 1, binLength);
            if (binLength - declaredBufferLength > 3)
                Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB BIN padding exceeds the format allowance.");

            BufferView[] bufferViews = ReadBufferViews(root, declaredBufferLength);
            Accessor[] accessors = ReadAccessors(root, bufferViews.Length);
            JsonElement[] meshes = RequireArray(root, "meshes", 1, MaximumMeshes).EnumerateArray().ToArray();
            JsonElement[] nodes = RequireArray(root, "nodes", 1, MaximumNodes).EnumerateArray().ToArray();
            JsonElement[] scenes = RequireArray(root, "scenes", 1, 1).EnumerateArray().ToArray();
            int sceneIndex = root.TryGetProperty("scene", out JsonElement sceneValue) ? sceneValue.GetInt32() : 0;
            if (sceneIndex != 0)
                Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "The selected glTF scene is out of range.");

            List<Ra2MeshVector3> positions = [];
            List<Ra2MeshTriangle> triangles = [];
            bool[] visited = new bool[nodes.Length];
            bool[] active = new bool[nodes.Length];
            int primitiveCount = 0;
            JsonElement.ArrayEnumerator roots = RequireArray(scenes[0], "nodes", 1, MaximumNodes).EnumerateArray();
            while (roots.MoveNext())
            {
                TraverseNode(
                    roots.Current.GetInt32(),
                    depth: 0,
                    MeshMatrix.Identity,
                    nodes,
                    meshes,
                    bufferViews,
                    accessors,
                    bytes.Slice(binOffset, declaredBufferLength),
                    visited,
                    active,
                    positions,
                    triangles,
                    ref primitiveCount,
                    cancellationToken);
            }

            if (positions.Count == 0 || triangles.Count == 0)
                Fail(Ra2MeshVoxelizationFailureKind.DegenerateGeometry, "The selected scene contains no indexed triangles.");

            return new Ra2MeshSnapshot(
                positions,
                triangles,
                Convert.ToHexString(SHA256.HashData(input.Span)));
        }
        catch (Ra2MeshVoxelizationException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        {
            throw new Ra2MeshVoxelizationException(
                Ra2MeshVoxelizationFailureKind.MalformedContainer,
                $"GLB structure is invalid: {exception.Message}");
        }
    }

    private static void TraverseNode(
        int nodeIndex,
        int depth,
        MeshMatrix parent,
        JsonElement[] nodes,
        JsonElement[] meshes,
        BufferView[] bufferViews,
        Accessor[] accessors,
        ReadOnlySpan<byte> bin,
        bool[] visited,
        bool[] active,
        List<Ra2MeshVector3> positions,
        List<Ra2MeshTriangle> triangles,
        ref int primitiveCount,
        CancellationToken cancellationToken)
    {
        if ((uint)nodeIndex >= (uint)nodes.Length || depth > MaximumNodeDepth)
            Fail(Ra2MeshVoxelizationFailureKind.ResourceLimitExceeded, "glTF node index or hierarchy depth is invalid.");
        if (active[nodeIndex] || visited[nodeIndex])
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "glTF nodes must form a single-parent acyclic graph.");

        active[nodeIndex] = true;
        visited[nodeIndex] = true;
        JsonElement node = nodes[nodeIndex];
        MeshMatrix world = MeshMatrix.Multiply(parent, ReadNodeMatrix(node));
        if (node.TryGetProperty("mesh", out JsonElement meshValue))
        {
            int meshIndex = meshValue.GetInt32();
            if ((uint)meshIndex >= (uint)meshes.Length)
                Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "glTF node references an invalid mesh.");
            ReadMesh(meshes[meshIndex], world, bufferViews, accessors, bin, positions, triangles, ref primitiveCount, cancellationToken);
        }

        if (node.TryGetProperty("children", out JsonElement children))
        {
            if (children.ValueKind != JsonValueKind.Array || children.GetArrayLength() > MaximumNodes)
                Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "glTF node children are invalid.");
            foreach (JsonElement child in children.EnumerateArray())
            {
                TraverseNode(child.GetInt32(), depth + 1, world, nodes, meshes, bufferViews, accessors, bin,
                    visited, active, positions, triangles, ref primitiveCount, cancellationToken);
            }
        }

        active[nodeIndex] = false;
    }

    private static void ReadMesh(
        JsonElement mesh,
        MeshMatrix transform,
        BufferView[] bufferViews,
        Accessor[] accessors,
        ReadOnlySpan<byte> bin,
        List<Ra2MeshVector3> positions,
        List<Ra2MeshTriangle> triangles,
        ref int primitiveCount,
        CancellationToken cancellationToken)
    {
        JsonElement.ArrayEnumerator primitives = RequireArray(mesh, "primitives", 1, MaximumPrimitives).EnumerateArray();
        while (primitives.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++primitiveCount > MaximumPrimitives)
                Fail(Ra2MeshVoxelizationFailureKind.ResourceLimitExceeded, "glTF primitive count exceeds the limit.");
            JsonElement primitive = primitives.Current;
            int mode = primitive.TryGetProperty("mode", out JsonElement modeValue) ? modeValue.GetInt32() : 4;
            if (mode != 4)
                Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Only indexed TRIANGLES primitives are supported.");
            if (!primitive.TryGetProperty("indices", out JsonElement indexValue))
                Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Only indexed TRIANGLES primitives are supported.");
            if (primitive.TryGetProperty("targets", out _))
                Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Morph targets are not supported.");
            if (!primitive.TryGetProperty("attributes", out JsonElement attributes) ||
                attributes.ValueKind != JsonValueKind.Object)
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "A POSITION accessor is required.");
            if (!attributes.TryGetProperty("POSITION", out JsonElement positionValue))
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "A POSITION accessor is required.");

            Accessor positionAccessor = GetAccessor(accessors, positionValue.GetInt32());
            if (positionAccessor.ComponentType != 5126 || positionAccessor.Type != "VEC3" || positionAccessor.Normalized)
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "POSITION must be non-normalized VEC3/FLOAT.");
            Accessor indexAccessor = GetAccessor(accessors, indexValue.GetInt32());
            if ((indexAccessor.ComponentType is not 5123 and not 5125) || indexAccessor.Type != "SCALAR" || indexAccessor.Normalized)
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "Indices must be unsigned 16-bit or 32-bit SCALAR.");
            if (indexAccessor.Count % 3 != 0)
                Fail(Ra2MeshVoxelizationFailureKind.InvalidIndex, "Triangle index count must be divisible by three.");
            if (positions.Count + positionAccessor.Count > MaximumVertices ||
                triangles.Count + (indexAccessor.Count / 3) > MaximumTriangles)
            {
                Fail(Ra2MeshVoxelizationFailureKind.ResourceLimitExceeded, "Mesh geometry exceeds the configured limits.");
            }

            int baseVertex = positions.Count;
            for (int index = 0; index < positionAccessor.Count; index++)
            {
                ReadOnlySpan<byte> element = GetElement(bin, bufferViews, positionAccessor, index, 12);
                Ra2MeshVector3 transformed = transform.Transform(new(
                    BinaryPrimitives.ReadSingleLittleEndian(element),
                    BinaryPrimitives.ReadSingleLittleEndian(element[4..]),
                    BinaryPrimitives.ReadSingleLittleEndian(element[8..])));
                if (!transformed.IsFinite)
                    Fail(Ra2MeshVoxelizationFailureKind.NonFiniteGeometry, "Mesh positions or transforms are non-finite.");
                positions.Add(transformed);
            }

            int indexSize = indexAccessor.ComponentType == 5123 ? 2 : 4;
            for (int index = 0; index < indexAccessor.Count; index += 3)
            {
                int a = ReadIndex(GetElement(bin, bufferViews, indexAccessor, index, indexSize), indexAccessor.ComponentType);
                int b = ReadIndex(GetElement(bin, bufferViews, indexAccessor, index + 1, indexSize), indexAccessor.ComponentType);
                int c = ReadIndex(GetElement(bin, bufferViews, indexAccessor, index + 2, indexSize), indexAccessor.ComponentType);
                if ((uint)a >= (uint)positionAccessor.Count || (uint)b >= (uint)positionAccessor.Count || (uint)c >= (uint)positionAccessor.Count)
                    Fail(Ra2MeshVoxelizationFailureKind.InvalidIndex, "A triangle index exceeds the POSITION accessor.");
                triangles.Add(new(baseVertex + a, baseVertex + b, baseVertex + c));
            }
        }
    }

    private static int ReadIndex(ReadOnlySpan<byte> bytes, int componentType) => componentType == 5123
        ? BinaryPrimitives.ReadUInt16LittleEndian(bytes)
        : checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes));

    private static ReadOnlySpan<byte> GetElement(
        ReadOnlySpan<byte> bin,
        BufferView[] views,
        Accessor accessor,
        int index,
        int elementSize)
    {
        BufferView view = views[accessor.BufferView];
        int stride = view.ByteStride == 0 ? elementSize : view.ByteStride;
        if (stride < elementSize)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "Accessor stride is smaller than its element.");
        int relative = checked(accessor.ByteOffset + (index * stride));
        if (relative < 0 || relative > view.ByteLength - elementSize)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "Accessor exceeds its buffer view.");
        return bin.Slice(checked(view.ByteOffset + relative), elementSize);
    }

    private static MeshMatrix ReadNodeMatrix(JsonElement node)
    {
        bool hasMatrix = node.TryGetProperty("matrix", out JsonElement matrix);
        bool hasTrs = node.TryGetProperty("translation", out _) || node.TryGetProperty("rotation", out _) || node.TryGetProperty("scale", out _);
        if (hasMatrix && hasTrs)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidTransform, "A glTF node cannot specify both matrix and TRS.");
        if (node.TryGetProperty("skin", out _))
            Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Skins are not supported.");

        MeshMatrix result = hasMatrix
            ? MeshMatrix.FromColumnMajor(ReadDoubleArray(matrix, 16, "matrix"))
            : MeshMatrix.FromTrs(
                node.TryGetProperty("translation", out JsonElement translation) ? ReadDoubleArray(translation, 3, "translation") : [0d, 0d, 0d],
                node.TryGetProperty("rotation", out JsonElement rotation) ? ReadDoubleArray(rotation, 4, "rotation") : [0d, 0d, 0d, 1d],
                node.TryGetProperty("scale", out JsonElement scale) ? ReadDoubleArray(scale, 3, "scale") : [1d, 1d, 1d]);
        if (!result.IsFinite)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidTransform, "A glTF node transform is non-finite.");
        return result;
    }

    private static double[] ReadDoubleArray(JsonElement element, int length, string name)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() != length)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidTransform, $"glTF {name} has an invalid shape.");
        double[] values = element.EnumerateArray().Select(value => value.GetDouble()).ToArray();
        if (values.Any(value => !double.IsFinite(value)))
            Fail(Ra2MeshVoxelizationFailureKind.InvalidTransform, $"glTF {name} contains a non-finite value.");
        return values;
    }

    private static void RejectRequiredExtensions(JsonElement root)
    {
        if (!root.TryGetProperty("extensionsRequired", out JsonElement extensions))
            return;
        if (extensions.ValueKind != JsonValueKind.Array)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "extensionsRequired must be an array.");
        if (extensions.GetArrayLength() != 0)
            Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Required glTF extensions are not supported.");
    }

    private static BufferView[] ReadBufferViews(JsonElement root, int bufferLength)
    {
        JsonElement[] elements = RequireArray(root, "bufferViews", 1, MaximumBufferViews).EnumerateArray().ToArray();
        BufferView[] views = new BufferView[elements.Length];
        for (int index = 0; index < views.Length; index++)
        {
            JsonElement element = elements[index];
            int buffer = RequireInt(element, "buffer", 0, 0);
            _ = buffer;
            int offset = element.TryGetProperty("byteOffset", out JsonElement offsetValue) ? offsetValue.GetInt32() : 0;
            int length = RequireInt(element, "byteLength", 1, bufferLength);
            int stride = element.TryGetProperty("byteStride", out JsonElement strideValue) ? strideValue.GetInt32() : 0;
            if (offset < 0 || length > bufferLength - offset || (stride != 0 && stride is < 4 or > 252))
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "A glTF buffer view is out of range.");
            views[index] = new(offset, length, stride);
        }
        return views;
    }

    private static Accessor[] ReadAccessors(JsonElement root, int bufferViewCount)
    {
        JsonElement[] elements = RequireArray(root, "accessors", 1, MaximumAccessors).EnumerateArray().ToArray();
        Accessor[] accessors = new Accessor[elements.Length];
        for (int index = 0; index < accessors.Length; index++)
        {
            JsonElement element = elements[index];
            if (element.TryGetProperty("sparse", out _))
                Fail(Ra2MeshVoxelizationFailureKind.UnsupportedFeature, "Sparse accessors are not supported.");
            int view = RequireInt(element, "bufferView", 0, bufferViewCount - 1);
            int offset = element.TryGetProperty("byteOffset", out JsonElement offsetValue) ? offsetValue.GetInt32() : 0;
            int component = RequireInt(element, "componentType", 1, int.MaxValue);
            int count = RequireInt(element, "count", 1, MaximumTriangles * 3);
            string type = element.TryGetProperty("type", out JsonElement typeValue) ? typeValue.GetString() ?? string.Empty : string.Empty;
            bool normalized = element.TryGetProperty("normalized", out JsonElement normalizedValue) && normalizedValue.GetBoolean();
            if (offset < 0)
                Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "Accessor byte offset cannot be negative.");
            accessors[index] = new(view, offset, component, count, type, normalized);
        }
        return accessors;
    }

    private static Accessor GetAccessor(Accessor[] accessors, int index)
    {
        if ((uint)index >= (uint)accessors.Length)
            Fail(Ra2MeshVoxelizationFailureKind.InvalidAccessor, "Accessor index is out of range.");
        return accessors[index];
    }

    private static JsonElement RequireArray(JsonElement parent, string name, int minimum, int maximum)
    {
        if (!parent.TryGetProperty(name, out JsonElement value) ||
            value.ValueKind != JsonValueKind.Array ||
            value.GetArrayLength() < minimum ||
            value.GetArrayLength() > maximum)
        {
            Fail(Ra2MeshVoxelizationFailureKind.ResourceLimitExceeded, $"glTF {name} count is outside the supported range.");
        }
        return value;
    }

    private static int RequireInt(JsonElement parent, string name, int minimum, int maximum)
    {
        int result = 0;
        if (!parent.TryGetProperty(name, out JsonElement value) || !value.TryGetInt32(out result) || result < minimum || result > maximum)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, $"glTF {name} is invalid.");
        return result;
    }

    private static int ReadChunkLength(ReadOnlySpan<byte> bytes, int offset, uint expectedType)
    {
        if (offset < 0 || offset > bytes.Length - 8)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB chunk header is truncated.");
        uint rawLength = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..]);
        if (rawLength > int.MaxValue)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB chunk length exceeds the supported range.");
        int length = (int)rawLength;
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[(offset + 4)..]) != expectedType)
            Fail(Ra2MeshVoxelizationFailureKind.MalformedContainer, "GLB chunk order or type is unsupported.");
        return length;
    }

    [DoesNotReturn]
    private static void Fail(Ra2MeshVoxelizationFailureKind kind, string message) =>
        throw new Ra2MeshVoxelizationException(kind, message);

    private readonly record struct BufferView(int ByteOffset, int ByteLength, int ByteStride);
    private readonly record struct Accessor(int BufferView, int ByteOffset, int ComponentType, int Count, string Type, bool Normalized);

    private readonly record struct MeshMatrix(
        double M00, double M01, double M02, double M03,
        double M10, double M11, double M12, double M13,
        double M20, double M21, double M22, double M23,
        double M30, double M31, double M32, double M33)
    {
        internal static MeshMatrix Identity => new(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        internal bool IsFinite => new[]
        {
            M00, M01, M02, M03, M10, M11, M12, M13,
            M20, M21, M22, M23, M30, M31, M32, M33
        }.All(double.IsFinite);

        internal Ra2MeshVector3 Transform(Ra2MeshVector3 value)
        {
            double x = (M00 * value.X) + (M01 * value.Y) + (M02 * value.Z) + M03;
            double y = (M10 * value.X) + (M11 * value.Y) + (M12 * value.Z) + M13;
            double z = (M20 * value.X) + (M21 * value.Y) + (M22 * value.Z) + M23;
            double w = (M30 * value.X) + (M31 * value.Y) + (M32 * value.Z) + M33;
            if (!double.IsFinite(w) || Math.Abs(w) < 1e-15)
                return new(double.NaN, double.NaN, double.NaN);
            return new(x / w, y / w, z / w);
        }

        internal static MeshMatrix FromColumnMajor(double[] values) => new(
            values[0], values[4], values[8], values[12],
            values[1], values[5], values[9], values[13],
            values[2], values[6], values[10], values[14],
            values[3], values[7], values[11], values[15]);

        internal static MeshMatrix FromTrs(double[] translation, double[] rotation, double[] scale)
        {
            double x = rotation[0];
            double y = rotation[1];
            double z = rotation[2];
            double w = rotation[3];
            double length = Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
            if (!double.IsFinite(length) || length < 1e-15)
                return new(double.NaN, 0, 0, 0, 0, double.NaN, 0, 0, 0, 0, double.NaN, 0, 0, 0, 0, 1);
            x /= length;
            y /= length;
            z /= length;
            w /= length;

            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;
            double wx = w * x;
            double wy = w * y;
            double wz = w * z;

            return new MeshMatrix(
                (1 - (2 * (yy + zz))) * scale[0], (2 * (xy - wz)) * scale[1], (2 * (xz + wy)) * scale[2], translation[0],
                (2 * (xy + wz)) * scale[0], (1 - (2 * (xx + zz))) * scale[1], (2 * (yz - wx)) * scale[2], translation[1],
                (2 * (xz - wy)) * scale[0], (2 * (yz + wx)) * scale[1], (1 - (2 * (xx + yy))) * scale[2], translation[2],
                0, 0, 0, 1);
        }

        internal static MeshMatrix Multiply(MeshMatrix left, MeshMatrix right)
        {
            double[] a = left.ToArray();
            double[] b = right.ToArray();
            double[] result = new double[16];
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            for (int k = 0; k < 4; k++)
                result[(row * 4) + column] += a[(row * 4) + k] * b[(k * 4) + column];
            return new MeshMatrix(
                result[0], result[1], result[2], result[3],
                result[4], result[5], result[6], result[7],
                result[8], result[9], result[10], result[11],
                result[12], result[13], result[14], result[15]);
        }

        private double[] ToArray() =>
        [
            M00, M01, M02, M03, M10, M11, M12, M13,
            M20, M21, M22, M23, M30, M31, M32, M33
        ];
    }
}
