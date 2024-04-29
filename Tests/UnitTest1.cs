using Core.Exceptions;
using Core.Interface;
using Core.Models;
using NUnit.Framework;

namespace PowerPlantCodingChallenge.Test.Services.Planners
{
    public class BruteForceTreeGenerationProductionPlanPlannerScenarios
    {
        private IComputeSolution _compute;

        private Fuels _fuels;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_NotEnough()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

            // arrange + act
            Payload productionPlan = new(500, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 50, 100),
                new("Gas2", "gasfired", 0.5m, 50, 100)
            });

            // assert
            NUnit.Framework.Assert.Throws(typeof(CustomException), () => _compute.Solution(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_TooMuch()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

            // arrange + act
            Payload productionPlan = new(20, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 50, 100),
                new("Wind1", "windturbine", 1, 0, 50)
            });

            // assert
            NUnit.Framework.Assert.Throws(typeof(CustomException), () => _compute.Solution(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_Enough()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

            // arrange + act
            Payload productionPlan = new(25, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 10, 100),
                new("Wind1", "windturbine", 1, 0, 50)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(25, result.PowerplantResponses.First(x => x.Name == "Wind1").Power);
            NUnit.Framework.Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_NotEnough()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

            // arrange + act
            Payload productionPlan = new(50, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 10, 100),
                new("Wind1", "windturbine", 1, 0, 50)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(25, result.PowerplantResponses.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(25, result.PowerplantResponses.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_TooMuch()
        {
            // arrange
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

            // arrange + act
            Payload productionPlan = new(20, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 10, 100),
                new("Wind1", "windturbine", 1, 0, 50)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(0, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(20, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Efficiency()
        {
            _fuels = new Fuels() { CO2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };
            // arrange
            Payload productionPlan = new(20, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 10, 100),
                new("Gas2", "gasfired", 0.6m, 10, 100),
                new("Gas3", "gasfired", 0.8m, 10, 100),
                new("Gas4", "gasfired", 0.3m, 10, 100),
                new("Gas5", "gasfired", 0.45m, 10, 100)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(20, result.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(0, result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum());
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_AllNeeded()
        {
            // arrange
            Payload productionPlan = new(490, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.5m, 10, 100),
                new("Gas2", "gasfired", 0.6m, 10, 100),
                new("Gas3", "gasfired", 0.8m , 10, 100),
                new("Gas4", "gasfired", 0.3m, 10, 100),
                new("Gas5", "gasfired", 0.45m, 10, 100)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(90, result.PowerplantResponses.First(x => x.Name == "Gas4").Power);
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "Gas5").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Pmin()
        {
            // arrange
            Payload productionPlan = new(125, _fuels, new List<Powerplant>()
            {
                new("Wind1", "windturbine", 1, 0, 50),
                new("Gas1", "gasfired", 0.5m, 110, 200),
                new("Gas2", "gasfired", 0.8m, 80, 150)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Kerosine()
        {
            // arrange
            Payload productionPlan = new(100, _fuels, new List<Powerplant>()
            {
                new("Wind1", "windturbine", 1, 0, 150),
                new("Gas1", "gasfired" , 0.5m, 100, 200),
                new("Kerosine1", "turbojet", 0.5m, 0, 200)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(25, result.PowerplantResponses.First(x => x.Name == "Kerosine1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_CO2Impact()
        {
            // arrange
            Payload productionPlan = new(150, _fuels, new List<Powerplant>()
            {
                new("Gas1", "gasfired", 0.3m, 100, 200),
                new("Kerosine1", "turbojet", 1, 0, 200)
            });

            // act
            var resultNoCO2 = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(150, resultNoCO2.PowerplantResponses.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_TrickyTest1()
        {
            // arrange
            Fuels energyMetrics = new() { CO2 = 0, Kerosine = 50.8m, Gas = 20, Wind = 100 };
            Payload productionPlan = new(60, energyMetrics, new List<Powerplant> {
                new("windpark1", "windturbine", 1, 0, 20),
                new("gasfired", "gasfired", 0.9m, 50, 100),
                new("gasfiredinefficient", "gasfired", 0.1m, 0, 100)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(60, result.PowerplantResponses.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(60, result.PowerplantResponses.First(x => x.Name == "gasfired").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredinefficient").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_TrickyTest2()
        {
            // arrange
            Fuels energyMetrics = new() { CO2 = 0, Kerosine = 50.8m, Gas = 20, Wind = 100 };
            Payload productionPlan = new(80, energyMetrics, new List<Powerplant> {
                new("windpark1", "windturbine", 1, 0, 60),
                new("gasfired", "gasfired", 0.9m, 50, 100),
                new("gasfiredinefficient", "gasfired", 0.1m, 0, 200)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(80, result.PowerplantResponses.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(80, result.PowerplantResponses.First(x => x.Name == "gasfired").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredinefficient").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload1_NoCO2()
        {
            // arrange
            Fuels energyMetrics = new() { CO2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 60 };
            Payload productionPlan = new(480, energyMetrics, new List<Powerplant> {
                new("gasfiredbig1", "gasfired", 0.53m, 100, 460),
                new("gasfiredbig2", "gasfired", 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", "gasfired", 0.37m, 40, 210),
                new("tj1", "turbojet", 0.3m, 0, 16),
                new("windpark1", "windturbine", 1, 0, 150),
                new("windpark2", "windturbine", 1, 0, 36)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(480, result.PowerplantResponses.Select(x => x.Power).Sum());
            Assert.AreEqual(90, result.PowerplantResponses.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(21.6, result.PowerplantResponses.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(368.4, result.PowerplantResponses.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "tj1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload2_NoCO2()
        {
            // arrange
            Fuels energyMetrics = new() { CO2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 0 };
            Payload productionPlan = new(480, energyMetrics, new List<Powerplant> {
                new("gasfiredbig1", "gasfired", 0.53m, 100, 460),
                new("gasfiredbig2", "gasfired", 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", "gasfired", 0.37m, 40, 210),
                new("tj1", "turbojet", 0.3m, 0, 16),
                new("windpark1", "windturbine", 1, 0, 150),
                new("windpark2", "windturbine", 1, 0, 36)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(480, result.PowerplantResponses.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(380, result.PowerplantResponses.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(100, result.PowerplantResponses.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "tj1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload3_NoCO2()
        {
            // arrange
            Fuels energyMetrics = new() { CO2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 60 };
            Payload productionPlan = new(910, energyMetrics, new List<Powerplant> {
                new("gasfiredbig1", "gasfired", 0.53m, 100, 460),
                new("gasfiredbig2", "gasfired", 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", "gasfired", 0.37m, 40, 210),
                new("tj1", "turbojet", 0.3m, 0, 16),
                new("windpark1", "windturbine", 1, 0, 150),
                new("windpark2", "windturbine", 1, 0, 36)
            });

            // act
            var result = _compute.Solution(productionPlan);

            // assert
            Assert.AreEqual(910, result.PowerplantResponses.Select(x => x.Power).Sum());
            Assert.AreEqual(90, result.PowerplantResponses.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(21.6, result.PowerplantResponses.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(460, result.PowerplantResponses.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(338.4, result.PowerplantResponses.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.PowerplantResponses.First(x => x.Name == "tj1").Power);
        }
    }
}