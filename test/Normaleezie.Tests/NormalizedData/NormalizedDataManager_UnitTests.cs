﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Normaleezie.NormalizedData;
using Normaleezie.NormalizedStructure;
using Normaleezie.Tests.Test_Classes;
using Xunit;
using Xunit.Extensions;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace Unit.NormalizedData
{
    #region CreateNormalizedData

    public class CreateNormalizedData
    {
        private readonly NormalizedDataManager normalizedDataManager;

        public CreateNormalizedData()
        {
            normalizedDataManager = new NormalizedDataManager();

        }

    }

    #endregion

    #region CreateNormalizedDataForListOfSimpleType

    #endregion

    #region CreateNormalizedDataForListOfComplexType

    #endregion

    #region CreateNormalizedDataForListOfIEnumerableType

    #endregion

    #region CallCreateNormalizedDataGenerically

    #endregion

    #region CheckForCircularReferences

    public class CheckForCircularReferences
    {
        private readonly NormalizedDataManager normalizedDataManager;

        public CheckForCircularReferences()
        {
            normalizedDataManager = new NormalizedDataManager();
        }

        public static IEnumerable<object[]> ValidScenarioArguments = new[]
        {
            new object[] {new List<string> {}, "Test" },
            new object[] {null, "Test" },
            new object[] {new List<string> { "Test" }, null },
            new object[] {new List<string> { "Test" }, string.Empty },
            new object[] {new List<string> { "Test", "Test", "Test", "Test", "Test" }, "Test" },
            new object[] {new List<string>
            {
                "Test1",
                "Test2",
                "Test3",
                "Test4",
                "Test5",
                "Test6",
                "Test7",
                "Test8",
                "Test9",
                "Test10",
                "Test11",
                "Test12",
                "Test13",
                "Test14",
                "Test15",
                "Test16",
                "Test17",
                "Test18",
                "Test19",
                "Test20",
                "Test21",
                "Test22",
                "Test23",
                "Test24",
                "Test25",
                "Test26",
                "Test27",
                "Test28",
                "Test29",
                "Test30"
            }, "Test" },
            new object[] {new List<string>
            {
                "Test",
                "Test",
                "Test",
                "Test",
                "Test"
            }, "Test" }
        };

        [Theory]
        [MemberData("ValidScenarioArguments")]
        public void Should_NOT_Throw_An_Exception_When_There_Is_No_Circular_Reference(List<string> previousDataNames, string dataName )
        {
            normalizedDataManager.CheckForCircularReferences(ref previousDataNames, dataName);

            Assert.Equal(dataName, previousDataNames.Last());
        }

        public static IEnumerable<object[]> InvalidScenarioArguments = new[]
        {
            new object[] {new List<string>
            {
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
                "Test",
            }, "Test" }
        };

        [Theory]
        [MemberData("InvalidScenarioArguments")]
        public void Should_Throw_An_Exception_When_Parameters_Indicate_There_Is_A_Circular_Reference(List<string> previousDataNames, string dataName)
        {
            Exception exception = Assert.Throws<Exception>(() => normalizedDataManager.CheckForCircularReferences(ref previousDataNames, dataName));

            Assert.Equal("Circular Reference Detected in object.", exception.Message);
        }
    }

    #endregion
}
