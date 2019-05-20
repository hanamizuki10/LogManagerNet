using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using LogManagerNet;

namespace LogManagerNetUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1_base()
        {
            LogManager logger = LogManager.GetInstance();
            logger.Debug("テスト");
            logger.Info("テスト");
            logger.Warn("テスト");
            logger.Error("テスト");
            logger.Fatal("テスト");

        }
        [TestMethod]
        public void TestMethod1_type1()
        {


            LogManager logger2 = LogManager.GetInstance("type1");
            logger2.Debug("テスト");
            logger2.Info("テスト");
            logger2.Warn("テスト");
            logger2.Error("テスト");
            logger2.Fatal("テスト");



        }
        [TestMethod]
        public void TestMethod1_type2()
        {



            LogManager logger3 = LogManager.GetInstance("type2");
            logger3.Debug("テスト");
            logger3.Info("テスト");
            logger3.Warn("テスト");
            logger3.Error("テスト");
            logger3.Fatal("テスト");
        }

        [TestMethod]
        public void TestMethod2()
        {
            LogManager logger = LogManager.GetInstance();
            LogManager logger2 = LogManager.GetInstance("type1");

            for (int i = 0; i < 9999999; i++)
            {
                logger2.Debug("テスト");
                logger2.Info("テスト");
                logger2.Warn("テスト");
                logger2.Error("テスト");
                logger2.Fatal("テスト");
                logger.Debug("テスト");
                logger.Info("テスト");
                logger.Warn("テスト");
                logger.Error("テスト");
                logger.Fatal("テスト");
                Task.Delay(100);   // 1000ミリ秒待機
            }
        }
    }
}
