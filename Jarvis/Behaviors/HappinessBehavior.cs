using Jarvis.API;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Jarvis.Behaviors
{
    public class HappinessBehavior : IStart, IWebUpdate
    {
        public bool Enabled { get; set; } = false;
        public int Priority => 0;

        private static readonly string dataPath = @"C:\Users\rex\Downloads\train.csv";

        private JarvisMLModel model;

        public void Start()
        {
            IDataView dataView = MLSystem.LoadCsv<HappinessData>(dataPath, true, true, true);

            IEstimator<ITransformer> estimator = Jarvis.mlContext.Transforms.Text.NormalizeText("FeaturesA", "SentimentText")
                .Append(MLSystem.Featurize("FeaturesB", "FeaturesA"))
                .Append(Jarvis.mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "FeaturesB"));

            model = new JarvisMLModel(dataView, estimator, JarvisMLModel.Type.Binary, "Label");
            model.Train();

            Log.Warning("Accuracy: " + model.Accuracy);
        }

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            if (requests.Length > 0 && model != null)
            {
                HappinessData data = new HappinessData
                {
                    SentimentText = requests[0].Request
                };
                var happy = model.EvaluateBinary(data);
                string msg;
                Log.Warning(happy.Probability.ToString());
                if (happy.Prediction) msg = "Happy";
                else msg = "Sad";
                ComSystem.SendResponse(msg, ResponseType.Text, requests[0].Id);
            }
        }

        public class HappinessData
        {
            [LoadColumn(2), ColumnName("SentimentText")]
            public string SentimentText;

            [LoadColumn(1), ColumnName("Label")]
            public bool Sentiment;
        }
    }
}
