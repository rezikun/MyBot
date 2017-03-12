using Bot_Application2;

namespace Recommendations
{

    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public class RecommendationsApp
    {
        public static RecommendationsApiWrapper reco = MessagesController.recommender;
        /// <summary>
        /// Creates a model, upload catalog and usage file and trigger a build.
        /// Returns the Build ID of the trained build.
        /// </summary>
        /// <param name="recommender">Wrapper that maintains API key</param>
        /// <param name="modelId">The model Id</param>
        public static string CreateModel(string modelName)
        {
            string modelId;

            ModelInfo modelInfo = reco.CreateModel(modelName, "Bot");
            modelId = modelInfo.Id;

            return modelId;
        }

        public static long UploadDataAndTrainModel(string modelId, BuildType buildType = BuildType.Recommendation)
        {
            long buildId = -1;
            // Trigger a recommendation build.
            string operationLocationHeader;

            if (buildType == BuildType.Recommendation)
            {
                buildId = reco.CreateRecommendationsBuild(modelId, "Recommendation Build " + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                enableModelInsights: false,
                operationLocationHeader: out operationLocationHeader);
            }
            else
            {
                buildId = reco.CreateFbtBuild(modelId, "Frequenty-Bought-Together Build " + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                enableModelInsights: false,
                operationLocationHeader: out operationLocationHeader);
            }

            // Monitor the build and wait for completion.
            var buildInfo = reco.WaitForOperationCompletion<BuildInfo>(RecommendationsApiWrapper.GetOperationId(operationLocationHeader));
            // Waiting  in order to propagate the model updates from the build...
            Thread.Sleep(TimeSpan.FromSeconds(40));


            reco.SetActiveBuild(modelId, buildId);

            return buildId;
        }

        /// <summary>
        /// Shows how to get item-to-item recommendations and user-to-item-recommendations
        /// </summary>
        /// <param name="recommender">Wrapper that maintains API key</param>
        /// <param name="modelId">Model ID</param>
        /// <param name="buildId">Build ID</param>
        public static void GetRecommendationsSingleRequest(RecommendationsApiWrapper recommender, string modelId, long buildId, ref string I2I, ref string U2I, int ID, string userID)
        {
            // Get item to item recommendations. (I2I)

            string itemIds = ID.ToString();
            var itemSets = recommender.GetRecommendations(modelId, buildId, itemIds, 6);
            if (itemSets.RecommendedItemSetInfo != null)
            {
                foreach (RecommendedItemSetInfo recoSet in itemSets.RecommendedItemSetInfo)
                {
                    foreach (var item in recoSet.Items)
                    {
                        I2I = I2I + item.Name + "                 " + Environment.NewLine;
                    }
                }
            }
            else
            {
                I2I = "No recommendations found.";
            }

            // Now let's get a user recommendation (U2I)

            MessagesController.OnlyOnce = false;
            itemSets = recommender.GetUserRecommendations(modelId, buildId, userID, 6);
            if (itemSets.RecommendedItemSetInfo != null)
            {
                foreach (RecommendedItemSetInfo recoSet in itemSets.RecommendedItemSetInfo)
                {
                    foreach (var item in recoSet.Items)
                    {
                        U2I = U2I + item.Name + "                     " + Environment.NewLine;
                    }
                }
            }
            else
            {
                U2I = "No recommendations found.";
            }

        }
    }
}