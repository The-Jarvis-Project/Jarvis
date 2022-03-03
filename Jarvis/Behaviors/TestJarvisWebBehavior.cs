using Jarvis.API;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

namespace Jarvis.Behaviors
{
    public class TestJarvisWebBehavior : IWebUpdate, IStart
    {
        public bool Enabled { get; set; } = true;
        public int Priority => 100;

        private static readonly string dataPath = 
            @"C:\Users\rexjw\Downloads\yelp_labelled.txt";

        public ITransformer model;

        public void Start()
        {
            Log.Warning("Started Test Behavior");

            IDataView dataView = Jarvis.mlContext.Data.LoadFromTextFile<SentimentData>(dataPath, hasHeader: false);

            TrainTestData splitDataView = Jarvis.mlContext.Data.TrainTestSplit(dataView, 0.2);

            IEstimator<ITransformer> estimator = Jarvis.mlContext.Transforms.Text.FeaturizeText("Features", "SentimentText")
                .Append(Jarvis.mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            Log.Warning("Training Model");

            model = estimator.Fit(splitDataView.TrainSet);
            
            Log.Warning("Finished Training");
            
            IDataView predictions = model.Transform(splitDataView.TestSet);
            CalibratedBinaryClassificationMetrics metrics = Jarvis.mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Log.Warning("Model Accuracy: " + metrics.Accuracy +
                "\nArea Under Curve: " + metrics.AreaUnderRocCurve +
                "\nF1 Score: " + metrics.F1Score);
        }

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            if (requests.Length > 0 && model != null)
            {
                SentimentData requestText = new SentimentData()
                {
                    SentimentText = requests[0].Request
                };

                PredictionEngine<SentimentData, SentimentPrediction> predictFunc =
                    Jarvis.mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
                SentimentPrediction prediction = predictFunc.Predict(requestText);

                string msg;
                if (prediction.Prediction) msg = "Positive";
                else msg = "Negative";
                msg += "    Probability: " + (prediction.Probability * 100);

                ComSystem.SendResponse(msg, ResponseType.Text, requests[0].Id);
            }
            if (model == null) Log.Warning("null model");
        }

        public class SentimentData
        {
            [LoadColumn(0), ColumnName("SentimentText")]
            public string SentimentText;

            [LoadColumn(1), ColumnName("Label")]
            public bool Sentiment;
        }

        public class SentimentPrediction : SentimentData
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }

            public float Probability { get; set; }

            public float Score { get; set; }
        }
    }
}
