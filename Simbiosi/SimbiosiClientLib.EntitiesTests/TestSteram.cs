using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimbiosiClientLib.Entities.Streams;

namespace SimbiosiClientLib.EntitiesTests
{
    [TestClass]
    public class TestSteram
    {
        [TestMethod]
        public void WriteReadStream()
        {
            var m = new MemoryStream();
            var w = new SimbiosiStreamWriter(m, Encoding.UTF8, true);
            var s0 = 12;
            var s1 = "Test short string ASCII";
            var s2 =
                @"Text eamble will be encoded possibly in UTF8 in order to test the functionality of binary simbiosi encodecoder \r\n ┬­}Ñ=`l";
            var s3 = 125;
            var s4 = (ushort) 612;
            var s5 = (short)612;
            var s6 = (long) 155263115;
            var s7 = (double) 155214.23;
            var s8 = (decimal) 12121233568.12;
            var s9 = DateTime.Now;
            
            
            w.Write7BitEncodedInt(s0);
            w.WriteASCIIShortString(s1);
            w.Write(s2);
            w.Write(s3);
            w.Write(s4);
            w.Write(s5);
            w.Write(s6);
            w.Write(s7);
            w.Write(s8);
            w.Write(s9);
            w.Flush();

            m.Position = 0;
            var r  = new SimbiosiStreamReader(m, Encoding.UTF8, true);
            var l0 = r.Read7BitEncodedInt();
            var l1 = r.ReadASCIIShortString();
            var l2 = r.ReadString();
            var l3 = r.ReadInt32();
            var l4 = r.ReadUInt16();
            var l5 = r.ReadInt16();
            var l6 = r.ReadInt64();
            var l7 = r.ReadDouble();
            var l8 = r.ReadDecimal();
            var l9 = r.ReaDateTime();

            Assert.IsTrue(
                s0==l0&&
                s1==l1&&
                s2==l2&&
                s3 == l3 &&
                s4 == l4 &&
                s5 == l5 &&
                s6 == l6 &&
                Math.Abs(s7 - l7) < double.Epsilon &&
                s8 == l8 &&
                s9 == l9 
                );

        }
    }
}
