using ShiftEngine.Application.Imports;

namespace ShiftEngine.Domain.Tests;

public class PersonnelFileParserTests
{
    [Fact]
    public void Parse_ValidRows_NoErrors()
    {
        var csv = """
            PersonnelNumber;DisplayName;ContractedHoursMonthly;GenderCode;PrimaryRole;Email;ExternalLegacyId
            1001;Max Mustermann;174;M;Security;;
            1002;Erika;130;F;LSKP;;
            """;
        var rows = PersonnelFileParser.Parse(csv);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Null(r.Error));
        Assert.Equal(174m, rows[0].ContractedHoursMonthly);
        Assert.Equal("F", rows[1].GenderCode);
    }

    [Fact]
    public void Parse_MissingHours_HasError()
    {
        var rows = PersonnelFileParser.Parse("1001;Max;;M;Security;;");
        Assert.Single(rows);
        Assert.NotNull(rows[0].Error);
    }
}

public class PositionFileParserTests
{
    [Fact]
    public void Parse_ValidPost_ParsesGenderMinimums()
    {
        var csv = """
            Name;WindowStart;WindowEnd;RequiredHeadcount;MinRequiredFemale;MinRequiredMale;GenderIrrelevant;RequiredQualificationCode;BufferPercent
            Haupteingang;06:00;14:00;2;1;0;0;SCHUER;10
            """;
        var rows = PositionFileParser.Parse(csv);
        Assert.Single(rows);
        Assert.Null(rows[0].Error);
        Assert.False(rows[0].IsGenderIrrelevant);
        Assert.Equal(1, rows[0].MinRequiredFemale);
    }

    [Fact]
    public void Parse_GenderIrrelevant_ClearsMinimums()
    {
        var csv = """
            Name;WindowStart;WindowEnd;RequiredHeadcount;MinRequiredFemale;MinRequiredMale;GenderIrrelevant;RequiredQualificationCode;BufferPercent
            Revier;22:00;06:00;1;2;1;irrelevant;SCHUER;0
            """;
        var rows = PositionFileParser.Parse(csv);
        Assert.Single(rows);
        Assert.True(rows[0].IsGenderIrrelevant);
        Assert.Equal(0, rows[0].MinRequiredFemale);
        Assert.Equal(0, rows[0].MinRequiredMale);
    }
}
