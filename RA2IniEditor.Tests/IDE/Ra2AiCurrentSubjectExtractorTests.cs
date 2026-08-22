using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiCurrentSubjectExtractorTests
{
    [Fact]
    public void Extract_UnitSubjectFromAssistantDraft()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ## rulesmd.ini 草稿

            ```ini
            [LAAV]
            Strength=200
            Armor=light
            Primary=LAAVMissile
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Unit, subject.Kind);
        Assert.Equal("LAAV", subject.SubjectId);
        Assert.Equal(Ra2AiSubjectSource.LastAssistantDraft, subject.Source);
        Assert.True(subject.IsDraft);
    }

    [Fact]
    public void Extract_WeaponSubjectFromAssistantDraft()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ```ini
            [LAAVMissile]
            Damage=30
            ROF=45
            Warhead=LAAVMissileWH
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Weapon, subject.Kind);
        Assert.Equal("LAAVMissile", subject.SubjectId);
    }

    [Fact]
    public void Extract_WarheadSubjectFromAssistantDraft()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ```ini
            [LAAVMissileWH]
            Verses=100%,80%,60%,40%,20%,20%,10%,10%,10%,100%,100%
            CellSpread=.3
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Warhead, subject.Kind);
        Assert.Equal("LAAVMissileWH", subject.SubjectId);
    }

    [Fact]
    public void Extract_ProjectileSubjectFromAssistantDraft()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ```ini
            [LAAVMissileP]
            AA=yes
            AG=no
            Image=DRAGON
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Projectile, subject.Kind);
        Assert.Equal("LAAVMissileP", subject.SubjectId);
    }

    [Fact]
    public void Extract_UnitPrototypeDraftPrioritizesMainUnitOverWeaponDefinitions()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ## rulesmd.ini 草稿

            ```ini
            [LAAV]
            Strength=200
            Armor=light
            Primary=LAAVMissile

            [LAAVMissile]
            Damage=30
            ROF=45
            Projectile=LAAVMissileP
            Warhead=LAAVMissileWH
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Unit, subject.Kind);
        Assert.Equal("LAAV", subject.SubjectId);
    }

    [Fact]
    public void Extract_UserMentionCanSelectSectionWhenNoMainUnitExists()
    {
        Ra2AiConversationContext context = Context([
            Assistant("""
                ```ini
                [LAAVMissile]
                Damage=30
                ROF=45
                Warhead=LAAVMissileWH

                [LAAVMissileWH]
                Verses=100%,80%,60%
                CellSpread=.3
                ```
                """),
            User("继续说明 [LAAVMissileWH]。")
        ]);

        Ra2AiCurrentSubject subject = new Ra2AiCurrentSubjectExtractor().Extract(context);

        Assert.Equal(Ra2AiSubjectKind.Warhead, subject.Kind);
        Assert.Equal("LAAVMissileWH", subject.SubjectId);
        Assert.Equal(Ra2AiSubjectSource.UserMention, subject.Source);
        Assert.True(subject.IsDraft);
    }

    [Fact]
    public void Extract_UnknownMalformedDraftReturnsUnknownSafely()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ```ini
            [LAAV
            Strength 200
            ```
            """);

        Assert.Equal(Ra2AiSubjectKind.Unknown, subject.Kind);
        Assert.Null(subject.SubjectId);
        Assert.Equal(Ra2AiSubjectSource.Unknown, subject.Source);
        Assert.False(subject.IsDraft);
        Assert.Equal(0, subject.Confidence);
    }

    [Fact]
    public void Extract_SubjectSummaryMarksDraftAndDoesNotClaimProjectFileState()
    {
        Ra2AiCurrentSubject subject = ExtractAssistantDraft("""
            ```ini
            [LAAV]
            Strength=200
            Armor=light
            Primary=LAAVMissile
            ```
            """);

        Assert.Equal(Ra2AiSubjectSource.LastAssistantDraft, subject.Source);
        Assert.True(subject.IsDraft);
        Assert.Contains("上一轮 AI 草稿", subject.Summary);
        Assert.Contains("尚未确认", subject.Summary);
        Assert.DoesNotContain("已存在于项目文件", subject.Summary);
        Assert.DoesNotContain("已写入项目文件", subject.Summary);
    }

    [Fact]
    public void Extract_DoesNotRequireFilesProvidersDiagnosticsOrEnvironmentVariables()
    {
        Ra2AiConversationContext context = Context([
            Assistant("""
                ```ini
                [LAAV]
                Strength=200
                Armor=light
                Primary=LAAVMissile
                ```
                """)
        ]);

        Ra2AiCurrentSubject subject = new Ra2AiCurrentSubjectExtractor().Extract(context);

        Assert.Equal(Ra2AiSubjectKind.Unit, subject.Kind);
        Assert.Equal("LAAV", subject.SubjectId);
    }

    private static Ra2AiCurrentSubject ExtractAssistantDraft(string text)
        => new Ra2AiCurrentSubjectExtractor().Extract(Context([Assistant(text)]));

    private static Ra2AiConversationContext Context(IReadOnlyList<Ra2AiConversationTurn> turns)
        => new()
        {
            Turns = turns,
            TotalCharacterCount = turns.Sum(turn => turn.Text.Length)
        };

    private static Ra2AiConversationTurn User(string text)
        => new()
        {
            Role = Ra2AiConversationRole.User,
            Text = text
        };

    private static Ra2AiConversationTurn Assistant(string text)
        => new()
        {
            Role = Ra2AiConversationRole.Assistant,
            Text = text,
            IsDraftResponse = true
        };
}
