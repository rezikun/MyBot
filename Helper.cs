
namespace Recommendations
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Newtonsoft.Json.Converters;
    #region response classes
    public enum OperationType
    {
        BuildModel = 0, // build is in queue waiting for execution
    }

    public enum OperationStatus
    {
        NotStarted = 0, // build is in queue waiting for execution
        Running = 1,  // build is in progress
        Cancelling = 2, // build is in the process of cancellation
        Cancelled = 3, // build was cancelled
        Succeeded = 4, // build ended with success
        Failed = 5, // build ended with error
    }



    public class BuildInfo
    {

        [JsonProperty("id")]
        public long Id { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }


        [JsonProperty("type")]
        public BuildType Type { get; set; }


        [JsonProperty("modelName")]
        public string ModelName { get; set; }


        [JsonProperty("modelId")]
        public string ModelId { get; set; }


        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }


        [JsonProperty("startDateTime")]
        public string StartDateTime { get; set; }


        [JsonProperty("endDateTime")]
        public string EndDateTime { get; set; }


        [JsonProperty("modifiedDateTime")]
        public string ModifiedDateTime { get; set; }

    }



    public class BuildInfoList
    {

        [JsonProperty("builds")]
        public IEnumerable<BuildInfo> Builds { get; set; }
    }



    public class BuildModelResponse
    {

        [JsonProperty("buildId")]
        public long BuildId { get; set; }
    }


    public class CatalogImportStats
    {

        [JsonProperty("processedLineCount")]
        public int ProcessedLineCount { get; set; }


        [JsonProperty("errorLineCount")]
        public int ErrorLineCount { get; set; }


        [JsonProperty("importedLineCount")]
        public int ImportedLineCount { get; set; }


        [JsonProperty("errorSummary")]
        public IEnumerable<ImportErrorStats> ErrorSummary { get; set; }

    }


    public class ImportErrorStats
    {

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }


        [JsonProperty("errorCodeCount")]
        public int ErrorCodeCount { get; set; }
    }



    public class ModelInfo
    {

        [JsonProperty("id")]
        public string Id { get; set; }


        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }


        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }


        [JsonProperty("activeBuildId")]
        public long ActiveBuildId { get; set; }

    }


    public class ModelInfoList
    {

        [JsonProperty("models")]
        public IEnumerable<ModelInfo> Models { get; set; }
    }


    public class OperationInfo<T>
    {

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }


        [JsonProperty("lastActionDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public string LastActionDateTime { get; set; }


        [JsonProperty("percentComplete", NullValueHandling = NullValueHandling.Ignore)]
        public int PercentComplete { get; set; }


        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }


        [JsonProperty("resourceLocation", NullValueHandling = NullValueHandling.Ignore)]
        public string ResourceLocation { get; set; }


        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }
    }



    public class RecommendedItemInfo
    {

        [JsonProperty("id")]
        public string Id { get; set; }


        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("metadata")]
        public string Metadata { get; set; }
    }

    /// <summary>
    /// Holds a recommendation result, which is a set of recommended items with reasoning and rating/score.
    /// </summary>

    public class RecommendedItemSetInfo
    {
        public RecommendedItemSetInfo()
        {
            Items = new List<RecommendedItemInfo>();
        }


        [JsonProperty("items")]
        public IEnumerable<RecommendedItemInfo> Items { get; set; }


        [JsonProperty("rating")]
        public double Rating { get; set; }


        [JsonProperty("reasoning")]
        public IEnumerable<string> Reasoning { get; set; }
    }



    public class RecommendedItemSetInfoList
    {

        [JsonProperty("recommendedItems")]
        public IEnumerable<RecommendedItemSetInfo> RecommendedItemSetInfo { get; set; }
    }


    public class UsageImportStats : CatalogImportStats
    {

        [JsonProperty("fileId")]
        public string FileId { get; set; }

    }

    #endregion

    #region request classes


    public class BuildParameters
    {

        [JsonProperty("ranking")]
        public RankingBuildParameters Ranking { get; set; }


        [JsonProperty("recommendation")]
        public RecommendationBuildParameters Recommendation { get; set; }


        [JsonProperty("fbt")]
        public FbtBuildParameters Fbt { get; set; }
    }



    public class BuildRequestInfo
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("buildType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BuildType BuildType { get; set; }

        [JsonProperty("buildParameters")]
        public BuildParameters BuildParameters { get; set; }
    }



    public class UpdateActiveBuildInfo
    {
        [JsonProperty("activeBuildId")]
        public long ActiveBuildId { get; set; }
    }


    public class BatchJobInfo
    {
        /// <summary>
        /// Unique batch identifier
        /// </summary>

        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Description of the batch request
        /// </summary>

        [JsonProperty("requestinfo")]
        public BatchJobsRequestInfo RequestInfo { get; set; }

        /// <summary>
        /// Status of the batch job (it is actually the job status: Registered, Ready, InProgress, Canceling, Canceled, Succeeded, Failed)
        /// </summary>

        [JsonProperty("status")]
        public string Status { get; set; }
    }


    public class BatchJobsRequestInfo
    {
        /// <summary>
        /// The input storage blob info
        /// </summary>
        [JsonProperty("input")]
        public StorageBlobInfo Input { get; set; }

        /// <summary>
        /// The output storage blob info
        /// </summary>
        [JsonProperty("output")]
        public StorageBlobInfo Output { get; set; }

        /// <summary>
        /// The error storage blob info
        /// </summary>
        [JsonProperty("error")]
        public StorageBlobInfo Error { get; set; }

        /// <summary>
        /// The job info
        /// </summary>
        [JsonProperty("job")]
        public JobInfo Job { get; set; }
    }


    public class StorageBlobInfo
    {
        /// <summary>
        /// Authentication Type
        /// value "PublicOrSas"
        /// </summary>
        [JsonProperty("authenticationType")]
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Base Location
        /// ex: "https://{storage name}.blob.core.windows.net/"
        /// </summary>
        [JsonProperty("baseLocation")]
        public string BaseLocation { get; set; }

        /// <summary>
        /// The relative location, including the container name
        /// </summary>
        [JsonProperty("relativeLocation")]
        public string RelativeLocation { get; set; }

        /// <summary>
        /// The sasToken to access the file
        /// </summary>
        [JsonProperty("sasBlobToken")]
        public string SasBlobToken { get; set; }
    }


    public class JobInfo
    {
        /// <summary>
        /// Api Name
        /// The ApiName is internally an enum (see SupportedApis in BatchScoringManager)
        /// The valid values should be: ItemRecommend, UserRecommend, ItemFbtRecommend
        /// </summary>
        [JsonProperty("apiName")]
        public string ApiName { get; set; }

        /// <summary>
        /// The Model Id
        /// ModelId is the model id which batch scoring is requested to
        /// </summary>
        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        /// <summary>
        /// The build Id
        /// BuildId is the build id which batch scoring is requested to
        /// It is optional. If it is not provided, the active build id will be used
        /// </summary>
        [JsonProperty("buildId")]
        public long BuildId { get; set; }

        /// <summary>
        /// Number of recommendations
        /// It indicates the number of results (recommended items) each request should return
        /// It is optional. The default value is 10
        /// </summary>
        [JsonProperty("numberOfResults")]
        public int NumberOfResults { get; set; }

        /// <summary>
        /// Include Metadata
        /// it indicates whether the result should include metadata or not
        /// It is optional. The default value is false
        /// </summary>
        [JsonProperty("includeMetadata")]
        public bool IncludeMetadata { get; set; }

        /// <summary>
        /// The minimum score. Currently only supported for FbtBuilds
        /// It indicates the minimal score to return
        /// It is optional. The default value is 0.1
        /// </summary>
        [JsonProperty("minimalScore")]
        public double MinimalScore { get; set; }
    }


    public class BatchJobsResponse
    {
        /// <summary>
        /// Unique batch job identifier 
        /// </summary>

        [JsonProperty("batchId")]
        public string BatchId { get; set; }
    }

    /// <summary>
    /// Types of build
    /// </summary>

    public enum BuildType
    {
        /// <summary>
        /// Build that will create a model for recommendation
        /// </summary>
        Recommendation = 1,

        /// <summary>
        /// A build that creates a model to score features.
        /// </summary>
        Ranking = 2,

        /// <summary>
        /// A build that creates a model for fbt
        /// </summary>
        Fbt = 3,
    }

    /// <summary>
    /// FBT similarity functions
    /// </summary>

    public enum FbtSimilarityFunction
    {
        /// <summary>
        /// Count of co-occurrences, favors predictability.
        /// </summary>
        Cooccurrence,
        /// <summary>
        /// Lift favors serendipity
        /// </summary>
        Lift,
        /// <summary>
        /// Jaccard is a compromise between co-occurrences and lift.
        /// </summary>
        Jaccard
    }



    /// <summary>
    /// FBT similarity functions
    /// </summary>

    public enum SplitterStrategy
    {
        /// <summary>
        /// Takes last transaction of users as the test set to use for evaluation.
        /// </summary>
        LastEventSplitter,
        /// <summary>
        /// Takes a random set of transactions as the test set to use for evaluation.
        /// </summary>
        RandomSplitter
    }



    public class RandomSplitterParameters
    {

        [JsonProperty("testPercent")]
        public int? TestPercent { get; set; }


        [JsonProperty("randomSeed")]
        public int? RandomSeed { get; set; }
    }


    public class FbtBuildParameters
    {

        [JsonProperty("supportThreshold")]
        public int? SupportThreshold { get; set; }


        [JsonProperty("maxItemSetSize")]
        public int? MaxItemSetSize { get; set; }


        [JsonProperty("minimalScore")]
        public double? MinimalScore { get; set; }


        [JsonProperty("similarityFunction")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FbtSimilarityFunction SimilarityFunction { get; set; }


        [JsonProperty("enableModelingInsights")]
        public bool? EnableModelingInsights { get; set; }


        [JsonProperty("splitterStrategy")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SplitterStrategy SplitterStrategy { get; set; }


        [JsonProperty("randomSplitterParameters")]
        public RandomSplitterParameters RandomSplitterParameters { get; set; }

    }




    public class RecommendationBuildParameters
    {

        [JsonProperty("numberOfModelIterations")]
        public int? NumberOfModelIterations { get; set; }


        [JsonProperty("numberOfModelDimensions")]
        public int? NumberOfModelDimensions { get; set; }


        [JsonProperty("itemCutOffLowerBound")]
        public int? ItemCutOffLowerBound { get; set; }


        [JsonProperty("itemCutOffUpperBound")]
        public int? ItemCutOffUpperBound { get; set; }


        [JsonProperty("userCutOffLowerBound")]
        public int? UserCutOffLowerBound { get; set; }


        [JsonProperty("userCutOffUpperBound")]
        public int? UserCutOffUpperBound { get; set; }


        [JsonProperty("enableModelingInsights")]
        public bool? EnableModelingInsights { get; set; }


        [JsonProperty("splitterStrategy")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SplitterStrategy SplitterStrategy { get; set; }



        [JsonProperty("randomSplitterParameters")]
        public RandomSplitterParameters RandomSplitterParameters { get; set; }


        [JsonProperty("useFeaturesInModel")]
        public bool? UseFeaturesInModel { get; set; }


        [JsonProperty("modelingFeatureList")]
        public string ModelingFeatureList { get; set; }


        [JsonProperty("allowColdItemPlacement")]
        public bool? AllowColdItemPlacement { get; set; }


        [JsonProperty("enableFeatureCorrelation")]
        public bool? EnableFeatureCorrelation { get; set; }


        [JsonProperty("reasoningFeatureList")]
        public string ReasoningFeatureList { get; set; }


        [JsonProperty("enableU2I")]
        public bool? EnableU2I { get; set; }

    }


    public class ModelRequestInfo
    {

        [JsonProperty("modelName")]
        public string ModelName { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }
    }


    public class RankingBuildParameters
    {

        [JsonProperty("numberOfModelIterations")]
        public int? NumberOfModelIterations { get; set; }


        [JsonProperty("numberOfModelDimensions")]
        public int? NumberOfModelDimensions { get; set; }


        [JsonProperty("itemCutOffLowerBound")]
        public int? ItemCutOffLowerBound { get; set; }


        [JsonProperty("itemCutOffUpperBound")]
        public int? ItemCutOffUpperBound { get; set; }


        [JsonProperty("userCutOffLowerBound")]
        public int? UserCutOffLowerBound { get; set; }


        [JsonProperty("userCutOffUpperBound")]
        public int? UserCutOffUpperBound { get; set; }



        public class RecommendationBuildParameters
        {

            [JsonProperty("numberOfModelIterations")]
            public int? NumberOfModelIterations { get; set; }


            [JsonProperty("numberOfModelDimensions")]
            public int? NumberOfModelDimensions { get; set; }


            [JsonProperty("itemCutOffLowerBound")]
            public int? ItemCutOffLowerBound { get; set; }


            [JsonProperty("itemCutOffUpperBound")]
            public int? ItemCutOffUpperBound { get; set; }


            [JsonProperty("userCutOffLowerBound")]
            public int? UserCutOffLowerBound { get; set; }


            [JsonProperty("userCutOffUpperBound")]
            public int? UserCutOffUpperBound { get; set; }


            [JsonProperty("enableModelingInsights")]
            public bool? EnableModelingInsights { get; set; }


            [JsonProperty("useFeaturesInModel")]
            public bool? UseFeaturesInModel { get; set; }


            [JsonProperty("modelingFeatureList")]
            public string ModelingFeatureList { get; set; }


            [JsonProperty("allowColdItemPlacement")]
            public bool? AllowColdItemPlacement { get; set; }


            [JsonProperty("enableFeatureCorrelation")]
            public bool? EnableFeatureCorrelation { get; set; }


            [JsonProperty("reasoningFeatureList")]
            public string ReasoningFeatureList { get; set; }


            [JsonProperty("enableU2I")]
            public bool? EnableU2I { get; set; }

        }



        public class UpdateModelRequestInfo
        {

            [JsonProperty("description")]
            public string Description { get; set; }


            [JsonProperty("activeBuildId")]
            public long? ActiveBuildId { get; set; }
        }
    }

    #endregion

}