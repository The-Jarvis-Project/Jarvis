using Microsoft.ML;
using System.Linq;

namespace Jarvis.API
{
    /// <summary>
    /// Convenience functions for machine learning.
    /// </summary>
    public static class MLSystem
    {
        /// <summary>
        /// Featurizes the text into an array of floating-point values.
        /// </summary>
        /// <param name="outputColumn">The output column's name</param>
        /// <param name="inputColumn">The input column's name</param>
        /// <returns>An estimator transformer of the featurized text</returns>
        public static IEstimator<ITransformer> Featurize(string outputColumn, string inputColumn) =>
            Jarvis.mlContext.Transforms.Text.FeaturizeText(outputColumn, inputColumn);

        /// <summary>
        /// Normalizes the text and then featurizes the normalized text.
        /// </summary>
        /// <param name="outputColumn">The output column's name</param>
        /// <param name="inputColumn">The input column's name</param>
        /// <returns>An estimator transformer of the normalized featurized text</returns>
        public static IEstimator<ITransformer> NormalizeFeaturize(string outputColumn, string inputColumn) =>
            Jarvis.mlContext.Transforms.Text.NormalizeText(outputColumn + "2", inputColumn)
            .Append(Jarvis.mlContext.Transforms.Text.FeaturizeText(outputColumn, outputColumn + "2"));

        /// <summary>
        /// Loads a .txt or .text file into an IDataView.
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="dataPath">The file path of the data</param>
        /// <param name="separator">The separator charactor used to separate data columns</param>
        /// <param name="header">Whether or not the data has a header</param>
        /// <param name="quotes">Whether or not the file has quotation marks around the data</param>
        /// <param name="trimSpace">Whether or not white space should be trimmed from the data</param>
        /// <returns>An IDataView object</returns>
        public static IDataView LoadTxt<T>(string dataPath, char separator = '\t',
            bool header = false, bool quotes = false, bool trimSpace = false) =>
            Jarvis.mlContext.Data.LoadFromTextFile<T>(dataPath, separator, header, quotes, trimSpace);

        /// <summary>
        /// Loads a .csv file into an IDataView.
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="dataPath">The file path of the data</param>
        /// <param name="quotes">Whether or not the file has quotation marks around the data</param>
        /// <param name="trimSpace">Whether or not white space should be trimmed from the data</param>
        /// <param name="header">Whether or not the data has a header</param>
        /// <returns>An IDataView object</returns>
        public static IDataView LoadCsv<T>(string dataPath,
            bool quotes = false, bool trimSpace = true, bool header = true) =>
            Jarvis.mlContext.Data.LoadFromTextFile<T>(dataPath, ',', header, quotes, trimSpace);
    }
}
