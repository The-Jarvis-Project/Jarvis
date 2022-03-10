using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

namespace Jarvis.API
{
    /// <summary>
    /// Class for creating a machine learning model using ML.NET.
    /// </summary>
    public class JarvisMLModel
    {
        /// <summary>
        /// Model type.
        /// </summary>
        public enum Type
        {
            Binary, Multiclass, Anomaly, Clustering, Ranking, Regression, Forecasting
        }

        private ITransformer model;
        private readonly IEstimator<ITransformer> estimator;
        private readonly TrainTestData split;
        private readonly string majorColumnName;
        private readonly Type type;
        private bool trained;

        /// <summary>
        /// Creates a JarvisMLModel that can be used to create and utilize neural networks and regressions.
        /// </summary>
        /// <param name="data">The data to base the model off of</param>
        /// <param name="estimator">Estimator used for the model</param>
        /// <param name="type">Type of machine learning model</param>
        /// <param name="majorColumnName">The major column name of the processed data</param>
        /// <param name="testPercent">The percentage of the data to test against</param>
        public JarvisMLModel(IDataView data, IEstimator<ITransformer> estimator, Type type,
            string majorColumnName = "Label", double testPercent = 0.2)
        {
            trained = false;
            this.type = type;
            this.estimator = estimator;
            this.majorColumnName = majorColumnName;
            split = Jarvis.mlContext.Data.TrainTestSplit(data, testPercent);
        }

        /// <summary>
        /// Accuracy for the given model (may be incorrect).
        /// </summary>
        public double Accuracy { get; private set; }

        /// <summary>
        /// How well the model fits the data (may be incorrect and may be different depending on model type).
        /// </summary>
        public double FittingScore { get; private set; }

        /// <summary>
        /// Trains the estimator on the data.
        /// </summary>
        public void Train()
        {
            if (!trained)
            {
                model = estimator.Fit(split.TrainSet);
                SetupMetrics();
                trained = true;
            }
        }

        private void SetupMetrics()
        {
            if (type == Type.Binary)
            {
                CalibratedBinaryClassificationMetrics m =
                        Jarvis.mlContext.BinaryClassification.Evaluate(model.Transform(split.TestSet), majorColumnName);
                Accuracy = m.Accuracy;
                FittingScore = m.F1Score;
            }
            else if (type == Type.Multiclass)
            {
                MulticlassClassificationMetrics m =
                        Jarvis.mlContext.MulticlassClassification.Evaluate(model.Transform(split.TestSet), majorColumnName);
                Accuracy = m.MacroAccuracy;
                FittingScore = m.LogLossReduction;
            }
            else if (type == Type.Anomaly)
            {
                AnomalyDetectionMetrics m =
                        Jarvis.mlContext.AnomalyDetection.Evaluate(model.Transform(split.TestSet), majorColumnName);
                Accuracy = m.DetectionRateAtFalsePositiveCount;
            }
            else if (type == Type.Clustering)
            {
                ClusteringMetrics m =
                        Jarvis.mlContext.Clustering.Evaluate(model.Transform(split.TestSet), majorColumnName);
                Accuracy = m.AverageDistance;
                FittingScore = m.DaviesBouldinIndex;
            }
            else if (type == Type.Regression)
            {
                RegressionMetrics m =
                        Jarvis.mlContext.Regression.Evaluate(model.Transform(split.TestSet), majorColumnName);
                FittingScore = m.RSquared;
            }
        }

        private PredictionEngine<TSrc, TDst> PredictEngine<TSrc, TDst>(ITransformer model)
            where TSrc : class where TDst : class, new() =>
            Jarvis.mlContext.Model.CreatePredictionEngine<TSrc, TDst>(model);

        /// <summary>
        /// Evaluates a binary model.
        /// </summary>
        /// <typeparam name="T">The input data value type</typeparam>
        /// <param name="item">The input data</param>
        /// <returns>A binary (true / false) prediction and the confidence in that prediction</returns>
        public BinaryPrediction EvaluateBinary<T>(T item) where T : class
        {
            if (trained) return PredictEngine<T, BinaryPrediction>(model).Predict(item);
            return null;
        }

        /// <summary>
        /// Evaluates a multiclass model.
        /// </summary>
        /// <typeparam name="T">The input data value type</typeparam>
        /// <param name="item">The input data</param>
        /// <returns>The class of that the input data falls into</returns>
        public MulticlassPrediction EvaluateMulticlass<T>(T item) where T : class
        {
            if (trained) return PredictEngine<T, MulticlassPrediction>(model).Predict(item);
            return null;
        }

        /// <summary>
        /// The result from a binary prediction evaluation.
        /// </summary>
        public class BinaryPrediction
        {
            /// <summary>
            /// The prediction made.
            /// </summary>
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }

            /// <summary>
            /// Probability the prediction is correct.
            /// </summary>
            public float Probability { get; set; }

            /// <summary>
            /// Score when evaluated in the model.
            /// </summary>
            public float Score { get; set; }
        }

        /// <summary>
        /// The result from a multiclass prediction evaluation.
        /// </summary>
        public class MulticlassPrediction
        {
            [ColumnName("PredictedLabel")]
            public string Area { get; set; }
        }

    }
}
