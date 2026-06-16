using Microsoft.ML;
using Microsoft.ML.Data;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Services;

public class AvaliacaoData
{
    public string Texto { get; set; } = string.Empty;
    public string Sentimento { get; set; } = string.Empty;
}

public class AvaliacaoPrediction
{
    [ColumnName("PredictedLabel")]
    public string SentimentoPrevisto { get; set; } = string.Empty;
}

/// <summary>
/// Serviço de análise de sentimento utilizando ML.NET.
/// </summary>
public class AnaliseSentimentoService : IAnaliseSentimentoService
{
    private readonly MLContext _mlContext;
    private ITransformer _modelo;

    public AnaliseSentimentoService()
    {
        _mlContext = new MLContext();
        _modelo = TreinarModelo(); 
    }

    private ITransformer TreinarModelo()
    {
        var dadosTreinamento = new List<AvaliacaoData>
        {
            new() { Texto = "O produto é excelente, amei demais!", Sentimento = "Positivo" },
            new() { Texto = "Chegou super rápido, recomendo.", Sentimento = "Positivo" },
            new() { Texto = "Muito bom, material de qualidade.", Sentimento = "Positivo" },
            new() { Texto = "Horrível, quebrou no primeiro dia.", Sentimento = "Negativo" },
            new() { Texto = "Odiei, a caixa veio rasgada e faltou peça.", Sentimento = "Negativo" },
            new() { Texto = "Péssimo atendimento e produto ruim.", Sentimento = "Negativo" },
            new() { Texto = "É ok, pelo preço vale a pena.", Sentimento = "Neutro" },
            new() { Texto = "Normal, nada demais.", Sentimento = "Neutro" },
            new() { Texto = "Veio como descrito, mas não me surpreendeu.", Sentimento = "Neutro" }
        };

        var dadosView = _mlContext.Data.LoadFromEnumerable(dadosTreinamento);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Sentimento", outputColumnName: "Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Texto", outputColumnName: "Features"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        return pipeline.Fit(dadosView);
    }

    public Task<string> AnalisarSentimentoAsync(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return Task.FromResult("Neutro");

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<AvaliacaoData, AvaliacaoPrediction>(_modelo);
        var predicao = predictionEngine.Predict(new AvaliacaoData { Texto = texto });

        return Task.FromResult(predicao?.SentimentoPrevisto ?? "Neutro");
    }
}