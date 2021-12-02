﻿namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Results
    {
        [JsonProperty("jobIdentifier")]
        public Guid JobIdentifier { get; set; }

        [JsonProperty("accountIdentifier")]
        public string AccountIdentifier { get; set; } = string.Empty;

        [JsonProperty("team")]
        public Team Team { get; set; } = new Team();

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("completed")]
        public long Completed { get; set; }

        [JsonProperty("failed")]
        public long Failed { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }

        [JsonProperty("submittedByKey")]
        public string SubmittedByKey { get; set; } = string.Empty;

        [JsonProperty("explained")]
        public bool Explained { get; set; }

        [JsonProperty("submittedAt")]
        public DateTimeOffset SubmittedAt { get; set; }

        [JsonProperty("initialQueueTime")]
        public long InitialQueueTime { get; set; }

        [JsonProperty("totalQueueTime")]
        public long TotalQueueTime { get; set; }

        [JsonProperty("averageModelLatency")]
        public long AverageModelLatency { get; set; }

        [JsonProperty("totalModelLatency")]
        public long TotalModelLatency { get; set; }

        [JsonProperty("elapsedTime")]
        public long ElapsedTime { get; set; }

        [JsonProperty("startingResultSummarizing")]
        public DateTimeOffset StartingResultSummarizing { get; set; }

        [JsonProperty("resultSummarizing")]
        public long ResultSummarizing { get; set; }

        [JsonProperty("inputSize")]
        public long InputSize { get; set; }

        [JsonProperty("results")]
        public Dictionary<string, ResultsClass> ResultsResults { get; set; } = new Dictionary<string, ResultsClass>();
    }

    public partial class ResultsClass
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("engine")]
        public string Engine { get; set; } = String.Empty;

        [JsonProperty("inputFetching")]
        public long InputFetching { get; set; }

        [JsonProperty("outputUploading")]
        public object OutputUploading { get; set; } = new object();

        [JsonProperty("modelLatency")]
        public long ModelLatency { get; set; }

        [JsonProperty("queueTime")]
        public long QueueTime { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; } = string.Empty;

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; } = string.Empty;

        [JsonProperty("endTime")]
        public string EndTime { get; set; } =  string.Empty;

        [JsonProperty("results.json")]
        public ResultsJson ResultsJson { get; set; } = new ResultsJson();

        [JsonProperty("voting")]
        public Voting Voting { get; set; } = new Voting();
    }

    public partial class ResultsJson
    {
        [JsonProperty("data")]
        public ResultsJsonData Data { get; set; } = new ResultsJsonData();
    }

    public partial class ResultsJsonData
    {
        [JsonProperty("result")]
        public Result Result { get; set; } = new Result();

        [JsonProperty("explanation")]
        public object Explanation { get; set; } = new object();

        [JsonProperty("drift")]
        public object Drift { get; set; } = new object();
    }

    public partial class Result
    {
        [JsonProperty("classPredictions")]
        public ClassPrediction[] ClassPredictions { get; set; } = Array.Empty<ClassPrediction>();
    }

    public partial class ClassPrediction
    {
        [JsonProperty("class")]
        public string Class { get; set; } = string.Empty;

        [JsonProperty("score")]
        public long Score { get; set; }
    }

    public partial class Voting
    {
        [JsonProperty("up")]
        public long Up { get; set; }

        [JsonProperty("down")]
        public long Down { get; set; }
    }


}