using Beef.Entities;
using Beef.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Core.UnitTest.Mapper
{
    [TestFixture]
    public class ChangeLogMapperTest
    {
        private ChangeLog CreateChangeLog() => new ChangeLog
        {
            CreatedBy = "C",
            CreatedDate = new DateTime(1970, 01, 01),
            UpdatedBy = "U",
            UpdatedDate = new DateTime(1980, 01, 01)
        };

        private ChangeLog CreateChangeLog2() => new ChangeLog
        {
            CreatedBy = "CC",
            CreatedDate = new DateTime(1971, 01, 01),
            UpdatedBy = "UU",
            UpdatedDate = new DateTime(1981, 01, 01)
        };

        [Test]
        public void MapToDest_Create_OperationAny()
        {
            var r = EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToDest(new TestS { ChangeLog = CreateChangeLog() });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.AreEqual("C", r.ChangeLog.CreatedBy);
            Assert.AreEqual(new DateTime(1970, 01, 01), r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToDest_Create_OperationUpdate()
        {
            var r = EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToDest(new TestS { ChangeLog = CreateChangeLog() }, OperationTypes.Update);

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.IsNull(r.ChangeLog.CreatedBy);
            Assert.IsNull(r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToDest_Replace_OperationAny()
        {
            var r = new TestD { ChangeLog = CreateChangeLog2() };
            EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToDest(new TestS { ChangeLog = CreateChangeLog() }, r);

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.AreEqual("C", r.ChangeLog.CreatedBy);
            Assert.AreEqual(new DateTime(1970, 01, 01), r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToDest_Replace_OperationUpdate()
        {
            var r = new TestD { ChangeLog = CreateChangeLog2() };
            EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToDest(new TestS { ChangeLog = CreateChangeLog() }, r, OperationTypes.Update);

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.AreEqual("CC", r.ChangeLog.CreatedBy);
            Assert.AreEqual(new DateTime(1971, 01, 01), r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToSrce_Create_OperationAny()
        {
            var r = EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToSrce(new TestD { ChangeLog = CreateChangeLog() });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.AreEqual("C", r.ChangeLog.CreatedBy);
            Assert.AreEqual(new DateTime(1970, 01, 01), r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToSrce_Create_OperationUpdate()
        {
            var r = EntityMapper.Create<TestS, TestD>()
                .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
                .MapToSrce(new TestD { ChangeLog = CreateChangeLog() }, OperationTypes.Update);

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ChangeLog);
            Assert.IsNull(r.ChangeLog.CreatedBy);
            Assert.IsNull(r.ChangeLog.CreatedDate);
            Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToSrce_Replace_OperationAny()
        {
            Assert.Inconclusive("Functionality not implemented (yet).");
            //var r = new TestS { ChangeLog = CreateChangeLog2() };
            //EntityMapper<TestS, TestD>.Create()
            //    .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
            //    .MapToSrce(new TestD { ChangeLog = CreateChangeLog() }, r);

            //Assert.IsNotNull(r);
            //Assert.IsNotNull(r.ChangeLog);
            //Assert.AreEqual("C", r.ChangeLog.CreatedBy);
            //Assert.AreEqual(new DateTime(1970, 01, 01), r.ChangeLog.CreatedDate);
            //Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            //Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        [Test]
        public void MapToSrce_Replace_OperationUpdate()
        {
            Assert.Inconclusive("Functionality not implemented (yet).");
            //var r = new TestS { ChangeLog = CreateChangeLog2() };
            //EntityMapper<TestS, TestD>.Create()
            //    .HasProperty(s => s.ChangeLog, d => d.ChangeLog, p => p.SetMapper(ChangeLogMapper.Default))
            //    .MapToSrce(new TestD { ChangeLog = CreateChangeLog() }, r, OperationTypes.Update);

            //Assert.IsNotNull(r);
            //Assert.IsNotNull(r.ChangeLog);
            //Assert.AreEqual("CC", r.ChangeLog.CreatedBy);
            //Assert.AreEqual(new DateTime(1971, 01, 01), r.ChangeLog.CreatedDate);
            //Assert.AreEqual("U", r.ChangeLog.UpdatedBy);
            //Assert.AreEqual(new DateTime(1980, 01, 01), r.ChangeLog.UpdatedDate);
        }

        public class TestS
        {
            public ChangeLog ChangeLog { get; set; }
        }

        public class TestD
        {
            public ChangeLog ChangeLog { get; set; }
        }
    }
}
