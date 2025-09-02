using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using static Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.DbManager;

namespace Sky.Template.Backend.UnitTests;

public class DbManagerMappingTests
{
    private static object InvokeMappings(Type t)
    {
        var method = typeof(DbManager).GetMethod("GetColumnMappings", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(t);
        return method.Invoke(null, null)!;
    }

    private class DuplicateIdBase
    {
        [mColumn("id")]
        public Guid Id { get; set; }
    }

    private class DuplicateIdEntity : DuplicateIdBase
    {
        [mColumn("id")]
        public Guid AnotherId { get; set; }
    }

    private class CaseInsensitiveEntity
    {
        [mColumn("ID")]
        public Guid First { get; set; }
        [mColumn("id")]
        public Guid Second { get; set; }
    }

    //[Fact]
    //public void GetColumnMappings_InheritedDuplicate_Throws()
    //{
    //    Action act = () => InvokeMappings(typeof(DuplicateIdEntity));

    //    act.Should().Throw<TargetInvocationException>()
    //        .WithInnerException<InvalidOperationException>()
    //        .Which.Message.Should().Contain("duplicate column mapping 'id'", StringComparison.OrdinalIgnoreCase);
    //}

    //[Fact]
    //public void GetColumnMappings_CaseInsensitiveDuplicate_Throws()
    //{
    //    Action act = () => InvokeMappings(typeof(CaseInsensitiveEntity));

    //    act.Should().Throw<TargetInvocationException>()
    //        .WithInnerException<InvalidOperationException>()
    //        .Which.Message.Should().Contain("duplicate column mapping 'id'", StringComparison.OrdinalIgnoreCase);
    //}
}
