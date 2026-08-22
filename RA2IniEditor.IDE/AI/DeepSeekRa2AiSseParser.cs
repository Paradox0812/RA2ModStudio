using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace RA2IniEditor.IDE.AI;

/// <summary>将 DeepSeek data-only SSE 响应解析为稳定的内部流事件。</summary>
internal sealed class DeepSeekRa2AiSseParser
{
    private const string DataPrefix = "data:";
    private const string DoneMarker = "[DONE]";
    private const string ChunkObjectName = "chat.completion.chunk";
    private const int MaxEventDataCharacters = 1024 * 1024;
    private const string InvalidChunkMessage = "DeepSeek provider returned an invalid streaming chunk.";
    private const string IncompleteStreamMessage = "DeepSeek provider stream ended before the completion marker.";
    private const string OversizedEventMessage = "DeepSeek provider returned an oversized streaming event.";

    /// <summary>按协议顺序读取事件；输入 reader 的释放责任属于调用方。</summary>
    internal async IAsyncEnumerable<Ra2AiStreamEvent> ParseAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reader);

        StringBuilder dataBuffer = new();
        Ra2AiStreamFinishKind finishKind = Ra2AiStreamFinishKind.Unknown;
        string? streamId = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                break;

            if (line.Length == 0)
            {
                if (dataBuffer.Length == 0)
                    continue;

                foreach (Ra2AiStreamEvent streamEvent in ParseDataEvent(
                             dataBuffer.ToString(),
                             ref finishKind,
                             ref streamId))
                {
                    yield return streamEvent;
                    if (streamEvent.Kind == Ra2AiStreamEventKind.Completed)
                        yield break;
                }

                dataBuffer.Clear();
                continue;
            }

            if (line[0] == ':')
                continue;

            if (!line.StartsWith(DataPrefix, StringComparison.Ordinal))
                continue;

            string data = line[DataPrefix.Length..];
            if (data.StartsWith(' '))
                data = data[1..];

            if (dataBuffer.Length > 0)
                dataBuffer.Append('\n');

            if (dataBuffer.Length + data.Length > MaxEventDataCharacters)
                throw new InvalidDataException(OversizedEventMessage);

            dataBuffer.Append(data);
        }

        if (dataBuffer.Length > 0)
        {
            foreach (Ra2AiStreamEvent finalEvent in ParseDataEvent(
                         dataBuffer.ToString(),
                         ref finishKind,
                         ref streamId))
            {
                yield return finalEvent;
                if (finalEvent.Kind == Ra2AiStreamEventKind.Completed)
                    yield break;
            }
        }

        throw new InvalidDataException(IncompleteStreamMessage);
    }

    private static IReadOnlyList<Ra2AiStreamEvent> ParseDataEvent(
        string data,
        ref Ra2AiStreamFinishKind finishKind,
        ref string? streamId)
    {
        string payload = data.Trim();
        if (payload.Length == 0)
            return [];

        if (string.Equals(payload, DoneMarker, StringComparison.Ordinal))
            return [Ra2AiStreamEvent.CreateCompleted(finishKind)];

        using JsonDocument document = JsonDocument.Parse(payload);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException(InvalidChunkMessage);

        ValidateChunkIdentity(root, ref streamId);

        if (!root.TryGetProperty("choices", out JsonElement choices)
            || choices.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(InvalidChunkMessage);
        }

        if (choices.GetArrayLength() == 0)
            return [];

        JsonElement selectedChoice = SelectPrimaryChoice(choices);
        if (!selectedChoice.TryGetProperty("delta", out JsonElement delta)
            || delta.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(InvalidChunkMessage);
        }

        UpdateFinishKind(selectedChoice, ref finishKind);

        List<Ra2AiStreamEvent> events = [];
        if (delta.TryGetProperty("content", out JsonElement content)
            && content.ValueKind != JsonValueKind.Null)
        {
            if (content.ValueKind != JsonValueKind.String)
                throw new JsonException(InvalidChunkMessage);

            string text = content.GetString() ?? string.Empty;
            if (text.Length > 0)
                events.Add(Ra2AiStreamEvent.CreateContentDelta(text));
        }

        AddToolCallDeltas(delta, events);
        return events;
    }

    private static void AddToolCallDeltas(
        JsonElement delta,
        ICollection<Ra2AiStreamEvent> events)
    {
        if (!delta.TryGetProperty("tool_calls", out JsonElement toolCalls)
            || toolCalls.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (toolCalls.ValueKind != JsonValueKind.Array)
            throw new JsonException(InvalidChunkMessage);

        foreach (JsonElement toolCall in toolCalls.EnumerateArray())
        {
            if (toolCall.ValueKind != JsonValueKind.Object ||
                !toolCall.TryGetProperty("index", out JsonElement indexElement) ||
                !indexElement.TryGetInt32(out int index) ||
                index < 0)
            {
                throw new JsonException(InvalidChunkMessage);
            }

            ValidateToolType(toolCall);
            string idFragment = ReadOptionalString(toolCall, "id");
            string nameFragment = string.Empty;
            string argumentsFragment = string.Empty;
            if (toolCall.TryGetProperty("function", out JsonElement function)
                && function.ValueKind != JsonValueKind.Null)
            {
                if (function.ValueKind != JsonValueKind.Object)
                    throw new JsonException(InvalidChunkMessage);

                nameFragment = ReadOptionalString(function, "name");
                argumentsFragment = ReadOptionalString(function, "arguments");
            }

            if (idFragment.Length == 0 &&
                nameFragment.Length == 0 &&
                argumentsFragment.Length == 0)
            {
                continue;
            }

            events.Add(Ra2AiStreamEvent.CreateToolCallDelta(new Ra2AiToolCallDelta(
                index,
                idFragment,
                nameFragment,
                argumentsFragment)));
        }
    }

    private static void ValidateToolType(JsonElement toolCall)
    {
        if (!toolCall.TryGetProperty("type", out JsonElement typeElement)
            || typeElement.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (typeElement.ValueKind != JsonValueKind.String ||
            !string.Equals(typeElement.GetString(), "function", StringComparison.Ordinal))
        {
            throw new JsonException(InvalidChunkMessage);
        }
    }

    private static string ReadOptionalString(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out JsonElement element)
            || element.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }

        if (element.ValueKind != JsonValueKind.String)
            throw new JsonException(InvalidChunkMessage);

        return element.GetString() ?? string.Empty;
    }

    private static void ValidateChunkIdentity(JsonElement root, ref string? streamId)
    {
        if (root.TryGetProperty("object", out JsonElement objectElement)
            && (objectElement.ValueKind != JsonValueKind.String
                || !string.Equals(objectElement.GetString(), ChunkObjectName, StringComparison.Ordinal)))
        {
            throw new JsonException(InvalidChunkMessage);
        }

        if (!root.TryGetProperty("id", out JsonElement idElement))
            return;

        if (idElement.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(idElement.GetString()))
        {
            throw new JsonException(InvalidChunkMessage);
        }

        string currentId = idElement.GetString()!;
        if (streamId is not null && !string.Equals(streamId, currentId, StringComparison.Ordinal))
            throw new JsonException(InvalidChunkMessage);

        streamId = currentId;
    }

    private static JsonElement SelectPrimaryChoice(JsonElement choices)
    {
        JsonElement selectedChoice = default;
        bool foundPrimaryChoice = false;
        bool hasIndexedChoice = false;

        foreach (JsonElement choice in choices.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object)
                throw new JsonException(InvalidChunkMessage);

            if (!choice.TryGetProperty("index", out JsonElement indexElement))
                continue;

            hasIndexedChoice = true;
            if (!indexElement.TryGetInt32(out int index))
                throw new JsonException(InvalidChunkMessage);

            if (index != 0)
                continue;

            if (foundPrimaryChoice)
                throw new JsonException(InvalidChunkMessage);

            selectedChoice = choice;
            foundPrimaryChoice = true;
        }

        if (foundPrimaryChoice)
            return selectedChoice;

        if (!hasIndexedChoice && choices.GetArrayLength() == 1)
            return choices[0];

        throw new JsonException(InvalidChunkMessage);
    }

    private static void UpdateFinishKind(
        JsonElement choice,
        ref Ra2AiStreamFinishKind finishKind)
    {
        if (!choice.TryGetProperty("finish_reason", out JsonElement finishReasonElement)
            || finishReasonElement.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (finishReasonElement.ValueKind != JsonValueKind.String)
            throw new JsonException(InvalidChunkMessage);

        finishKind = finishReasonElement.GetString() switch
        {
            "stop" => Ra2AiStreamFinishKind.Stop,
            "length" => Ra2AiStreamFinishKind.Length,
            "content_filter" => Ra2AiStreamFinishKind.ContentFilter,
            "tool_calls" => Ra2AiStreamFinishKind.ToolCalls,
            "insufficient_system_resource" => Ra2AiStreamFinishKind.InsufficientSystemResource,
            _ => Ra2AiStreamFinishKind.Unknown
        };
    }
}
