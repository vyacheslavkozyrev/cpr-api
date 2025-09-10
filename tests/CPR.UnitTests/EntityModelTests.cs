using System;
using CPR.Domain.Entities;
using Xunit;

namespace CPR.UnitTests
{
    public class EntityModelTests
    {
        [Fact]
        public void NewCareerPath_Defaults()
        {
            var cp = new CareerPath { Title = "Test" };
            Assert.False(cp.IsDeleted);
            // Id is not generated until persisted; default should be empty
            Assert.Equal(Guid.Empty, cp.Id);
        }

        [Fact]
        public void NewSkill_Defaults()
        {
            var s = new Skill { Title = "X" };
            Assert.False(s.IsDeleted);
            // Id is not generated until persisted; default should be empty
            Assert.Equal(Guid.Empty, s.Id);
        }
    }
}
