using System;
using System.Collections.Generic;
using System.Linq;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiConversationContextProvider : IRa2AiConversationContextProvider
{
    private const string TruncationSuffix = " [truncated]";

    public Ra2AiConversationContext BuildContext(Ra2AiConversationContextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<Ra2AiConversationTurn> eligibleTurns = request.Turns
            .Where(turn => turn.State == Ra2AiConversationTurnState.Completed && turn.IsContextEligible)
            .ToArray();

        if (eligibleTurns.Count == 0)
            return new Ra2AiConversationContext();

        int lastTurns = Math.Max(0, request.LastTurns);
        int maxCharacters = Math.Max(0, request.MaxCharacters);
        int maxSingleTurnCharacters = Math.Max(0, request.MaxSingleTurnCharacters);

        if (lastTurns == 0 || maxCharacters == 0 || maxSingleTurnCharacters == 0)
        {
            return new Ra2AiConversationContext
            {
                WasTruncated = true
            };
        }

        bool wasTruncated = eligibleTurns.Count > lastTurns;
        IReadOnlyList<Ra2AiConversationTurn> recentTurns = eligibleTurns
            .Skip(Math.Max(0, eligibleTurns.Count - lastTurns))
            .ToArray();

        List<Ra2AiConversationTurn> boundedNewestFirstTurns = [];
        int remainingCharacters = maxCharacters;

        for (int index = recentTurns.Count - 1; index >= 0; index--)
        {
            Ra2AiConversationTurn sourceTurn = recentTurns[index];
            string sanitizedText = Ra2AiOutboundTextSanitizer.Sanitize(sourceTurn.Text).Text;
            string singleBoundedText = Truncate(sanitizedText, maxSingleTurnCharacters, out bool singleTurnTruncated);
            wasTruncated |= singleTurnTruncated;

            if (singleBoundedText.Length > remainingCharacters)
            {
                singleBoundedText = Truncate(singleBoundedText, remainingCharacters, out bool totalTruncated);
                wasTruncated |= totalTruncated;
            }

            if (singleBoundedText.Length == 0 && sourceTurn.Text.Length > 0)
            {
                wasTruncated = true;
                break;
            }

            boundedNewestFirstTurns.Add(new Ra2AiConversationTurn
            {
                Role = sourceTurn.Role,
                Text = singleBoundedText,
                IsDraftResponse = sourceTurn.Role == Ra2AiConversationRole.Assistant,
                State = Ra2AiConversationTurnState.Completed,
                IsContextEligible = true
            });

            remainingCharacters -= singleBoundedText.Length;
            if (remainingCharacters <= 0 && index > 0)
            {
                wasTruncated = true;
                break;
            }
        }

        boundedNewestFirstTurns.Reverse();

        return new Ra2AiConversationContext
        {
            Turns = boundedNewestFirstTurns,
            TotalCharacterCount = boundedNewestFirstTurns.Sum(turn => turn.Text.Length),
            WasTruncated = wasTruncated
        };
    }

    private static string Truncate(string text, int maxCharacters, out bool wasTruncated)
    {
        if (text.Length <= maxCharacters)
        {
            wasTruncated = false;
            return text;
        }

        wasTruncated = true;
        if (maxCharacters <= 0)
            return string.Empty;

        if (maxCharacters <= TruncationSuffix.Length)
            return text[..maxCharacters];

        return string.Concat(text.AsSpan(0, maxCharacters - TruncationSuffix.Length), TruncationSuffix);
    }
}
