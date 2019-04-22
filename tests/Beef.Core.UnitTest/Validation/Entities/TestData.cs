// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Validation.Entities
{
    public class TestDataBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class TestData : TestDataBase
    {
        [JsonProperty("datefrom")]
        public DateTime DateA { get; set; }

        [JsonProperty("dateto")]
        public DateTime? DateB { get; set; }

        public int CountA { get; set; }

        public int? CountB { get; set; }

        public decimal AmountA { get; set; }

        public decimal? AmountB { get; set; }

        public double DoubleA { get; set; }

        public double? DoubleB { get; set; }

        public bool SwitchA { get; set; }

        public bool? SwitchB { get; set; }

        public List<int> Vals { get; set; }
    }
}
