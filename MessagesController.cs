using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using MyBot;
using APIJSON;
using static MyBot.Search;
using TMDbLib.Objects.General;
using TMDbLib.Client;
using TMDbLib.Objects.Search;
using System.IO;
using Recommendations;
using Pair;
using System.Collections.Generic;
using Bot;

using Autofac;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Bot_Application2
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static bool OnlyOnce = true;
        private static string modelId = "20631dc0-8709-4963-9034-54817e872b35";
        private static int buildId = 1614213; // ID за замовчуванню
        public static string Name = "";
        public static int id = 2;
        private static string AccountKey = "836a85650c2446b38f435e48e8ffacc8";
        private const string BaseUri = "https://westus.api.cognitive.microsoft.com/recommendations/v4.0";
        public static RecommendationsApiWrapper recommender = null;
        public static List<Pair<int, int>> NumberAndID = new List<Pair<int, int>>();
        public static List<int> RememberBest = new List<int>();

        private async Task<string> GetStock(int ID)
        {
            string answer = "Here they are :)";
            var imdbID = await MovieBot.SearchMovieByID(ID);
            var list = imdbID.Results;
            int counter = 0;
            NumberAndID.Clear();
            foreach (SearchMovie movies in list)
            {
                ++counter;
                string ans = counter.ToString();
                answer = answer + Environment.NewLine + ans + ". " + movies.Title + " with rating " + movies.VoteAverage;
                Pair<int, int> adder = new Pair<int, int>(movies.Id, counter);
                NumberAndID.Add(adder);
            }
            if (imdbID == null)
            {
                return "This is not a valid movie genre ";
            }
            else
            {
                return answer;
            }
        }
        private static async Task<StockLUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            StockLUIS Data = new StockLUIS();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v2.0/apps/eacd21dd-a921-4b9b-90a9-66bb8ca02c67?subscription-key=a3e3368531d74171be894c9f4e092b10&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<StockLUIS>(JsonDataResponse);
                }
            }
            return Data;
        }
        private static APIJSON.APIJSON toJson(string user, int film)
        {
            Event eve = new Event();
            eve.eventType = "Purchase";
            eve.itemId = film.ToString();
            eve.timestamp = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss");
            eve.count = 1;
            eve.unitPrice = 1;
            Event[] e = new Event[] { eve };
            var convert = new APIJSON.APIJSON
            {
                userId = user,
                buildId = buildId,
                events = e
            };
            return convert;
        }
        public async Task<HttpResponseMessage> Post([FromBody]Activity message)
        {

            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            TMDbClient client = new TMDbClient("f345648a16265462c06f2d0136d0c9bd");
            var builder = new ContainerBuilder();

            builder.Register(c => new CachingBotDataStore(c.Resolve<ConnectorStore>(),
                CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Update(Conversation.Container);
            if (message.Type == "message" && message.Text[0] != '/')
            {
                
                string MovieString = "Sorry, I am not getting you...";
                StockLUIS StLUIS = await GetEntityFromLUIS(message.Text);
                
                if (StLUIS.topScoringIntent != null) 
                {
                    
                    if (StLUIS.topScoringIntent.intent == "Greeting")
                    {
                        StateClient stateClient = message.GetStateClient();   
                        BotData userData = await stateClient.BotState.GetUserDataAsync(message.ChannelId, message.From.Id);
                        string ActualName = userData.GetProperty<string>("Name");
                        if (userData.GetProperty<string>("Name") != null)
                        {
                            if (StLUIS.entities.Count() > 0)
                                MovieString = StLUIS.entities[0].entity + ", " + ActualName;
                            else
                                MovieString = "Hello, " + userData.Data;
                        }
                        else {
                            if (StLUIS.entities.Count() > 0)
                                MovieString = StLUIS.entities[0].entity + ", what's your name?";
                            else
                                MovieString = "Hello, what's your name?";
                        }
                    }
                    if (StLUIS.topScoringIntent.intent == "Name")
                    {
                        
                        foreach (Entity2 NameName in StLUIS.entities)
                        {
                            if (NameName.type == "Greeting")
                            {
                                Activity talk = message.CreateReply(StLUIS.entities[0].entity);
                                await connector.Conversations.ReplyToActivityAsync(talk);
                            }
                            if (NameName.type == "Name")
                            {
                                //userData.Data = NameName.entity;
                                StateClient stateClient = message.GetStateClient();
                                BotData userData = await stateClient.BotState.GetUserDataAsync(message.ChannelId, message.From.Id);
                                userData.SetProperty<string>("Name", NameName.entity);
                                await stateClient.BotState.SetUserDataAsync(message.ChannelId, message.From.Id, userData);
                            }
                            MovieString = "Nice to meet you, " + NameName.entity;
                        }
                    }
                    if (StLUIS.topScoringIntent.intent == "ChangeName")
                    {
                        StateClient stateClient = message.GetStateClient();
                        MovieString = "ok, so what's your name?";
                        await stateClient.BotState.DeleteStateForUserAsync(message.ChannelId, message.From.Id);
                    }
                    if (StLUIS.topScoringIntent.intent == "thank")
                    {
                        MovieString = "You are welcome";
                    }
                    if (StLUIS.topScoringIntent.intent == "None")
                    {
                            MovieString = "I'm sorry, i can't do this :(";

                    }
                    if (StLUIS.topScoringIntent.intent == "mood")
                    {
                        foreach (Entity2 Type in StLUIS.entities)
                        {
                            if (Type.type == "Greeting")
                            {
                                Activity talk = message.CreateReply(StLUIS.entities[0].entity);
                                await connector.Conversations.ReplyToActivityAsync(talk);
                            }
                        }
                        MovieString = "i think You should watch a film :)";
                    }
                    if (StLUIS.topScoringIntent.intent == "wazzap")
                    {
                        MovieString = "I'm fine";
                        foreach (Entity2 Type in StLUIS.entities)
                        {
                            if (Type.type == "Greeting")
                            {
                                Activity talk = message.CreateReply(StLUIS.entities[0].entity);
                                await connector.Conversations.ReplyToActivityAsync(talk);
                            }
                        }
                    }
                    int IDofMovie = 1;
                    if (StLUIS.topScoringIntent.intent == "WatchAFilm" || StLUIS.topScoringIntent.intent == "makeAList")
                    {
                            bool flag = false;
                            foreach (Entity2 Type in StLUIS.entities)
                            {
                                flag = false;
                                bool flagwazzap = false;
                                if (Type.type == "Greeting")
                                {
                                    Activity talk = message.CreateReply(StLUIS.entities[0].entity);
                                    await connector.Conversations.ReplyToActivityAsync(talk);
                                }
                                if (Type.type == "wazzap" && flagwazzap == false)
                                {
                                flagwazzap = true;
                                Activity talk = message.CreateReply("I'm fine");
                                await connector.Conversations.ReplyToActivityAsync(talk);
                                }
                            
                                if (Type.type == "Genre")
                                {

                                    foreach(Genre name in client.GetMovieGenresAsync("en").Result)
                                    {
                                        
                                        flag = true;
                                        if (name.Name[1] == Type.entity[1] && name.Name[2] == Type.entity[2])
                                        {
                                            IDofMovie = name.Id;
                                            break;
                                        }
                                    
                                    }
                                }
                                if (Type.type == "KindaAdventure")
                                {
                                    flag = true;
                                    Activity talk = message.CreateReply("I think adventure movie fits Your description. I will make a list of adventure movies for you");
                                    await connector.Conversations.ReplyToActivityAsync(talk);
                                    foreach (Genre name in client.GetMovieGenresAsync("en").Result)
                                    {

                                        if (name.Name == "Adventure")
                                        {
                                            IDofMovie = name.Id;
                                            break;
                                        }

                                    }
                                break;
                                }
                            
                                if (Type.type == "KindaCrime")
                                {
                                    flag = true;
                                    Activity talk = message.CreateReply("I think there are several genres that can satisfy Your needs. Mystery and Crime.");
                                    await connector.Conversations.ReplyToActivityAsync(talk);
                                    Activity talk2 = message.CreateReply("here is a list of crime films");
                                    await connector.Conversations.ReplyToActivityAsync(talk2);
                                    foreach (Genre name in client.GetMovieGenresAsync("en").Result)
                                    {

                                        if (name.Name == "Crime")
                                        {
                                            IDofMovie = name.Id;
                                            MovieString = await GetStock(IDofMovie);
                                            Activity reply = message.CreateReply(MovieString);
                                            await connector.Conversations.ReplyToActivityAsync(reply);
                                            break;
                                        }

                                    }
                                    Activity talk1 = message.CreateReply("here is a list of mystery films");
                                    await connector.Conversations.ReplyToActivityAsync(talk1);
                                    
                                    foreach (Genre name in client.GetMovieGenresAsync("en").Result)
                                    {

                                        if (name.Name == "Mystery")
                                        {
                                            IDofMovie = name.Id;
                                            MovieString = await GetStock(IDofMovie);
                                            break;
                                        }

                                    }
                                break;    
                                }
                            }
                        if(!flag)
                        {
                            Activity question = message.CreateReply(StLUIS.dialog.prompt);
                            await connector.Conversations.ReplyToActivityAsync(question);
                        }
                        else
                        {
                            
                                MovieString = await GetStock(IDofMovie);
                                Activity reply = message.CreateReply(MovieString);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                                Activity ask = message.CreateReply("I'd appreciate if you named the numbers of movies you liked");
                                await connector.Conversations.ReplyToActivityAsync(ask);
                            
                        }   
                    }
                    if (StLUIS.topScoringIntent.intent == "number")
                    {
                        recommender = new RecommendationsApiWrapper(AccountKey, BaseUri);
                        MovieString = "I can also recommend you some other films based on your choices";
                        Activity ask = message.CreateReply("oh, that will help me to learm something new :)");
                        await connector.Conversations.ReplyToActivityAsync(ask);
                        foreach (Entity2 Type in StLUIS.entities)
                        {
                            if (Type.type == "builtin.number")
                            {
                                RememberBest.Clear();
                                foreach(Pair<int, int> Id in NumberAndID)
                                {
                                    if (int.Parse(Type.entity) == Id.Second)
                                    {
                                        RememberBest.Add(Id.First);
                                        var row = toJson(message.From.Id, Id.First);
                                        string jsontext = JsonConvert.SerializeObject(row);
                                        recommender.AddUsageEvent(modelId, jsontext);
                                    }
                                }
                            }
                        }
                    }
                    if(StLUIS.topScoringIntent.intent == "Another")
                    {
                        recommender = new RecommendationsApiWrapper(AccountKey, BaseUri); 
                        MovieString = "Sorry that the previous films didn't satisfy you.                             " + Environment.NewLine + "Here are some other films you might like based on your previous choices                                  ";
                        string modelName = "RecommendationsForMovieBot";
                        try
                        {
                           
                            // Create a model if not already provided.
                            if (String.IsNullOrEmpty(modelId))
                            {
                                modelId = RecommendationsApp.CreateModel(modelName);
                            }
                            var itemSets = recommender.GetUserRecommendations(modelId, buildId, message.From.Id, 6);
                            if (itemSets.RecommendedItemSetInfo != null)
                            {
                                foreach (RecommendedItemSetInfo recoSet in itemSets.RecommendedItemSetInfo)
                                {
                                    foreach (var item in recoSet.Items)
                                    {
                                        MovieString = MovieString+ item.Name + "                     " + Environment.NewLine; // додавання пропусків необхідне для роботи в емуляторі, оскільки тільки так можна перейти в новий рядок
                                    }
                                }
                            }
                            else
                            {
                                MovieString = MovieString + "No recommendations found.";
                            }

                            recommender = RecommendationsApp.reco;

                        }
                        catch (Exception e)
                        {
                            Activity talking = message.CreateReply("Error encountered:");
                            await connector.Conversations.ReplyToActivityAsync(talking);
                            talking = message.CreateReply(e.Message);
                            await connector.Conversations.ReplyToActivityAsync(talking);
                        }
                        finally
                        {
                            //recommender.DeleteModel(modelId); 
                        }
                    }
                    if (StLUIS.topScoringIntent.intent == "Yes")
                    {
                        MovieString = "hope this will be usefull for you";
                        string modelName = "RecommendationsForMovieBot";
                        recommender = new RecommendationsApiWrapper(AccountKey, BaseUri); 
                        try
                        {
                            //recommender = new RecommendationsApiWrapper(AccountKey, BaseUri);
                            
                            // Create a model if not already provided.
                            if (String.IsNullOrEmpty(modelId))
                            {
                                modelId = RecommendationsApp.CreateModel(modelName);
                            }


                            
                            // Get item-to-item recommendations and user-to-item recommendations one at a time
                            string I2I = "People also like:                           " + Environment.NewLine, U2I = "You might also like:                         " + Environment.NewLine;
                            foreach (int IdOfBest in RememberBest)
                            {
                                RecommendationsApp.GetRecommendationsSingleRequest(recommender, modelId, buildId, ref I2I, ref U2I, IdOfBest, message.From.Id);
                            }
                            Activity talking = message.CreateReply(I2I);
                            await connector.Conversations.ReplyToActivityAsync(talking);
                            if (U2I == "You might also like:                         " + Environment.NewLine)
                            {
                                talking = message.CreateReply("I don't know you well enough :(");
                                await connector.Conversations.ReplyToActivityAsync(talking);
                            }
                            else
                            {
                                talking = message.CreateReply(U2I);
                                await connector.Conversations.ReplyToActivityAsync(talking);
                            }

                           
                            recommender = RecommendationsApp.reco;

                        }
                        catch (Exception e)
                        {
                            Activity talking = message.CreateReply("Error encountered:");
                            await connector.Conversations.ReplyToActivityAsync(talking);
                            talking = message.CreateReply(e.Message);
                            await connector.Conversations.ReplyToActivityAsync(talking);
                        }
                        finally
                        {
                            //recommender.DeleteModel(modelId); 
                        }
                    }
                }
                if (StLUIS.topScoringIntent.intent != "WatchAFilm" && StLUIS.topScoringIntent.intent != "makeAList")
                {

                    Activity reply = message.CreateReply(MovieString);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                   
                }

            }
            else
            {
                await HandleSystemMessageAsync(message);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            /*ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            Activity workpls = message.CreateReply("Something's wrong");
            await connector.Conversations.ReplyToActivityAsync(workpls);*/
            return null;
        }
    }
}