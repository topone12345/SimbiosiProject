using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimbiosiClientLib.Entities.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimbiosiClientLib.Entities.Tags.Tests
{
    [TestClass()]
    public class SubscribeTagTests
    {
        private TagsCollection _collection;

        [TestMethod()]
        [ExpectedException(typeof( ArgumentException))]
        public void CreateWrongSubscribeTag1()
        {
            var s = new SubscribeTag("PIPPO.<.PALLO");
            Assert.Inconclusive();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWrongSubscribeTag2()
        {
            var s = new SubscribeTag("PIPPO.>.PALLO");
            Assert.Inconclusive();
        }






        [TestMethod()]
        public void MatchWithTest1()
        {
            var s = new SubscribeTag("PIPPO.PLUTO.PAPERINO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest2()
        {
            var s = new SubscribeTag("PIPPO.*.PAPERINO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest3()
        {
            var s = new SubscribeTag("PIPPO.*.*");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest4()
        {
            var s = new SubscribeTag("PIPPO.PLUTO.LL");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest5()
        {
            var s = new SubscribeTag("PIPPO.*.LL");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest6()
        {
            var s = new SubscribeTag("PIPPO.PLUTO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest7()
        {
            var s = new SubscribeTag("PIPPO.PLUTO.LOLL.POKK");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest8()
        {
            var s = new SubscribeTag("PIPPO.>");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest9()
        {
            var s = new SubscribeTag("PIPPO.PLUTO.PAPERINO.ARCHIMEDE");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest10()
        {
            var s = new SubscribeTag("PIPPO.PLUTO.PAPERINO.*");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest11()
        {
            var s = new SubscribeTag("*.*.*.*");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest12()
        {
            var s = new SubscribeTag("*.*.*");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest13()
        {
            var s = new SubscribeTag(">");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }


        [TestMethod()]
        public void MatchWithTest14()
        {
            var s = new SubscribeTag("<");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }


        [TestMethod()]
        public void MatchWithTest15()
        {
            var s = new SubscribeTag("PIPPO.>");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest16()
        {
            var s = new SubscribeTag("<.PAPERINO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }


        [TestMethod()]
        public void MatchWithTest16Bis()
        {
            var s = new SubscribeTag("<.PAPERINO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PAP");
            Assert.IsFalse(s.MatchWith(m), "Should be different");
        }

        [TestMethod()]
        public void MatchWithTest17()
        {
            var s = new SubscribeTag("<.PLUTO.>");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest18()
        {
            var s = new SubscribeTag("<.PLUTO.PAPERINO.PALLO.PIATTOLA");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsTrue(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest19()
        {
            var s = new SubscribeTag("<.PLUTO.PAPERINO.PALLO.PIATTOLA.BARBABIETOLA.CALAMARO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsFalse(s.MatchWith(m), "Should be equals");
        }

        [TestMethod()]
        public void MatchWithTest20()
        {
            var s = new SubscribeTag("<.PLUTO.PAPERINO.PALLO.PIATTOLAZZA");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsFalse(s.MatchWith(m), "Should be equals");
        }


        [TestMethod()]
        public void MatchWithTest21()
        {
            var s = new SubscribeTag("<.PLUTO");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsFalse(s.MatchWith(m), "Should be false");
        }

        [TestMethod()]
        public void MatchWithTest22()
        {
            var s = new SubscribeTag("<.PAPERINO.*.*");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsTrue(s.MatchWith(m));
        }

        [TestMethod()]
        public void MatchWithTest23()
        {
            var s = new SubscribeTag("<.MERDA");
            var m = new MessageTag("PIPPO.PLUTO.PAPERINO.PALLO.PIATTOLA");
            Assert.IsFalse(s.MatchWith(m));
        }

        [TestMethod()]
        public void MatchWithTest24()
        {
            var s = new SubscribeTag("<.MERDA");
            var m = new MessageTag("PIPPO");
            Assert.IsFalse(s.MatchWith(m));
        }

        [TestMethod()]
        public void CreateTagClass()
        {
            _collection = new TagsCollection();
            _collection.Add(new SubscribeTag("TRANSACTIONS.DONE.SUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.DONE.UNSUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.SUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.ERROR"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.TYPE2.>"));
            _collection.Add(new SubscribeTag("<.ERROR"));
            _collection.Add(new SubscribeTag("<.TYPE"));
            _collection.Add(new SubscribeTag("<.119"));
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void RemoveAll()
        {
            _collection = new TagsCollection();
            _collection.Add(new SubscribeTag("TRANSACTIONS.DONE.SUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.DONE.UNSUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.SUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.ERROR"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.*"));
            _collection.Add(new SubscribeTag("TRANSACTIONS.TYPE2.>"));
            _collection.Add(new SubscribeTag("<.ERROR"));
            _collection.Add(new SubscribeTag("<.TYPE"));
            _collection.Add(new SubscribeTag("<.119"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.DONE.SUCCESFULLY.*"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.DONE.UNSUCCESFULLY.*"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.INPROGRESS.SUCCESFULLY.*"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.*"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.INPROGRESS.UNSUCCESFULLY.ERROR"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.*"));
            _collection.Remove(new SubscribeTag("TRANSACTIONS.TYPE2.>"));
            _collection.Remove(new SubscribeTag("<.ERROR"));
            _collection.Remove(new SubscribeTag("<.TYPE"));
            _collection.Remove(new SubscribeTag("<.119"));
            Assert.IsTrue(true);
        }
    }
}